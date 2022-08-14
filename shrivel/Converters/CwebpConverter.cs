using System.IO.Abstractions;
using CliWrap;
using CliWrap.Buffered;
using shrivel.Commands.Settings;

namespace shrivel.Converters;

public class CwebpConverter : ImageConverterBase
{
    private static readonly string[] SupportedExtensions =
    {
        "jpg", "jpeg", "gif", "png"
    };

    private readonly Command _command;

    public CwebpConverter(FileSystem fs, Command command, ConvertCommandSettings settings) : base(fs, settings)
    {
        _command = command;
    }

    public override async Task<IEnumerable<string>> ConvertAsync(string sourceFilePath, string fileNameTemplate, int? size)
    {
        var result = new List<string>();
        var sourceFile = Fs.FileInfo.FromFileName(sourceFilePath);
        if (!sourceFile.Exists || !IsExtensionSupported(sourceFile.Extension, SupportedExtensions))
        {
            return result;
        }

        var destinationPath = BuildDestination(sourceFilePath, fileNameTemplate, size);
        var destinationFile = Fs.FileInfo.FromFileName(destinationPath);
        Fs.Directory.CreateDirectory(destinationFile.Directory.FullName);

        var commandResult = await _command.WithArguments(a =>
        {
            // cwebp", "-resize", "{size}", "0", "-q", "75", "{source}", "-o", "{destination}", "-quiet
            if (size > 0)
            {
                a.Add("-resize").Add(size);
            }

            a.Add("0").Add("q").Add("75").Add(sourceFile.FullName).Add("-o").Add(destinationFile.FullName)
                .Add("-quiet");
        }).ExecuteBufferedAsync();
        if (destinationFile.Exists)
        {
            result.Add(destinationFile.FullName);
        }

        return result;
    }
}