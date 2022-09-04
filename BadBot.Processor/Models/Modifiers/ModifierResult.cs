namespace BadBot.Processor.Models.Modifiers;

public class ModifierResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public DateTime Started { get; set; }
    public DateTime Ended { get; set; }
}