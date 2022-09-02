using BadBot.Processor.Models;
using BadBot.Processor.Modifiers;
using MimeTypes;
using Serilog;
using Serilog.Context;

namespace BadBot.Processor;

public static class RequestQueue
{
    private static Queue<IModifier> _requests = new();
    private static Dictionary<string, ModifierResult> _results = new();
    public static event Action<string> RequestStarted;
    public static event Action RequestFinished;
    private static HttpClient _client = new();
    
    public static string Add(Request request)
    {
        var id = Guid.NewGuid().ToString("N");
        request.Id = id;
        _requests.Enqueue(request.Modifier);
        Log.Information("Added new request with ID {Id}", id);
        return id;
    }

    public static void Start()
    {
        if (_started)
            throw new Exception("RequestQueue was already started!");
        Task.Factory.StartNew(() => StartAsync().GetAwaiter().GetResult());
        Log.Information("Started the request queue");
    }

    private static bool _started;
    private static async Task StartAsync()
    {
        _started = true;
        while (true)
        {
            while (_requests.Count == 0)
                await Task.Delay(1);

            var modifier = _requests.Dequeue();
            await ProcessRequest(modifier);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static async Task DownloadFile(IModifier modifier)
    {
        Log.Debug("Downloading file from {Url} for request {Id}", modifier.Request.Url, modifier.Request.Id);
        var response = await _client.GetAsync(modifier.Request.Url);
        if (!response.IsSuccessStatusCode)
            throw new Exception(
                $"Trying to download a file from {modifier.Request.Url} resulted in a non-successful HTTP code ({response.StatusCode})");
        var responseType = response.Content.Headers.ContentType?.MediaType;
        if (string.IsNullOrEmpty(responseType) ||
            (!responseType.StartsWith("video") && !responseType.StartsWith("image")))
            throw new Exception(
                $"Trying to download a file from {modifier.Request.Url} resulted in a non-expected content type ({(string.IsNullOrEmpty(responseType) ? "not specified" : responseType)})");
        var saveTo = Path.Join(modifier.WorkingDirectory, "input" + MimeTypeMap.GetExtension(responseType));
        await File.WriteAllBytesAsync(saveTo, await response.Content.ReadAsByteArrayAsync());
    }

    private static async Task Cleanup(IModifier modifier)
    {
        Log.Debug("Removing worker directory");
        //Directory.Delete(modifier.WorkingDirectory, true);
    }

    private static async Task ProcessRequest(IModifier modifier)
    {
        var requestId = modifier.Request.Id;
        RequestStarted?.Invoke(requestId);
        
        using var handle = LogContext.PushProperty("RequestId", requestId);

        Log.Information("Processing request with ID {Id}", requestId);
        var started = DateTime.Now;

        var path = Path.Join(Path.GetTempPath(), "BadBotWorker_" + requestId);
        Directory.CreateDirectory(path);
        modifier.WorkingDirectory = path;
        Log.Debug("Worker directory is {WorkerDirectory}", path);

        ModifierResult result = null!;
        try
        {
            await DownloadFile(modifier);
            
            result = await modifier.OnProcess();
            result.Started = started;
        }
        catch (Exception e)
        {
            result = new ModifierResult
            {
                Started = started,
                Success = false,
                Message = e.Message
            };
            Log.Error("Failed to process request with ID {ID}: {Exception}", requestId, e.Message);
        }
        finally
        {
            Log.Debug("Performing cleanup");
            await Cleanup(modifier);

            result!.Ended = DateTime.Now;
            _results[requestId] = result;

            Log.Information("Finished request with ID {ID}", requestId);
            RequestFinished?.Invoke();
        }
    }

    public static void FinishRequest(string id)
    {
        if (_results[id].Success)
        {
            var path = Path.Join(Environment.CurrentDirectory, "WorkerLogs", id + ".txt");
            File.Delete(path);
        }
        _results.Remove(id);
        Log.Information("Removed request with ID {ID} from queue", id);
    }

    public static bool HasResult(string id) => _results.ContainsKey(id);
}