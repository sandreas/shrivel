
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Jint;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Sandreas.Files;
using shrivel;
using shrivel.Commands;
using shrivel.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;


var propagateExceptions = args.Contains("--debug");

var settingsProvider = new CustomCommandSettingsProvider();

var services = new ServiceCollection();
services.AddSingleton(_ => settingsProvider);
services.AddSingleton<FileSystem>();
services.AddSingleton<FileWalker>();
services.AddSingleton<SpectreConsoleService>();

var app = new CommandApp(new CustomTypeRegistrar(services));

app.Configure(config =>
{
    config.SetInterceptor(new CustomCommandInterceptor(settingsProvider));
    config.UseStrictParsing();
    config.CaseSensitivity(CaseSensitivity.None);
    config.SetApplicationName("tone");
    config.SetApplicationVersion("0.0.8");
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