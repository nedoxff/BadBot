using BadBot.Processor.Modifiers;

namespace BadBot.Processor.Models;

public class Request
{
    public string Url { get; set; }
    public ulong UserId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong GuildId { get; set; }
    public string Id { get; set; }
    public IModifier Modifier { get; set; }
}