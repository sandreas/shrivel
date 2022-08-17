using System.IO.Abstractions;
using CliWrap;
using CliWrap.Buffered;

namespace shrivel.Config;

public class CommandRunner
{
    public readonly string Id;
    public readonly string[] CallParameters;
    private readonly FileSystem _fs;
    private readonly string _input;
    private readonly string _output;

    public CommandRunner(FileSystem fs, string input, string output, string id, string[] callParameters)
    {
        _fs = fs;
        _input = input;
        _output = output;
        Id = id;
        CallParameters = callParameters;
    }
    


    public async Task RunAsync(string sourceFile, Instruction instruction)
    {
        var relative = "";
        if (sourceFile.StartsWith(_input))
        {
            relative = sourceFile[_input.Length..];
        }

        var baseDestination = _fs.FileInfo.FromFileName(Path.Join(_output, relative));
        var name = baseDestination.Name;

        if (baseDestination.Extension != "" && baseDestination.Name.EndsWith(baseDestination.Extension))
        {
            name = baseDestination.Name[..^baseDestination.Extension.Length];
        }

        var ext = baseDestination.Extension.TrimStart('.');
        var path = baseDestination.DirectoryName;
        if (path.StartsWith(_output))
        {
            path = path[_output.Length..].TrimStart('/');
        }
        var replacements = new Dictionary<string, string>()
        {
            { "input", _input },
            { "output", _output },
            { "source", sourceFile },
            { "tempPath", Path.GetTempPath() },
            { "path",  path},
            { "name", name },
            { "extension", ext },
        };
        foreach(var vars in instruction.Runs)        {
            foreach (var (key, value) in vars)
            {
                replacements[key] = value ?? "";
            }
            
            replacements = ResolveReplacementsRecursively(replacements); 

            // check conditions
            foreach(var (conditionType, conditionParameters) in instruction.Conditions)
            {
                var replacedConditionParameters =
                    conditionParameters.Select(p => ReplaceCallParameter(p, replacements)).ToArray();
                
                var condition = BuildCondition(conditionType, replacedConditionParameters);
                
                if(!await condition.IsFulfilledAsync(sourceFile, replacements))
                {
                    Console.WriteLine($"file: {sourceFile}, condition type={condition.Type}, parameters=[{string.Join(", ", replacedConditionParameters)}] is not fulfilled - skipping run");
                    return;
                }
                
            }
            
            var replacedCallParameters = CallParameters.Select(p => ReplaceCallParameter(p, replacements)).ToArray();

            var result = await Cli.Wrap(replacedCallParameters.First()).WithArguments(replacedCallParameters.Skip(1))
                .ExecuteBufferedAsync();
            if(result.ExitCode != 0)
            {
                break;
            }
        }

    }

    private ConditionBase BuildCondition(string conditionType, string[] conditionParameters) => conditionType switch
    {
        "sourceExtension" => new SourceExtensionCondition(conditionType, conditionParameters, _fs),
        "imageSizeGreaterEqual" => new ImageNoUpscaleCondition(conditionType, conditionParameters),
        "sourceIsNewer" => new SourceIsNewerCondition(conditionType, conditionParameters, _fs),
        _ => new UnknownCondition(conditionType, conditionParameters)
    };

    private static Dictionary<string,string> ResolveReplacementsRecursively(Dictionary<string, string> replacements)
    {

        // var newReplacements = new Dictionary<string, string>();
        
        foreach(var (key,value) in replacements){
            var counter = 0;
            do
            {
                foreach (var (innerKey, innerValue) in replacements)
                {
                    replacements[key] = replacements[key].Replace("{" + innerKey + "}", innerValue);
                }
                counter++;
            } while (replacements[key].Contains("{") && counter < 10);
        }
        return replacements;
    }

    private string ReplaceCallParameter(string s, Dictionary<string, string> replacements)
    {

        foreach (var (pattern, replacement) in replacements)
        {
            s = s.Replace("{" + pattern + "}", replacement);
        }
        return s;
    }
}