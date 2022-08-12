using System.IO.Abstractions;
using CliWrap;
using CliWrap.Buffered;

namespace shrivel.Converters;

public class ImageMagickConverter: ImageConverterBase
{
    private static readonly string[] SupportedExtensions = {
        "jpg", "jpeg", "gif", "png"
    };
    private readonly Command _command;
    private readonly IImageConverterSettings? _options;
    
    public ImageMagickConverter(FileSystem fs, Command command,IImageConverterSettings? options) : base(fs)
    {
        _options = options;
        _command = command;
    }
    public override async Task<IEnumerable<string>> ConvertAsync(string sourceFilePath)
    {
        var result = new List<string>();
        var sourceFile = Fs.FileInfo.FromFileName(sourceFilePath);
        if(_options == null || !sourceFile.Exists || !IsExtensionSupported(sourceFile.Extension, SupportedExtensions))
        {
            return result;
        }
        
        foreach(var size in _options.Sizes)
        {
            var destinationPath = BuildDestination(sourceFilePath, _options, size);
            var destinationFile = Fs.FileInfo.FromFileName(destinationPath);
            Fs.Directory.CreateDirectory(destinationFile.Directory.FullName);
            
            await _command.WithArguments(a =>
            {
                a.Add(sourceFile.FullName);
                if(size> 0)
                {
                    a.Add("-resize").Add(size);
                }
                a.Add(destinationFile.FullName);

            }).ExecuteBufferedAsync();
            if(destinationFile.Exists){
                result.Add(destinationFile.FullName);
            }
        }
        return result;
    }


}