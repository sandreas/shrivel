using System.ComponentModel;
using shrivel.Filters;
using Spectre.Console.Cli;

namespace shrivel.Commands.Settings;

public class ProcessCommandSettings : CommandSettings
{
    [CommandOption("--yes")] public bool AssumeYes { get; set; } = false;
    [CommandOption("--filter")] public string[] Filters { get; set; } = Array.Empty<string>();    
    [CommandOption("--filter-preset")] public string FilterPreset { get; set; } = "";

    [CommandOption("--output-template")] public string OutputTemplate { get; set; } = "{inputDirectory}/processed/{name}.png";

    [Description("Input file or directory")]
    [CommandArgument(0, "[input]")]
    public string Input { get; set; } = "";

    [CommandOption("--merge-pdf")] public string MergePdf { get; set; } = "";
    [CommandOption("--delete-merge-source")] public bool DeleteMergeSources { get; set; } = false;

}