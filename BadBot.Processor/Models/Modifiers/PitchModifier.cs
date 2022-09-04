using BadBot.Processor.Ffmpeg;
using BadBot.Processor.Models.Requests;

namespace BadBot.Processor.Models.Modifiers;

public class PitchModifier : Modifier<PitchRequest>
{
    public override async Task<ModifierResult> OnProcess()
    {
        await new FfmpegBuilder(this)
            .PitchAudio(Request.Amplifier, "output.mp4")
            .Run();
        OutputFile = "output.mp4";
        return new ModifierResult
        {
            Success = true
        };
    }
}