using System.Diagnostics;
using BadBot.Processor.Extensions;
using BadBot.Processor.Models.Modifiers;
using BadBot.Processor.Models.Requests;
using MimeTypes;
using Serilog;
using Serilog.Context;

namespace BadBot.Processor;

public static class RequestQueue
{
    private static readonly Queue<Modifier> _requests = new();
    private static readonly Dictionary<string, ModifierResult> _results = new();
    private static readonly HttpClient _client = new();

    private static bool _started;
    public static event Action<string> RequestStarted;
    public static event Action RequestFinished;

    public static Modifier Add(Request request)
    {
        var id = Guid.NewGuid().ToString("N");
        request.Id = id;
        var modifier = request.ToModifier();
        _requests.Enqueue(modifier);
        Log.Information("Added new request with ID {Id}", id);
        return modifier;
    }

    public static void Start()
    {
        if (_started)
            throw new Exception("RequestQueue was already started!");
        RequestExtensions.RegisterModifiers();
        Task.Factory.StartNew(() => StartAsync().GetAwaiter().GetResult());
        Log.Information("Started the request queue");
    }

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

    private static async Task DownloadFile(Modifier modifier)
    {
        Log.Debug("Downloading file from {Url} for request {Id}", modifier.RawRequest.Url, modifier.RawRequest.Id);
        var linkType = modifier.RawRequest.Url.GetLinkType();
        switch (linkType)
        {
            case LinkType.Invalid:
                break;
            case LinkType.Youtube:
                modifier.Info("Using yt-dlp to download the file");
                var pci = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments =
                        $"{modifier.RawRequest.Url} --merge-output-format mp4 -o \"{Path.Join(modifier.WorkingDirectory, "input.mp4")}\""
                };
                var process = Process.Start(pci);
                await process!.WaitForExitAsync();
                modifier.InputFile = Path.Join(modifier.WorkingDirectory, "input.mp4");
                break;
            case LinkType.Other:
                modifier.Info("Using HttpClient to download the file");
                var response = await _client.GetAsync(modifier.RawRequest.Url);
                if (!response.IsSuccessStatusCode)
                    throw new Exception(
                        $"Trying to download a file from {modifier.RawRequest.Url} resulted in a non-successful HTTP code ({response.StatusCode})");
                var responseType = response.Content.Headers.ContentType?.MediaType;
                if (string.IsNullOrEmpty(responseType) ||
                    (!responseType.StartsWith("video") && !responseType.StartsWith("image")))
                    throw new Exception(
                        $"Trying to download a file from {modifier.RawRequest.Url} resulted in a non-expected content type ({(string.IsNullOrEmpty(responseType) ? "not specified" : responseType)})");
                var saveTo = Path.Join(modifier.WorkingDirectory, "input" + MimeTypeMap.GetExtension(responseType));
                await File.WriteAllBytesAsync(saveTo, await response.Content.ReadAsByteArrayAsync());
                modifier.InputFile = saveTo;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void Cleanup(Modifier modifier)
    {
        Log.Debug("Removing worker directory");
#if !DEBUG
        Directory.Delete(modifier.WorkingDirectory, true);
#endif
        Ffmpeg.Ffmpeg.KillProcesses();
    }

    private static async Task ProcessRequest(Modifier modifier)
    {
        var requestId = modifier.RawRequest.Id;
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
            modifier.Error(e.Message);
            Log.Error("Failed to process request with ID {ID}: {Exception}", requestId, e.Message);
        }
        finally
        {
            result!.Ended = DateTime.Now;
            _results[requestId] = result;

            Log.Information("Finished request with ID {ID}", requestId);
            modifier.Finish();
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

    public static bool HasResult(string id)
    {
        return _results.ContainsKey(id);
    }
}