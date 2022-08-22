using System.Diagnostics;
using System.IO.Abstractions;
using CliWrap;
using CliWrap.Buffered;

namespace shrivel.Config;

public class CommandRunner
{
    public readonly string Id;
    private readonly string[] _callParameters;
    private readonly FileSystem _fs;
    private readonly string _input;
    private readonly string _output;
    private readonly string _commandLogFile;

    
    public CommandRunner(FileSystem fs, ContainerSettings settings, string id, string[] callParameters)
    {
        _fs = fs;
        _input = NormalizeDirectoryPath(settings.Input);
        _output = NormalizeDirectoryPath(settings.Output);
        _commandLogFile = settings.CommandLog;
        Id = id;
        _callParameters = callParameters;
    }
    
    private static string NormalizeDirectoryPath(string path){
        return path.TrimEnd('/').TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    }


    public async Task<(int code, string message)> RunAsync(string sourceFilePath, Instruction instruction)
    {
        var inputDirectory = _fs.FileInfo.FromFileName(_input);
        var sourceFile = _fs.FileInfo.FromFileName(sourceFilePath);

        if (!_fs.Directory.Exists(inputDirectory.FullName) || !sourceFile.Exists)
        {
            return (1, "inputDir or sourceFile did not exist");
        }

        var relative = sourceFile.FullName[inputDirectory.FullName.Length..];

        var baseDestination = _fs.FileInfo.FromFileName(Path.Join(_output, relative));
        var name = baseDestination.Name;

        if (baseDestination.Extension != "" && baseDestination.Name.EndsWith(baseDestination.Extension))
        {
            name = baseDestination.Name[..^baseDestination.Extension.Length];
        }

        var ext = baseDestination.Extension.TrimStart('.');
        var relBase = baseDestination.ToString();
        var path = NormalizeDirectoryPath(relBase?[..relBase.LastIndexOf(Path.DirectorySeparatorChar)] ?? "") ;

        if (path.StartsWith(_output))
        {
            path = path[_output.Length..].TrimStart('/');
        }

        var outputDirectory = _fs.FileInfo.FromFileName(_output);

        var replacements = new Dictionary<string, string>()
        {
            { "input", inputDirectory.FullName },
            { "output", outputDirectory.FullName },
            { "source", sourceFile.FullName },
            { "tempPath", Path.GetTempPath() },
            { "path", path },
            { "name", name },
            { "extension", ext },
        };

        
        foreach (var vars in instruction.Runs)
        {
            foreach (var (key, value) in vars)
            {
                replacements[key] = value ?? "";
            }

            replacements = ResolveReplacementsRecursively(replacements);

            var replacedCallParameters = _callParameters.Select(p => ReplaceCallParameter(p, replacements)).ToArray();


            var (conditionsMet, message) = await CheckConditions(sourceFilePath, replacements, replacedCallParameters, instruction);
            if(!conditionsMet)
            {
                Console.WriteLine(message);
                continue;
            }
            
            await LogCallParameters(replacedCallParameters);

            if(replacements.ContainsKey("destination"))
            {
                var destination = _fs.FileInfo.FromFileName(replacements["destination"]);
                if(!_fs.Directory.Exists(destination.DirectoryName))
                {
                    _fs.Directory.CreateDirectory(destination.DirectoryName);
                }
            }
            

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine("    -> " + CallParametersToCommandString(replacedCallParameters));
            var result = await Cli.Wrap(replacedCallParameters.First())
                .WithWorkingDirectory(_fs.Directory.GetCurrentDirectory())
                .WithArguments(replacedCallParameters.Skip(1))
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();
            Console.WriteLine($"    -> {stopWatch.ElapsedMilliseconds}ms");
            if (result.ExitCode != 0)
            {
                return (result.ExitCode, result.StandardError + result.StandardOutput);
            }
        }


        
        return (0, "");
    }
    
    private async Task<(bool conditionsMet, string message)> CheckConditions(string sourceFilePath, Dictionary<string, string> replacements, IEnumerable<string> replacedCallParameters, Instruction instruction)
    {
        // check conditions
        foreach (var (conditionType, conditionParameters) in instruction.Conditions)
        {
            var replacedConditionParameters =
                conditionParameters.Select(p => ReplaceCallParameter(p, replacements)).ToArray();

            var condition = BuildCondition(conditionType, replacedConditionParameters);

            if (!await condition.IsFulfilledAsync(sourceFilePath, replacements))
            {
                // Console.WriteLine();
                if(condition is SourceIsNewerCondition){
                    await LogCallParameters(replacedCallParameters);
                }
                return (false, $"file: {sourceFilePath}, condition type={condition.Type}, parameters=[{string.Join(", ", replacedConditionParameters)}] is not fulfilled - skipping run");
            }
        }

        return (true, "");
    }

    private async Task LogCallParameters(IEnumerable<string> replacedCallParameters, string prefix="") {
        if(!string.IsNullOrEmpty(_commandLogFile)){
            await _fs.File.AppendAllTextAsync(_commandLogFile, prefix+CallParametersToCommandString(replacedCallParameters) + "\n");
        }
    }

    private static string CallParametersToCommandString(IEnumerable<string> callParameters)            {
        return "'" + string.Join("' '", callParameters.Select(p => p.Replace("'", "\'"))) + "'";
    }
    
    private ConditionBase BuildCondition(string conditionType, string[] conditionParameters) => conditionType switch
    {
        "sourceExtension" => new SourceExtensionCondition(conditionType, conditionParameters, _fs),
        "imageSizeGreaterEqual" => new ImageNoUpscaleCondition(conditionType, conditionParameters),
        "sourceIsNewer" => new SourceIsNewerCondition(conditionType, conditionParameters, _fs),
        _ => new UnknownCondition(conditionType, conditionParameters)
    };

    private static Dictionary<string, string> ResolveReplacementsRecursively(Dictionary<string, string> replacements)
    {
        // var newReplacements = new Dictionary<string, string>();

        foreach (var (key, _) in replacements)
        {
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

    private static string ReplaceCallParameter(string s, Dictionary<string, string> replacements)
    {
        foreach (var (pattern, replacement) in replacements)
        {
            s = s.Replace("{" + pattern + "}", replacement);
        }

        return s;
    }
}