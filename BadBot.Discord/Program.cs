// See https://aka.ms/new-console-template for more information

using BadBot.Discord;
using BadBot.Processor;
using BadBot.Processor.Helpers;
using dotenv.net;
using Serilog;
using Serilog.Events;

if (!Directory.Exists("WorkerLogs"))
    Directory.CreateDirectory("WorkerLogs");
DotEnv.Load();
var env = DotEnv.Read();
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("latest_log.txt")
    .WriteTo.RequestSink()
    .WriteTo.Console()
    .MinimumLevel.Is(env.ContainsKey("DEBUG") && env["DEBUG"] == "true"
        ? LogEventLevel.Debug
        : LogEventLevel.Information)
    .CreateLogger();
Log.Information("Initialized logger");

RequestQueue.Start();
Client.Start();
Console.ReadLine();