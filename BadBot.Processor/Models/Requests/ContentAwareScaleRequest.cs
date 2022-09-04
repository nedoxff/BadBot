namespace BadBot.Processor.Models.Requests;

public class ContentAwareScaleRequest : Request
{
    public override string ModifierType { get; set; } = "ContentAwareScale";
}