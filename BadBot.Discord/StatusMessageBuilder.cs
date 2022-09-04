using DSharpPlus.Entities;

namespace BadBot.Discord;

public class StatusMessageBuilder
{
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
{(_info.Any() ? string.Join("\n", _info) : " ")}
```
Предупреждения
```
{(_warnings.Any() ? string.Join("\n", _warnings) : " ")}
```
Ошибки
```
{(_errors.Any() ? string.Join("\n", _errors) : " ")}
```
Статус: `{(string.IsNullOrEmpty(_status) ? "Unknown" : _status)}`";
        var builder = new DiscordEmbedBuilder()
            .WithDescription(description)
            .WithColor(DiscordColor.Gold)
            .WithTimestamp(DateTime.Now);
        return builder.Build();
    }
}