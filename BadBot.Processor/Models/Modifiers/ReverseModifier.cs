using BadBot.Processor.Ffmpeg;
using BadBot.Processor.Models.Requests;

namespace BadBot.Processor.Models.Modifiers;

public class ReverseModifier : Modifier<ReverseRequest>
{
    public override async Task<ModifierResult> OnProcess()
    {
        await new FfmpegBuilder(this)
            .Split(300)
            .ReverseSegments()
            .WriteIntoFile("Reversed", "videos.txt", true)
            .Concat("videos.txt", "output.mp4")
            .Run();
        OutputFile = "output.mp4";
        return new ModifierResult
        {
            Success = true
        };
    }
}