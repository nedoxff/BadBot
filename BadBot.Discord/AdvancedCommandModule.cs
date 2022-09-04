using BadBot.Discord.Extensions;
using BadBot.Processor;
using BadBot.Processor.Extensions;
using BadBot.Processor.Models.Requests;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace BadBot.Discord;

public class AdvancedCommandModule : ApplicationCommandModule
{
    public async Task CreateRequest<T>(InteractionContext ctx, T request) where T : Request
    {
        request.ChannelId = ctx.Channel.Id;
        request.GuildId = ctx.Guild.Id;
        request.UserId = ctx.User.Id;
        var modifier = RequestQueue.Add(request);

        DiscordMessage statusMessage = null;
        if (Client.StatusChannelId != null && ctx.Guild.Channels.ContainsKey(Client.StatusChannelId.Value))
        {
            var channel = ctx.Guild.Channels[Client.StatusChannelId.Value];
            statusMessage = await channel.SendMessageAsync($"Здесь будет статус для запроса с ID `{modifier.Id}`");
        }

        var embed = new DiscordEmbedBuilder()
            .WithDescription($"```ansi\nЗапрос с ID [0;32m{modifier.Id}[0m был успешно создан!\n```")
            .WithColor(DiscordColor.Green)
            .WithTimestamp(DateTime.Now);

        if (statusMessage != null)
            embed = embed.AddField("Статус", statusMessage.JumpLink.ToString());
        modifier.AttachDiscordEvents(ctx, statusMessage);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(embed));
    }

    public async Task<string> WaitForUrl(InteractionContext ctx, string url)
    {
        if (!string.IsNullOrEmpty(url))
            return url;
        var interacivity = ctx.Client.GetInteractivity();
        await ctx.DeferAsync();
        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder().WithContent(
                $"{ctx.User.Mention}, я жду от тебя картинку или видео. (30 секунд)"));
        var result = await interacivity.WaitForMessageAsync(m =>
                m.Author.Id == ctx.User.Id && (m.Attachments.Count != 0 || m.Content.IsValidUrl()),
            TimeSpan.FromSeconds(30));
        if (result.TimedOut)
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Время вышло!"));
            return null;
        }

        return result.Result.Attachments.Any() ? result.Result.Attachments[0].Url : result.Result.Content;
    }
}