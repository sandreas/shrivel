using System.IO.Abstractions;
using CliWrap;
using CliWrap.Buffered;

namespace shrivel.Optimizers;

public class JpegoptimOptimizer : ImageOptimizerBase
{
    private readonly Command _command;
    private static readonly string[] SupportedExtensions =
    {
        "jpg", "jpeg"
    };
    public JpegoptimOptimizer(FileSystem fs, Command command) : base(fs)
    {
        _command = command;
    }

    public override async Task<IEnumerable<string>> OptimizeAsync(string filePath)
    {
        var result = new List<string>();
        var fileToOptimize = Fs.FileInfo.FromFileName(filePath);

        if (!fileToOptimize.Exists || !IsExtensionSupported(fileToOptimize.Extension, SupportedExtensions))
        {
            return result;
        }

        var commandResult = await _command.WithArguments(a =>
        {
            a.Add("--max=75").Add("--all-progressive").Add("--strip-all").Add(fileToOptimize.FullName);

        }).ExecuteBufferedAsync();
        if (commandResult.ExitCode == 0)
        {
            result.Add(fileToOptimize.FullName);
        }

        return result;
    }
    protected static bool IsExtensionSupported(string extension, params string[] supportedExtensions)=>supportedExtensions.Contains(extension.TrimStart('.').ToLowerInvariant());

}