using System.IO.Abstractions;

namespace shrivel.Config;

public class SourceIsNewerCondition: ConditionBase
{
    private readonly FileSystem _fs;

    public SourceIsNewerCondition(string type, string[] parameters, FileSystem fs): base(type, parameters)
    {
        _fs = fs;
    }
    
    public override Task<bool> IsFulfilledAsync(string sourceFilePath, IDictionary<string, string> vars)
    {
        var sourceFile = _fs.FileInfo.FromFileName(sourceFilePath);
        var destinationFile = _fs.FileInfo.FromFileName(Parameters.First());

        // todo: zero length files may be allowed in context other than images
        if(!destinationFile.Exists || destinationFile.Length == 0)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(sourceFile.LastWriteTimeUtc > destinationFile.LastWriteTimeUtc);
    }
}
