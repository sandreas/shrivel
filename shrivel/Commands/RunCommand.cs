using CliWrap;
using CliWrap.Buffered;
using Newtonsoft.Json;
using Sandreas.Files;
using shrivel.Commands.Settings;
using shrivel.Config;
using shrivel.Converters;
using shrivel.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace shrivel.Commands;

public class RunCommand : AsyncCommand<RunCommandSettings>
{
    private readonly SpectreConsoleService _console;
    private readonly FileWalker _fileWalker;


    public RunCommand(SpectreConsoleService console, FileWalker fileWalker)
    {
        _console = console;
        _fileWalker = fileWalker;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, RunCommandSettings settings)
    {
        var fs = _fileWalker.FileSystem;

        var files = _fileWalker.WalkRecursive(settings.Input).SelectFileInfo().Where(f => !_fileWalker.IsDir(f));
        var config = JsonConvert.DeserializeObject<Container>(await fs.File.ReadAllTextAsync(settings.Config));
        if(config == null){
            return await Task.FromResult((int)ReturnCode.GeneralError);
        }

        var commandRunners = config.Commands
            .Select(kvp => new CommandRunner(fs, settings.Input, settings.Output, kvp.Key, kvp.Value))
            .ToDictionary(c => c.Id, c => c);

        
        foreach (var file in files)
        {

            foreach(var inst in config.Instructions)
            {
                if(!commandRunners.ContainsKey(inst.Command))
                {
                    continue;
                }
                if(inst.Filters.ContainsKey("extensions") && !inst.Filters["extensions"].Contains(file?.Extension.TrimStart('.').ToLowerInvariant()))
                {
                    Console.WriteLine("filter did not match: " + inst.Command);
                    continue;
                }
                
                var runner = commandRunners[inst.Command];
                await runner.RunAsync(file?.ToString()??"", inst);
                
                /*
                if(result.ExitCode != 0){
                    Console.WriteLine("non zero exit code on instruction: " + inst.Command);
                    break;
                }
                */
            }
        }

        return await Task.FromResult((int)ReturnCode.Success);
    }
}