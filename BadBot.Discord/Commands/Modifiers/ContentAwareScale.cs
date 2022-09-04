using BadBot.Processor.Models.Requests;
using DSharpPlus.SlashCommands;

namespace BadBot.Discord.Commands.Modifiers;

public class ContentAwareScale : AdvancedCommandModule
{
    [SlashCommand("cas", "Жмыхнуть видео")]
    public async Task Execute(InteractionContext ctx, [Option("url", "Ссылка на видео")] string url = "")
    {
        var requestUrl = await WaitForUrl(ctx, url);
        if (string.IsNullOrEmpty(requestUrl))
            return;
        await CreateRequest(ctx, new ContentAwareScaleRequest
        {
            Url = requestUrl
        });
    }
}