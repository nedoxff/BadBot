using DSharpPlus.Entities;

namespace BadBot.Discord;

public class StatusMessageBuilder
{
    private const int MaxStatusLength = 635;
    private readonly List<string> _errors = new();
    private readonly List<string> _info = new();
    private readonly List<string> _warnings = new();
    private string _status;

    public StatusMessageBuilder WithInfo(string str)
    {
        _info.Add($"[{DateTime.Now:g}] {str}");
        return this;
    }

    public StatusMessageBuilder WithError(string str)
    {
        _errors.Add($"[{DateTime.Now:g}] {str}");
        return this;
    }

    public StatusMessageBuilder WithWarning(string str)
    {
        _warnings.Add($"[{DateTime.Now:g}] {str}");
        return this;
    }

    public StatusMessageBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public DiscordEmbed Build()
    {
        var description = $@"Сообщения
```
{(_info.Any() ? new string(string.Join("\n", _info).TakeLast(MaxStatusLength).ToArray()) : " ")}
```
Предупреждения
```
{(_warnings.Any() ? new string(string.Join("\n", _warnings).TakeLast(MaxStatusLength).ToArray()) : " ")}
```
Ошибки
```
{(_errors.Any() ? new string(string.Join("\n", _errors).TakeLast(MaxStatusLength).ToArray()) : " ")}
```
Статус: `{(string.IsNullOrEmpty(_status) ? "Unknown" : _status)}`";
        var builder = new DiscordEmbedBuilder()
            .WithDescription(description)
            .WithColor(DiscordColor.Gold)
            .WithTimestamp(DateTime.Now);
        return builder.Build();
    }
}