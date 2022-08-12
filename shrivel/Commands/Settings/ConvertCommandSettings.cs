using System.ComponentModel;
using shrivel.Converters;
using Spectre.Console;
using Spectre.Console.Cli;

namespace shrivel.Commands.Settings;

public class ConvertCommandSettings : CommandSettings, IImageConverterSettings
{
    [CommandOption("--debug")] public bool Debug { get; set; } = false;
    [CommandOption("--force")] public bool Force { get; set; } = false;

    [CommandOption("--convert-command")] public string? ConvertCommand { get; set; } = null;

    [CommandOption("--svgo-command")] public string? SvgoCommand { get; set; } = null;
    [CommandOption("--cwebp-command")] public string? CwebpCommand { get; set; } = null;

    
    [Description("Input directory")]
    [CommandArgument(0, "[input]")]
    public string Input { get; set; } ="";

    [Description("Output directory")]
    [CommandArgument(1, "[output]")]
    public string Output { get; set; } = "";
    
    [CommandOption("--size")] public int[] Sizes { get; set; } = Array.Empty<int>();
    public string FileNameTemplate { get; set; } = "{name}_{size}.{extension}";

    public override ValidationResult Validate()
    {
        var violations = new List<string>();
        if(Input == "") {
            violations.Add("please specify an input directory");
        }
        if(Output == "") {
            violations.Add("please specify an output directory");
        }
        return violations.Count > 0 ? ValidationResult.Error(string.Join(", ", violations))
            : ValidationResult.Success();
    }

}