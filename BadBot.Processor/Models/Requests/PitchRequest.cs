namespace BadBot.Processor.Models.Requests;

public class PitchRequest : Request
{
    public override string ModifierType { get; set; } = "Pitch";
    public double Amplifier { get; set; }
}