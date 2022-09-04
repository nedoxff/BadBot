using BadBot.Processor.Models.Requests;
using DSharpPlus.SlashCommands;

namespace BadBot.Discord.Commands.Modifiers;

public class Pitch : AdvancedCommandModule
{
    [SlashCommand("pitch", "Изменить тон аудио (в видео)")]
    public async Task Execute(InteractionContext ctx,
        [Choice("Ниже", 0.75)]
        [Choice("Низкий", 0.5)]
        [Choice("Очень низкий", 0.25)]
        [Choice("Выше", 1.25)]
        [Choice("Высокий", 1.5)]
        [Choice("Очень высокий", 1.75)]
        [Option("amplifier", "Насколько изменить тон аудио")]
        double amplifier, [Option("url", "Ссылка на видео")] string url = "")
    {
        var requestUrl = await WaitForUrl(ctx, url);
        if (string.IsNullOrEmpty(requestUrl))
            return;
        await CreateRequest(ctx, new PitchRequest
        {
            Url = requestUrl,
            Amplifier = amplifier
        });
    }
}