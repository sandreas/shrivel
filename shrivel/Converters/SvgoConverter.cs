using System.IO.Abstractions;
using CliWrap;
using CliWrap.Buffered;

namespace shrivel.Converters;

public class SvgoConverter: ImageConverterBase
{
    private readonly IImageConverterSettings? _options;
    private readonly Command _command;

    public SvgoConverter(FileSystem fs, Command command, IImageConverterSettings? options) : base(fs)
    {
        _options = options;
        _command = command;
    }
    public override async Task<IEnumerable<string>> ConvertAsync(string sourceFilePath)
    {
        var result = new List<string>();
        var sourceFile = Fs.FileInfo.FromFileName(sourceFilePath);
        if(_options == null || !sourceFile.Exists || !IsExtensionSupported(sourceFile.Extension, "svg"))
        {
            return result;
        }
        
        var destinationPath = BuildDestination(sourceFilePath, _options);
        var destinationFile = Fs.FileInfo.FromFileName(destinationPath);
        Fs.Directory.CreateDirectory(destinationFile.Directory.FullName);
            
        var commandResult = await _command.WithArguments(a =>
        {
            a.Add("--multipass").Add(sourceFile.FullName).Add("-o").Add(destinationFile.FullName);
        }).ExecuteBufferedAsync();
        if(destinationFile.Exists){
            result.Add(destinationFile.FullName);
        }

        return result;
    }
}