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
            WorkingDirectory = modifier.WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardError = true
        };
        modifier.Info($"Executing ffmpeg {command}");
        pci.Arguments = command;
        var process = new Process
        {
            StartInfo = pci
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
                return;
            Log.Debug(e.Data);
        };
        process.Start();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();
    }

    public static void KillProcesses()
    {
        Log.Debug("Killing ffmpeg processes");
        foreach (var process in Process.GetProcessesByName("ffmpeg"))
            process.Kill(true);
    }
}