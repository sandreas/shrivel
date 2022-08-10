using System.ComponentModel;
using Spectre.Console.Cli;

namespace shrivel.Commands.Settings;

public class ConvertCommandSettings : CommandSettings
{
    [CommandOption("--debug")] public bool Debug { get; set; } = false;
    [CommandOption("--force")] public bool Force { get; set; } = false;

    [Description("Input directory")]
    [CommandArgument(0, "[input]")]
    public string Input { get; set; } ="";

    [Description("Output directory")]
    [CommandArgument(1, "[output]")]
    public string Output { get; set; } = "";

}