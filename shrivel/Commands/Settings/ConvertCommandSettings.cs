using System.ComponentModel;
using shrivel.Commands.Settings.TypeConverters;
using shrivel.Converters;
using Spectre.Console;
using Spectre.Console.Cli;

namespace shrivel.Commands.Settings;

public class ConvertCommandSettings : CommandSettings
{
    [CommandOption("--debug")] public bool Debug { get; set; } = false;
    [CommandOption("--force")] public bool Force { get; set; } = false;

    [CommandOption("--convert-command")] public string? ConvertCommand { get; set; } = null;

    [CommandOption("--svgo-command")] public string? SvgoCommand { get; set; } = null;
    [CommandOption("--cwebp-command")] public string? CwebpCommand { get; set; } = null;
    [CommandOption("--jpegoptim-command")] public string? JpegoptimCommand { get; set; } = null;
    [CommandOption("--pngquant-command")] public string? PngquantCommand { get; set; } = null;

    
    [Description("Input directory")]
    [CommandArgument(0, "[input]")]
    public string Input { get; set; } ="";

    [Description("Output directory")]
    [CommandArgument(1, "[output]")]
    public string Output { get; set; } = "";
    
    [CommandOption("--instruction")] 
    [TypeConverter(typeof(InstructionConverter<ConverterInstruction>))]
    public ConverterInstruction[] Instructions { get; set; } = Array.Empty<ConverterInstruction>();    
    
    [CommandOption("--instructions-file")] public string? InstructionsFile { get; set; } = null;

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