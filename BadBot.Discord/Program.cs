// See https://aka.ms/new-console-template for more information

using BadBot.Discord;
using BadBot.Processor;
using dotenv.net;
using Serilog;
using Serilog.Core;

DotEnv.Load();
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("latest_log.txt")
    .WriteTo.Console()
#if DEBUG
    .MinimumLevel.Debug()
#else
    .MinimumLevel.Information()
#endif
    .CreateLogger();
Log.Information("Initialized logger");

RequestQueue.Start();
Client.Start();
Console.ReadLine();
