using System.IO.Abstractions;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Sandreas.Files;
using shrivel.Commands.Settings;
using shrivel.Config;
using shrivel.DependencyInjection;
using shrivel.Filters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Spectre.Console.Cli;

namespace shrivel.Commands;

public class ProcessCommand : AsyncCommand<ProcessCommandSettings>
{
    private readonly SpectreConsoleService _console;
    private readonly FileWalker _fileWalker;


    public ProcessCommand(SpectreConsoleService console, FileWalker fileWalker)
    {
        _console = console;
        _fileWalker = fileWalker;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ProcessCommandSettings settings)
    {
        var fs = _fileWalker.FileSystem;
        // todo: add recursive option
        var files = _fileWalker.Walk(settings.Input).SelectFileInfo().Where(f => !_fileWalker.IsDir(f));
        var filters = settings.Filters
            .Select(FilterFactory.Create).ToList();

        var returnCode = ReturnCode.Success;
        foreach (var file in files)
        {
            using var image = await Image.LoadAsync<Rgba32>(file.FullName);
            foreach (var filter in filters)
            {
                await filter.ApplyAsync(image);
            }

            
            var outputFileAsString = ReplaceOutputTemplate(file, settings.OutputTemplate);
            var outputFile = fs.FileInfo.FromFileName(outputFileAsString);
            if (outputFile.Exists && !settings.AssumeYes)
            {
                Console.WriteLine("file already exists and no overwrite " + outputFile);
                continue;
            }

            fs.Directory.CreateDirectory(outputFile.DirectoryName);
            await SaveImageAsync(outputFile, image);
        }

        return await Task.FromResult((int)returnCode);
    }

    private async Task SaveImageAsync(IFileInfo outputFile, Image image)
    {

        IImageEncoder enc;
        switch (outputFile.Extension.ToLowerInvariant().TrimStart('.'))
        {
            case "jpg":
            case "jpeg":
                enc = new JpegEncoder();
                break;
            case "png":
                enc = new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.Level9,
                    ColorType = PngColorType.Palette,
                    BitDepth = PngBitDepth.Bit1
                };
                break;
            default:
                throw new Exception("I do not support " + outputFile.Extension + " as output file");
        }
         await image.SaveAsync(outputFile.FullName, enc);
    }

    private string ReplaceOutputTemplate(IFileInfo file, string settingsOutputTemplate)
    {
        var name = file.Name;
        if (file.Extension != "" && name.EndsWith(file.Extension))
        {
            name = name[..^file.Extension.Length];
        }

        ;
        var templateParameters = new Dictionary<string, string>()
        {
            { "inputDirectory", file.DirectoryName },
            { "basename", file.Name },
            { "name", name },
            { "ext", file.Extension.TrimStart('.') },
        };
        foreach (var (key, value) in templateParameters)
        {
            settingsOutputTemplate = settingsOutputTemplate.Replace("{" + key + "}", value);
        }

        return settingsOutputTemplate;
    }
}