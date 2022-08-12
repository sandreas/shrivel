using System.IO.Abstractions;

namespace shrivel.Converters;

public abstract class ImageConverterBase: IImageConverter
{
    protected readonly FileSystem Fs;

    protected ImageConverterBase(FileSystem fs)
    {
        Fs = fs;
    }
    protected string BuildDestination(string sourceFile, IImageConverterSettings options, int? size=null)
    {
        var relative = "";
        if(sourceFile.StartsWith(options.Input))
        {
            relative = sourceFile.Substring(options.Input.Length);
        }

        var baseDestination = Fs.FileInfo.FromFileName(Path.Join(options.Output, relative));
        var destinationDir = baseDestination.Directory.FullName;
        var name = baseDestination.Name;
        if(baseDestination.Extension != "" && baseDestination.Name.EndsWith(baseDestination.Extension))
        {
            name = baseDestination.Name[..^baseDestination.Extension.Length];
        }

        var ext = baseDestination.Extension.TrimStart('.');
        var destinationName = ReplaceFileNameTemplate(options.FileNameTemplate, name, ext, size);
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

    public abstract Task<IEnumerable<string>> ConvertAsync(string sourceFilePath);
}