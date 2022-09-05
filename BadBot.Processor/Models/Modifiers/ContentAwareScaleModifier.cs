using BadBot.Processor.Extensions;
using BadBot.Processor.Ffmpeg;
using BadBot.Processor.Models.Requests;
using ImageMagick;

namespace BadBot.Processor.Models.Modifiers;

public class ContentAwareScaleModifier : Modifier<ContentAwareScaleRequest>
{
    private const int MaxWorkers = 5;

    public override async Task<ModifierResult> OnProcess()
    {
        ResourceLimits.LimitMemory(new Percentage(20));
        var preBuild = new FfmpegBuilder(this);
        if (Path.GetExtension(InputFile) != ".mp4")
            preBuild = preBuild.Convert(InputFile, "input.mp4");
        preBuild = preBuild
            .Split(30, "Segments")
            .ExtractAudio("audio.mp3", "-filter_complex \"vibrato=f=10:d=0.6\"")
            .GetVideoFramerate(InputFile, "fps.txt")
            .SplitVideosIntoFrames("Segments", "Frames");
        await preBuild.Run();

        var fps = float.Parse(await File.ReadAllTextAsync(Path.Join(WorkingDirectory, "fps.txt")));
        var builder = new FfmpegBuilder(this);
        var frames = Path.Join(WorkingDirectory, "Frames");
        var processed = Path.Join(WorkingDirectory, "FramesProcessed");
        var processedChunks = Path.Join(WorkingDirectory, "ProcessedChunks");
        Directory.CreateDirectory(processed);
        Directory.CreateDirectory(processedChunks);
        var directories = Directory.GetDirectories(frames).ToList();
        var totalFiles = directories.Sum(directory => Directory.GetFiles(directory).Length);
        Info($"The total amount of workers is {directories.Count}");
        Info($"The total amount of frames is {totalFiles}");
        var fileIndex = 0;
        var index = 1;
        while (true)
        {
            var chunks = directories.Take(MaxWorkers).ToList();
            if (chunks.Count == 0) break;
            directories = directories.Skip(chunks.Count).ToList();
            Info($"Starting {chunks.Count} workers");
            var tasks = new List<Task>();
            foreach (var chunk in chunks)
            {
                var files = Directory.GetFiles(chunk).OrderBy(x => int.Parse(Path.GetFileNameWithoutExtension(x)))
                    .ToList();
                var processedChunk = Path.Join(processed, new DirectoryInfo(chunk).Name);
                Directory.CreateDirectory(processedChunk);
                tasks.Add(new ImageMagickWorker(fileIndex, totalFiles, files, processedChunk, index, this).Run());
                fileIndex += files.Count;
                index++;
                builder = builder.CombineImages(processedChunk,
                    Path.Join(processedChunks, new DirectoryInfo(chunk).Name + ".mp4"), fps);
            }

            await Task.WhenAll(tasks);
        }

        builder = builder
            .WriteIntoFile("ProcessedChunks", "files.txt")
            .Concat("files.txt", "noaudio.mp4")
            .CombineVideoAndAudio("noaudio.mp4", "audio.mp3", "output.mp4");
        await builder.Run();
        OutputFile = "output.mp4";

        return new ModifierResult
        {
            Success = true
        };
    }
}

public class ImageMagickWorker
{
    public List<string> Files;
    public int FirstFileGlobalIndex;
    public int Index;
    public Modifier Modifier;
    public string OutputDirectory;
    public int TotalFileCount;

    public ImageMagickWorker(int firstFileGlobalIndex, int totalFileCount, List<string> files, string outputDirectory,
        int index, Modifier modifier)
    {
        FirstFileGlobalIndex = firstFileGlobalIndex;
        TotalFileCount = totalFileCount;
        Files = files;
        OutputDirectory = outputDirectory;
        Modifier = modifier;
        Index = index;
    }

    public async Task Run()
    {
        Modifier.Info(
            $"FirstFileGlobalIndex is {FirstFileGlobalIndex}\nFileCount is {Files.Count}\nOutputDirectory is {OutputDirectory}\nTotalFileCount is {TotalFileCount}");
        var current = (double)FirstFileGlobalIndex;
        foreach (var file in Files)
        {
            var image = new MagickImage(file);
            var value = (1.2 - current.Map(0, TotalFileCount, 0.2, 1)) * 100;
            var width = (int)(image.Width * (value / 100f));
            width = width % 2 != 0 ? width + 1 : width;
            var height = (int)(image.Height * (value / 100f));
            height = height % 2 != 0 ? height + 1 : height;
            image.LiquidRescale(new MagickGeometry(width, height) { IgnoreAspectRatio = true });
            await image.WriteAsync(Path.Join(OutputDirectory, Path.GetFileName(file)));
            current++;
        }

        Modifier.Info($"Worker No.{Index} finished");
    }
}