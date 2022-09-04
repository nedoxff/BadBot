namespace BadBot.Processor.Ffmpeg;

public static class FfmpegBuilderExtensions
{
    public static FfmpegBuilder Split(this FfmpegBuilder builder, int segmentTime)
    {
        builder.Add(
            $"-i \"{builder.Modifier.InputFile}\" -map 0 -c copy -f segment -segment_time {segmentTime} -reset_timestamps 1 video_%03d.mp4");
        return builder;
    }

    public static FfmpegBuilder PitchAudio(this FfmpegBuilder builder, double amplifier, string to)
    {
        builder.Add(
            $"-i \"{builder.Modifier.InputFile}\" -filter_complex rubberband=pitch={amplifier.ToString().Replace(",", ".")} \"{to}\"");
        return builder;
    }

    public static FfmpegBuilder ReverseSegments(this FfmpegBuilder builder)
    {
        builder.Add(() =>
        {
            File.Delete(builder.Modifier.InputFile);
            var directory = Path.Join(builder.Modifier.WorkingDirectory, "Reversed");
            Directory.CreateDirectory(directory);
            foreach (var video in Directory.GetFiles(builder.Modifier.WorkingDirectory))
                Ffmpeg.Run(builder.Modifier,
                        $"-i \"{Path.GetFileName(video)}\" -vf reverse -af areverse \"{Path.Join(directory, Path.GetFileNameWithoutExtension(video))}_reversed.mp4\"")
                    .GetAwaiter().GetResult();
        });
        return builder;
    }

    public static FfmpegBuilder WriteIntoFile(this FfmpegBuilder builder, string directory, string to,
        bool reverse = false)
    {
        builder.Add(() =>
        {
            var files = Directory.GetFiles(Path.Join(builder.Modifier.WorkingDirectory, directory))
                .Select(x => $"file '{x}'");
            if (reverse)
                files = files.Reverse().ToArray();
            File.WriteAllLines(Path.Join(builder.Modifier.WorkingDirectory, to), files);
        });
        return builder;
    }

    public static FfmpegBuilder Concat(this FfmpegBuilder builder, string list, string to)
    {
        builder.Add($"-f concat -safe 0 -i \"{list}\" -c copy \"{to}\"");
        return builder;
    }
}