using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Sandreas.Files;
using shrivel;
using shrivel.Commands;
using shrivel.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;


// manually handle args to define default command and debug behaviour
var propagateExceptions = args.Contains("--debug");
if (!args.Contains("run"))
{
    var argsList = new List<string>(new []{"run"});
    argsList.AddRange(args);
    args = argsList.ToArray();
}

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
    config.SetApplicationName("shrivel");
    config.SetApplicationVersion("0.0.1");
    config.ValidateExamples();
    config.AddCommand<RunCommand>("run")
        .WithDescription("run instruction set")
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