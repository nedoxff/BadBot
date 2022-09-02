using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace BadBot.Processor.Helpers;

public class RequestSink: ILogEventSink
{
    private readonly IFormatProvider _formatProvider;
    private StreamWriter _stream;
    private readonly string _outputTemplate;
    private MessageTemplateTextFormatter _formatter;
    private const string DefaultOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    
    public RequestSink(IFormatProvider formatProvider, string outputTemplate = DefaultOutputTemplate)
    {
        _formatProvider = formatProvider;
        _outputTemplate = outputTemplate;
        _formatter = new MessageTemplateTextFormatter(_outputTemplate, _formatProvider);
        
        RequestQueue.RequestStarted += id =>
        {
            var path = Path.Join(Environment.CurrentDirectory, "WorkerLogs", id + ".txt");
            _stream = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write));
        };

        RequestQueue.RequestFinished += () => _stream.Close();
    }

    public void Emit(LogEvent logEvent) => _formatter.Format(logEvent, _stream);
}

public static class RequestSinkExtensions
{
    public static LoggerConfiguration RequestSink(this LoggerSinkConfiguration configuration,
        IFormatProvider provider = null)
    {
        return configuration.Sink(new RequestSink(provider));
    }
}