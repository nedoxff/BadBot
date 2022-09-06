using System.Globalization;

namespace BadBot.Processor.Ffmpeg;

public static class FfmpegBuilderExtensions
{
    public static FfmpegBuilder Split(this FfmpegBuilder builder, int segmentTime, string to = "", string from = "")
    {
        var dir = Path.Join(builder.Modifier.WorkingDirectory, to);
        if (!string.IsNullOrEmpty(to) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        builder.Add(
            $"-i \"{(string.IsNullOrEmpty(from) ? builder.Modifier.InputFile : from)}\" -map 0 -c copy -f segment -segment_time {segmentTime} -reset_timestamps 1 \"{Path.Join(dir, "%03d.mp4")}\"");
        return builder;
    }

    public static FfmpegBuilder PitchAudio(this FfmpegBuilder builder, double amplifier, string to)
    {
        builder.Add(
            $"-i \"{builder.Modifier.InputFile}\" -filter_complex rubberband=pitch={amplifier.ToString().Replace(",", ".")} \"{to}\"");
        return builder;
    }

    public static FfmpegBuilder ExtractAudio(this FfmpegBuilder builder, string to, string effects = "")
    {
        builder.Add($"-i \"{builder.Modifier.InputFile}\" -acodec mp3 -vn {effects} \"{to}\"");
        return builder;
    }

    public static FfmpegBuilder SplitVideosIntoFrames(this FfmpegBuilder builder, string from, string to)
    {
        builder.Add(() =>
        {
            var dir = Path.Join(builder.Modifier.WorkingDirectory, to);
            if (Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var files = Directory.GetFiles(Path.Join(builder.Modifier.WorkingDirectory, from));
            foreach (var file in files)
            {
                var path = Path.Join(builder.Modifier.WorkingDirectory, to, Path.GetFileNameWithoutExtension(file));
                Directory.CreateDirectory(path);
                Ffmpeg.Run(builder.Modifier, $"-i \"{file}\" \"{Path.Join(path, "%d.png")}\"").GetAwaiter().GetResult();
            }
        });
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

    public static FfmpegBuilder GetVideoFramerate(this FfmpegBuilder builder, string from, string writeTo)
    {
        builder.Add(async () =>
        {
            var ratio = await Ffmpeg.RunFfprobe(
                builder.Modifier, $"-v 0 -of csv=p=0 -select_streams v:0 -show_entries stream=r_frame_rate \"{from}\"");
            if (!ratio.Contains('/') || ratio.Split('/')[1] == "0")
                throw new Exception("Invalid framerate response received");
            var split = ratio.Split('/');
            var fps = float.Parse(split[0]) / float.Parse(split[1]);
            await File.WriteAllTextAsync(Path.Join(builder.Modifier.WorkingDirectory, writeTo),
                fps.ToString(CultureInfo.InvariantCulture));
        });
        return builder;
    }

    public static FfmpegBuilder GetVideoDuration(this FfmpegBuilder builder, string from, string writeTo)
    {
        builder.Add(() =>
        {
            var duration = Ffmpeg.RunFfprobe(builder.Modifier, $"-v 0 -select_streams v:0 -show_entries stream=duration -of default=noprint_wrappers=1:nokey=1 \"{from}\"").Result;
            File.WriteAllText(Path.Join(builder.Modifier.WorkingDirectory, writeTo), duration);
        });
        return builder;
    }

    public static FfmpegBuilder CompressVideo(this FfmpegBuilder builder, string from, string to, long bits, float duration)
    {
        var nullLocation = Environment.OSVersion.Platform == PlatformID.Win32NT ? "NUL" : "/dev/null";
        var bitrate = (int)Math.Floor(bits / duration / 8000);
        builder.Add($"-i \"{from}\" -c:v libx264 -preset medium -b:v {bitrate}k -pass 1 -an -f {Path.GetExtension(to).Replace(".", "")} -y {nullLocation}");
        builder.Add($"-i \"{from}\" -c:v libx264 -preset medium -b:v {bitrate}k -pass 2 -c:a copy \"{to}\"");
        return builder;
    }

    public static FfmpegBuilder CombineImages(this FfmpegBuilder builder, string from, string to, float fps)
    {
        builder.Add($"-r {fps} -i \"{Path.Join(from, "%d.png")}\" -vf format=yuv420p -r {fps} \"{to}\"");
        return builder;
    }

    public static FfmpegBuilder Convert(this FfmpegBuilder builder, string from, string to)
    {
        builder.Add($"-i \"{from}\" \"{to}\"");
        return builder;
    }

    public static FfmpegBuilder CombineVideoAndAudio(this FfmpegBuilder builder, string from, string audio, string to)
    {
        builder.Add($"-i \"{from}\" -i \"{audio}\" -map 0:v -map 1:a -c:v copy \"{to}\"");
        return builder;
    }

    public static FfmpegBuilder Concat(this FfmpegBuilder builder, string list, string to)
    {
        builder.Add($"-f concat -safe 0 -i \"{list}\" -c copy \"{to}\"");
        return builder;
    }
}