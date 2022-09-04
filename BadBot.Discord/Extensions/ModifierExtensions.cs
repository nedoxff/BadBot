using BadBot.Processor;
using BadBot.Processor.Models.Modifiers;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Serilog;

namespace BadBot.Discord.Extensions;

public static class ModifierExtensions
{
    public static void AttachDiscordEvents(this Modifier modifier, InteractionContext ctx, DiscordMessage message)
    {
        var statusBuilder = new StatusMessageBuilder();

        modifier.OnWarning += async str =>
        {
            Log.Warning($"[Request] {str}");
            if (message == null) return;
            statusBuilder = statusBuilder.WithWarning(str);
            await message.ModifyAsync(statusBuilder.Build());
        };
        modifier.OnError += async str =>
        {
            Log.Error($"[Request] {str}");
            if (message == null) return;
            statusBuilder = statusBuilder.WithError(str);
            await message.ModifyAsync(statusBuilder.Build());
        };
        modifier.OnInfo += async str =>
        {
            Log.Debug($"[Request] {str}");
            if (message == null) return;
            statusBuilder = statusBuilder.WithInfo(str);
            await message.ModifyAsync(statusBuilder.Build());
        };
        modifier.OnStatusChange += async str =>
        {
            if (message == null) return;
            statusBuilder = statusBuilder.WithStatus(str);
            await message.ModifyAsync(statusBuilder.Build());
        };
        modifier.OnFinished += async modifier =>
        {
#if !DEBUG
            if (message != null)
                await message.DeleteAsync();
#endif
            try
            {
                var path = Path.Join(modifier.WorkingDirectory, modifier.OutputFile);
                if (new FileInfo(path).Length < 8000000) //8MB
                {
                    var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder()
                        .WithFile(modifier.Id + ".mp4", stream));
                    stream.Close();
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Файл весит больше 8МБ, я (пока что) не могу его отправить.");
                }
            }
            catch (Exception e)
            {
                await ctx.FollowUpAsync(
                    new DiscordFollowupMessageBuilder().WithContent("Произошла ошибка при отправке видео!"));
                modifier.Error(e.ToString());
            }
            finally
            {
                RequestQueue.Cleanup(modifier);
            }
        };

        modifier.Info("Attached Discord events");
    }
}