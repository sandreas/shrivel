using shrivel.Commands.Settings;
using shrivel.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace shrivel.Commands;

public class ConvertCommand : AsyncCommand<ConvertCommandSettings>
{
    private readonly SpectreConsoleService _console;

    public ConvertCommand(SpectreConsoleService console)
    {
        _console = console;
    }
    public override async Task<int> ExecuteAsync(CommandContext context, ConvertCommandSettings settings)
    {
        _console.WriteLine("convert executed");
        return await Task.FromResult((int)ReturnCode.Success);
    }
}