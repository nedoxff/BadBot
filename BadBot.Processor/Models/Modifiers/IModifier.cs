using BadBot.Processor.Models;

namespace BadBot.Processor.Modifiers;

public interface IModifier
{
    public string Name { get; set; }
    public string Id { get; set; }
    public string WorkingDirectory { get; set; }
    public Request Request { get; set; }
    public List<string> Warnings { get; set; }
    public List<string> Errors { get; set; }
    public string Status { get; set; }
    public string InputFile { get; set; }
    public string OutputFile { get; set; }

    public Task<ModifierResult> OnProcess();

    public void Warning(string msg) => Warnings.Add(msg);
    public void Error(string msg) => Errors.Add(msg);
}