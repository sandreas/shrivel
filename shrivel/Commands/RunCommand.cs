using Newtonsoft.Json;
using Sandreas.Files;
using shrivel.Commands.Settings;
using shrivel.Config;
using shrivel.DependencyInjection;
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

        Container? config;
        try
        {
            var configFile = fs.FileInfo.FromFileName(settings.Config);
            fs.Directory.SetCurrentDirectory(configFile.DirectoryName);
        
            config = JsonConvert.DeserializeObject<Container>(await fs.File.ReadAllTextAsync(settings.Config));
            if(config == null){
                return await Task.FromResult((int)ReturnCode.GeneralError);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }


        var files = _fileWalker.WalkRecursive(config.Settings.Input).SelectFileInfo().Where(f => !_fileWalker.IsDir(f));
        var commandRunners = config.Commands
            .Select(kvp => new CommandRunner(fs, config.Settings.Input, config.Settings.Output, kvp.Key, kvp.Value))
            .ToDictionary(c => c.Id, c => c);
        
        var returnCode = ReturnCode.Success;
        foreach (var file in files)
        {
            foreach(var inst in config.Instructions)
            {
                if(!commandRunners.ContainsKey(inst.Command))
                {
                    continue;
                }

                Console.WriteLine($"processing {inst.Command} for {file?.FullName}...");
                var runner = commandRunners[inst.Command];
                var (exitCode, message) = await runner.RunAsync(file?.ToString()??"", inst);
                if(exitCode != 0)
                {
                    Console.WriteLine("=> FAILED: " + message);

                    returnCode = ReturnCode.GeneralError;
                } else {
                    Console.WriteLine("=> OK");
                }
            }
        }

        return await Task.FromResult((int)returnCode);
    }
}