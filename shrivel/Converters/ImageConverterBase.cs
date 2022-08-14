using System.IO.Abstractions;
using shrivel.Commands.Settings;

namespace shrivel.Converters;

public abstract class ImageConverterBase: IImageConverter
{
    protected readonly FileSystem Fs;
    protected readonly ConvertCommandSettings Settings;

    protected ImageConverterBase(FileSystem fs, ConvertCommandSettings convertCommandSettings)
    {
        Fs = fs;
        Settings = convertCommandSettings;
    }


    protected string BuildDestination(string sourceFile, string fileNameTemplate, int? size=null)
    {
        var relative = "";
        if(sourceFile.StartsWith(Settings.Input))
        {
            relative = sourceFile[Settings.Input.Length..];
        }

        var baseDestination = Fs.FileInfo.FromFileName(Path.Join(Settings.Output, relative));
        var destinationDir = baseDestination.Directory.FullName;
        var name = baseDestination.Name;
        if(baseDestination.Extension != "" && baseDestination.Name.EndsWith(baseDestination.Extension))
        {
            name = baseDestination.Name[..^baseDestination.Extension.Length];
        }

        var ext = baseDestination.Extension.TrimStart('.');
        var destinationName = ReplaceFileNameTemplate(fileNameTemplate, name, ext, size);
        return Path.Join(destinationDir, destinationName);
        
    }

    private static string ReplaceFileNameTemplate(string optionsFileNameTemplate, string destinationName, string destinationExtension, int? size=null)
    {
        
        return optionsFileNameTemplate
            .Replace("{name}", destinationName)
            .Replace("{extension}", destinationExtension)
            .Replace("{size}", size?.ToString() ?? "");
    }


    
    protected static bool IsExtensionSupported(string extension, params string[] supportedExtensions)=>supportedExtensions.Contains(extension.TrimStart('.').ToLowerInvariant());

    public abstract Task<IEnumerable<string>> ConvertAsync(string sourceFilePath, string fileNameTemplate, int? size);
}