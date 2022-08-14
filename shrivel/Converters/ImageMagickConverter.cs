using System.IO.Abstractions;
using CliWrap;
using CliWrap.Buffered;
using shrivel.Commands.Settings;

namespace shrivel.Converters;

public class ImageMagickConverter : ImageConverterBase
{
    private static readonly string[] SupportedExtensions =
    {
        "jpg", "jpeg", "gif", "png"
    };

    private readonly Command _command;

    public ImageMagickConverter(FileSystem fs, Command command, ConvertCommandSettings settings) : base(fs, settings)
    {
        _command = command;
    }

    public override async Task<IEnumerable<string>> ConvertAsync(string sourceFilePath, string fileNameTemplate,
        int? size)
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

        await _command.WithArguments(a =>
        {
            a.Add(sourceFile.FullName);
            if (size > 0)
            {
                a.Add("-resize").Add(size);
            }

            a.Add(destinationFile.FullName);
        }).ExecuteBufferedAsync();
        if (destinationFile.Exists)
        {
            result.Add(destinationFile.FullName);
        }

        return result;
    }
}