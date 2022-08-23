using System.ComponentModel;
using Spectre.Console.Cli;

namespace shrivel.Commands.Settings;

public class CleanCommandSettings : CommandSettings
{
    [CommandOption("--extension")] public string[] Extensions { get; set; } = Array.Empty<string>();    
    [CommandOption("--yes")] public bool AssumeYes { get; set; } = false;
    
    [Description("Input directory")]
    [CommandArgument(0, "[input]")]
    public string Input { get; set; } = "";
    
}