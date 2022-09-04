using System.Reflection;
using dotenv.net;
using DSharpPlus;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BadBot.Discord;

public class Client
{
    private static DiscordClient _client;
    private static bool _started;
    public static ulong? StatusChannelId;

    public static void Start()
    {
        if (_started)
            throw new Exception("The discord client was already started!");
        _started = true;
        Task.Factory.StartNew(() => StartAsync().GetAwaiter().GetResult());
        Log.Information("Starting the Discord client..");
    }

    private static async Task StartAsync()
    {
        var env = DotEnv.Read();
        _client = new DiscordClient(new DiscordConfiguration
        {
            Token = env["TOKEN"],
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            LoggerFactory = new LoggerFactory().AddSerilog()
        });

        var slashCommands = _client.UseSlashCommands();
        slashCommands.RegisterCommands(Assembly.GetExecutingAssembly(),
            env.ContainsKey("GUILD_ID") ? ulong.Parse(env["GUILD_ID"]) : null);
        Log.Debug("Initialized slash commands");

        StatusChannelId = env.ContainsKey("STATUS_CHANNEL_ID") ? ulong.Parse(env["STATUS_CHANNEL_ID"]) : null;

        _client.UseInteractivity();
        Log.Debug("Initialized interactivity");

        await _client.ConnectAsync();
        await Task.Delay(-1);
    }
}