namespace BadBot.Processor.Models.Requests;

public class ReverseRequest : Request
{
    public override string ModifierType { get; set; } = "Reverse";
}