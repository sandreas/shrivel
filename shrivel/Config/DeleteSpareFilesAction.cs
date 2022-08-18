using System.IO.Abstractions;

namespace shrivel.Config;

public class DeleteSpareFilesAction
{
    private readonly FileSystem _fs;
    private readonly string _output;
    public readonly string Id;
    private readonly string[] _values;
    private readonly string _baseExtension;
    private readonly string[] _compareExtensions;

    public DeleteSpareFilesAction(FileSystem fs, string settingsOutput, string argKey, string[] argValue)
    {
        Id = argKey;
        _fs = fs;
        _output = settingsOutput;
        _values = argValue;
        _baseExtension = _values.FirstOrDefault() ?? "";
        _compareExtensions = _values.Length > 1 ? _values.Skip(1).ToArray() : Array.Empty<string>();
    }
    
    public Task<bool> ExecuteAsync(string sourceFilePath)
    {
        if(_baseExtension == ""){
            return Task.FromResult(false);
        }
        var sourceFile = _fs.FileInfo.FromFileName(sourceFilePath);
        if(!sourceFile.Exists || sourceFile.Extension.TrimStart('.') != _baseExtension)
        {
            return Task.FromResult(false);
        }

        var filePrefix = sourceFile.FullName[..^sourceFile.Extension.Length];
        var filesToCheck = _compareExtensions.Select(e => _fs.FileInfo.FromFileName(filePrefix + "." + e)).ToArray();

        foreach(var f in filesToCheck)        {
            if(sourceFile.Length > f.Length) {
                sourceFile.Delete();
            }
        }
        return Task.FromResult(true);
    }
}