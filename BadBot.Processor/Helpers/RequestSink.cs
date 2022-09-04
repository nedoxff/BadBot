using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace BadBot.Processor.Helpers;

public class RequestSink : ILogEventSink
{
    private const string DefaultOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    private readonly IFormatProvider _formatProvider;
    private readonly MessageTemplateTextFormatter _formatter;
    private readonly string _outputTemplate;
    private StreamWriter _stream;

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

    public void Emit(LogEvent logEvent)
    {
        if (_stream != null)
            _formatter.Format(logEvent, _stream);
    }
}

public static class RequestSinkExtensions
{
    public static LoggerConfiguration RequestSink(this LoggerSinkConfiguration configuration,
        IFormatProvider provider = null)
    {
        return configuration.Sink(new RequestSink(provider));
    }
}