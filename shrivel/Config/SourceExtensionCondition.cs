using System.IO.Abstractions;

namespace shrivel.Config;

public class SourceExtensionCondition: ConditionBase
{
    private readonly FileSystem _fs;

    public SourceExtensionCondition(string type, string[] parameters, FileSystem fs): base(type, parameters)
    {
        _fs = fs;
    }


    public override Task<bool> IsFulfilledAsync(string sourceFile, IDictionary<string, string> vars)
    {
        var file = _fs.FileInfo.FromFileName(sourceFile);
        return Task.FromResult(Parameters.Contains(file.Extension.TrimStart('.')));
    }
}
