using System.Diagnostics;
using BadBot.Processor.Models.Modifiers;
using Serilog;

namespace BadBot.Processor.Ffmpeg;

public class Ffmpeg
{
    public static async Task Run(Modifier modifier, string command)
    {
        var pci = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = command,
            WorkingDirectory = modifier.WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        modifier.Info($"Executing ffmpeg {command}");
        var process = new Process
        {
            StartInfo = pci
        };
        process.OutputDataReceived += (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
                return;
            Log.Debug(e.Data);
        };
        process.Start();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync();
    }

    public static async Task<string> RunFfprobe(Modifier modifier, string command)
    {
        var pci = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = command,
            WorkingDirectory = modifier.WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        modifier.Info($"Executing ffprobe {command}");
        var process = new Process
        {
            StartInfo = pci
        };
        var lastLine = "";
        process.OutputDataReceived += (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
                return;
            lastLine = e.Data;
        };
        process.Start();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync();
        return lastLine;
    }

    public static void KillProcesses()
    {
        Log.Debug("Killing ffmpeg and ffprobe processes");
        foreach (var process in Process.GetProcessesByName("ffmpeg"))
            process.Kill(true);
        foreach (var process in Process.GetProcessesByName("ffprobe"))
            process.Kill(true);
    }
}