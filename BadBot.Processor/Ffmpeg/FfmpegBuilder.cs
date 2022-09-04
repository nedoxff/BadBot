using BadBot.Processor.Models.Modifiers;

namespace BadBot.Processor.Ffmpeg;

public class FfmpegBuilder
{
    private readonly List<FfmpegCommand> _commands = new();

    public FfmpegBuilder(Modifier modifier)
    {
        Modifier = modifier;
    }

    public Modifier Modifier { get; }

    public void Add(Action action)
    {
        _commands.Add(new FfmpegCommand { Action = action });
    }

    public void Add(string command)
    {
        _commands.Add(new FfmpegCommand { Command = command });
    }

    public async Task Run()
    {
        Modifier.Info($"Running {_commands.Count} ffmpeg commands");
        foreach (var command in _commands)
        {
            command.Action?.Invoke();
            if (!string.IsNullOrEmpty(command.Command))
                await Ffmpeg.Run(Modifier, command.Command);
        }
    }

    private class FfmpegCommand
    {
        public Action Action;
        public string Command;
    }
}