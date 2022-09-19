using System.IO.Abstractions;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using Sandreas.Files;
using shrivel.Commands.Settings;
using shrivel.DependencyInjection;
using shrivel.Filters;
using shrivel.Filters.Implementations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
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
        var outputFiles = new List<IFileInfo>();
        foreach (var file in files)
        {
            var outputFileAsString = ReplaceOutputTemplate(file, settings.OutputTemplate);
            var outputFile = fs.FileInfo.FromFileName(outputFileAsString);
            outputFiles.Add(outputFile);
            if (outputFile.Exists && !settings.AssumeYes)
            {
                Console.WriteLine("file already exists and no overwrite " + outputFile);
                continue;
            }
            using var image = await Image.LoadAsync<Rgba32>(file.FullName);
            foreach (var filter in filters)
            {
                await filter.ApplyAsync(image);
            }
            fs.Directory.CreateDirectory(outputFile.DirectoryName);
            await SaveImageAsync(outputFile, image, filters);
            
        }

        if (settings.MergePdf != "")
        {
            var pdfFiles = outputFiles.Where(f => f.Extension.ToLowerInvariant().TrimStart('.') == "pdf").OrderBy(f => f.FullName).ToList();
            using var outputDocument = new PdfDocument();
            foreach (var pdfFile in pdfFiles)
            {
                var inputDocument = PdfReader.Open(pdfFile.FullName, PdfDocumentOpenMode.Import);
                var count = inputDocument.PageCount;
                
                for (int idx = 0; idx < count; idx++)
                {
                    var page = inputDocument.Pages[idx]; 
                    outputDocument.AddPage(page);
                }
            }

            var outputFile = fs.FileInfo.FromFileName(settings.MergePdf);
            if (!outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
            }

            if (!fs.File.Exists(settings.MergePdf) || settings.AssumeYes)
            {
                outputDocument.Save(settings.MergePdf);
            }
            if (settings.DeleteMergeSources)
            {
                foreach (var pdf in pdfFiles)
                {
                    pdf.Delete();
                }
            }
        }
        
        return await Task.FromResult((int)returnCode);
    }

    private async Task SaveImageAsync(IFileInfo outputFile, Image image,
        List<IImageFilter> imageFilters)
    {
        // threshold images can be saved as 1 bit png
        var binaryImage = imageFilters.Any(f => f is AdaptiveThreshold);
        switch (outputFile.Extension.ToLowerInvariant().TrimStart('.'))
        {
            case "jpg":
            case "jpeg":
                await image.SaveAsync(outputFile.FullName,  new JpegEncoder());
                break;
            case "png":
                var enc = binaryImage ? new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.Level9,
                    ColorType = PngColorType.Palette,
                    BitDepth = PngBitDepth.Bit1
                } : new PngEncoder();
                await image.SaveAsync(outputFile.FullName, enc);
                break;
            case "pdf":

                IImageEncoder pdfEnc = binaryImage
                    ? new PngEncoder
                    {
                        CompressionLevel = PngCompressionLevel.Level9,
                        ColorType = PngColorType.Palette,
                        BitDepth = PngBitDepth.Bit1
                    }
                    : new JpegEncoder();
                var tmpFilePath = outputFile.FullName + ".tmp";

                try
                {
                    await image.SaveAsync(tmpFilePath, pdfEnc);
                    using var pdfDocument = new PdfDocument();
                    pdfDocument.Info.Title = outputFile.Name;
                    var page = pdfDocument.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);
                    // Use the memory stream in a binary reader.
                    var pdfImage = XImage.FromFile(tmpFilePath);
                    gfx.DrawImage(pdfImage, 0, 0, page.Width, page.Height);
                    pdfDocument.Save(outputFile.FullName);
                }
                finally
                {
                    if (_fileWalker.FileSystem.File.Exists(tmpFilePath))
                    {
                        _fileWalker.FileSystem.File.Delete(tmpFilePath);
                    }                    
                }
                break;
            default:
                throw new Exception("I do not support " + outputFile.Extension + " as output file");
        }

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