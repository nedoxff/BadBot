using BadBot.Processor.Extensions;
using BadBot.Processor.Models.Requests;

namespace BadBot.Processor.Models.Modifiers;

public abstract class Modifier
{
    private string _status;
    public string Name { get; set; }
    public string Id { get; set; }
    public string WorkingDirectory { get; set; }
    public Request RawRequest { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Messages { get; set; } = new();

    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnStatusChange?.Invoke(value);
        }
    }

    public string InputFile { get; set; }
    public string OutputFile { get; set; }

    public abstract Task<ModifierResult> OnProcess();
    public event Action<string> OnWarning;
    public event Action<string> OnError;
    public event Action<string> OnInfo;
    public event Action<string> OnStatusChange;
    public event Action<Modifier> OnFinished;

    public void Finish()
    {
        OnFinished?.Invoke(this);
    }

    public void Warning(string msg)
    {
        Warnings.Add(msg);
        OnWarning?.Invoke(msg);
    }

    public void Error(string msg)
    {
        Errors.Add(msg);
        OnError?.Invoke(msg);
    }

    public void Info(string msg)
    {
        Messages.Add(msg);
        OnInfo?.Invoke(msg);
    }
}

public abstract class Modifier<T> : Modifier where T : Request
{
    public T Request => RawRequest.To<T>() ?? throw new Exception($"Could not cast Request to {typeof(T).Name}");
}