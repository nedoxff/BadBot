namespace BadBot.Processor.Models.Requests;

public abstract class Request
{
    public string Url { get; set; }
    public ulong UserId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong GuildId { get; set; }
    public string Id { get; set; }
    public abstract string ModifierType { get; set; }
}