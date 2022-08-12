using System.IO.Abstractions;
using CliWrap;
using Microsoft.Extensions.DependencyInjection;
using Sandreas.Files;
using shrivel;
using shrivel.Commands;
using shrivel.Commands.Settings;
using shrivel.Converters;
using shrivel.DependencyInjection;
using shrivel.Extensions;
using Spectre.Console;
using Spectre.Console.Cli;


var propagateExceptions = args.Contains("--debug");

var settingsProvider = new CustomCommandSettingsProvider();

var services = new ServiceCollection();
services.AddSingleton(_ => settingsProvider);
services.AddSingleton<FileSystem>();
services.AddSingleton<FileWalker>();
services.AddSingleton<SpectreConsoleService>();

services.AddSingleton(s =>
{
    var convertCommandSettings = settingsProvider.Get<ConvertCommandSettings>();
    var command = Cli.Wrap(convertCommandSettings?.ConvertCommand ?? "convert").WithValidation(CommandResultValidation.None);
    return new ImageMagickConverter(s.GetRequiredService<FileSystem>(), command, convertCommandSettings);
});
services.AddSingleton(s =>
{
    // svg has no "sizes", so override FileNameTemplate setting manually
    // todo: optimize this into real setting
    var convertCommandSettings = settingsProvider.Get<ConvertCommandSettings>().DeepCopy() ?? new ConvertCommandSettings();
    convertCommandSettings.FileNameTemplate = "{name}.{extension}";
    var command = Cli.Wrap(convertCommandSettings?.SvgoCommand ?? "svgo").WithValidation(CommandResultValidation.None);
    return new SvgoConverter(s.GetRequiredService<FileSystem>(), command, convertCommandSettings);
});

services.AddSingleton(s =>
{
    var convertCommandSettings = settingsProvider.Get<ConvertCommandSettings>().DeepCopy() ??new ConvertCommandSettings();
    convertCommandSettings.FileNameTemplate = "{name}_{size}.webp";
    var command = Cli.Wrap(convertCommandSettings?.CwebpCommand ?? "cwebp").WithValidation(CommandResultValidation.None);
    return new CwebpConverter(s.GetRequiredService<FileSystem>(), command, convertCommandSettings);
});

var app = new CommandApp(new CustomTypeRegistrar(services));

app.Configure(config =>
{
    config.SetInterceptor(new CustomCommandInterceptor(settingsProvider));
    config.UseStrictParsing();
    config.CaseSensitivity(CaseSensitivity.None);
    config.SetApplicationName("shrivel");
    config.SetApplicationVersion("0.0.1");
    config.ValidateExamples();
    config.AddCommand<ConvertCommand>("convert")
        .WithDescription("convert images from input to output")
        ;
    

    if (propagateExceptions)
    {
        config.PropagateExceptions();
    }
#if DEBUG
    config.ValidateExamples();
#endif
});
try
{
    return await app.RunAsync(args).ConfigureAwait(false);
}
catch (Exception e)
{
    if (e is CommandParseException { Pretty: { } } ce)
    {
        AnsiConsole.Write(ce.Pretty);
    }

    AnsiConsole.WriteException(e);
    return (int)ReturnCode.UncaughtException;
}