using System.IO.Abstractions;
using CliWrap;
using CliWrap.Buffered;

namespace shrivel.Optimizers;

public class PngquantOptimizer : ImageOptimizerBase
{
    private readonly Command _command;
    private static readonly string[] SupportedExtensions =
    {
        "png"
    };
    public PngquantOptimizer(FileSystem fs, Command command) : base(fs)
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
            // inplace: --ext .png -f, see https://github.com/kornelski/pngquant/issues/255
            // https://github.com/kornelski/pngquant
            a.Add("--quality=65-80").Add("--skip-if-larger").Add("--speed=1").Add("--strip").Add(fileToOptimize.FullName).Add("--ext").Add(fileToOptimize.Extension).Add("-f");

        }).ExecuteBufferedAsync();
        if (commandResult.ExitCode == 0)
        {
            result.Add(fileToOptimize.FullName);
        }

        return result;
    }
    protected static bool IsExtensionSupported(string extension, params string[] supportedExtensions)=>supportedExtensions.Contains(extension.TrimStart('.').ToLowerInvariant());

}