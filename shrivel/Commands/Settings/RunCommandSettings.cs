using Spectre.Console.Cli;

namespace shrivel.Commands.Settings;

public class RunCommandSettings : CommandSettings
{
    [CommandOption("--config")] public string? Config { get; set; } = ".shrivel.json";
}