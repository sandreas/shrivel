using System.ComponentModel;
using shrivel.Commands.Settings.TypeConverters;
using shrivel.Converters;
using Spectre.Console;
using Spectre.Console.Cli;

namespace shrivel.Commands.Settings;

public class RunCommandSettings : CommandSettings
{
    [CommandOption("--debug")] public bool Debug { get; set; } = false;
    [CommandOption("--force")] public bool Force { get; set; } = false;

    [Description("Input directory")]
    [CommandArgument(0, "[input]")]
    public string Input { get; set; } ="";

    [Description("Output directory")]
    [CommandArgument(1, "[output]")]
    public string Output { get; set; } = "";
    

    [CommandOption("--config")] public string? Config { get; set; } = null;

    public override ValidationResult Validate()
    {
        var violations = new List<string>();
        if(Input == "") {
            violations.Add("please specify an input directory");
        }
        if(Output == "") {
            violations.Add("please specify an output directory");
        }
        if(Config == "") {
            violations.Add("please specify a config file");
        }
        return violations.Count > 0 ? ValidationResult.Error(string.Join(", ", violations))
            : ValidationResult.Success();
    }

}