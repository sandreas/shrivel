using System.IO.Abstractions;
using Sandreas.Files;
using shrivel.Commands.Settings;
using shrivel.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace shrivel.Commands;

public class CleanCommand : AsyncCommand<CleanCommandSettings>
{
    private readonly SpectreConsoleService _console;
    private readonly FileWalker _fileWalker;


    public CleanCommand(SpectreConsoleService console, FileWalker fileWalker)
    {
        _console = console;
        _fileWalker = fileWalker;
    }

    // Returns the human-readable file size for an arbitrary, 64-bit file size 
// The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
    public string GetBytesReadable(long i)
    {
        // Get absolute value
        long absolute_i = (i < 0 ? -i : i);
        // Determine the suffix and readable value
        string suffix;
        double readable;
        if (absolute_i >= 0x1000000000000000) // Exabyte
        {
            suffix = "EB";
            readable = (i >> 50);
        }
        else if (absolute_i >= 0x4000000000000) // Petabyte
        {
            suffix = "PB";
            readable = (i >> 40);
        }
        else if (absolute_i >= 0x10000000000) // Terabyte
        {
            suffix = "TB";
            readable = (i >> 30);
        }
        else if (absolute_i >= 0x40000000) // Gigabyte
        {
            suffix = "GB";
            readable = (i >> 20);
        }
        else if (absolute_i >= 0x100000) // Megabyte
        {
            suffix = "MB";
            readable = (i >> 10);
        }
        else if (absolute_i >= 0x400) // Kilobyte
        {
            suffix = "KB";
            readable = i;
        }
        else
        {
            return i.ToString("0 B"); // Byte
        }

        // Divide by 1024 to get fractional value
        readable = (readable / 1024);
        // Return formatted number with suffix
        return readable.ToString("0.### ") + suffix;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, CleanCommandSettings settings)
    {
        var fs = _fileWalker.FileSystem;
        if (settings.Input == "" || !_fileWalker.IsDir(settings.Input))
        {
            _console.Error.WriteLine("please specify a valid input directory to clean");
            return (int)ReturnCode.GeneralError;
        }

        var normalizedExtensions = settings.Extensions.Select(NormalizeExtension).Distinct().ToArray();
        if (normalizedExtensions.Length < 2)
        {
            _console.Error.WriteLine("please specify at least 2 different extensions");
            return (int)ReturnCode.GeneralError;
        }

        var filesToDelete = new List<(IFileInfo file, string reason)>();

        var filePrefixes = _fileWalker.WalkRecursive(settings.Input).SelectFileInfo().Where(f =>
                !_fileWalker.IsDir(f) &&
                normalizedExtensions.Contains(NormalizeExtension(f.Extension))).Select(f =>
                f.ToString()![..(f.ToString()!.Length - NormalizeExtension(f.Extension).Length)])
            .Distinct();

        var fileCounter = 0;
        foreach (var filePrefix in filePrefixes)
        {
            fileCounter++;
            var fileSet = new List<IFileInfo>();
            foreach (var ext in normalizedExtensions)
            {
                var file = fs.FileInfo.FromFileName(filePrefix + ext);
                if (file is { Exists: true })
                {
                    if (file.Length == 0)
                    {
                        filesToDelete.Add((file, "empty / 0 bytes file: " + file.FullName));
                        continue;
                    }

                    fileSet.Add(file);
                }
            }

            for (var i = 0; i < fileSet.Count - 1; i++)
            {
                var preferredFile = fileSet.ElementAt(i);
                var replacementFile = fileSet.ElementAt(i + 1);

                if (replacementFile.Length <= preferredFile.Length)
                {
                    var reason = preferredFile.Name + " (" + GetBytesReadable(preferredFile.Length) +
                                 ") is bigger than " + replacementFile.Name + " (" +
                                 GetBytesReadable(replacementFile.Length) + ")";
                    filesToDelete.Add((preferredFile, reason));
                }
            }
        }

        _console.WriteLine(fileCounter + " images scanned" );
        if (filesToDelete.Count > 1)
        {
            if (!settings.AssumeYes && _console.Confirm($"found {filesToDelete.Count} files to delete - delete all?"))
            {
                settings.AssumeYes = true;
            }
        }

        if (filesToDelete.Count > 0)
        {
            foreach (var (fileToDelete, reason) in filesToDelete)
            {
                if (settings.AssumeYes || _console.Confirm(reason + " - delete?"))
                {
                    fs.File.Delete(fileToDelete.FullName);
                }
            }
        }

        return await Task.FromResult((int)ReturnCode.Success);
    }

    private static string NormalizeExtension(string e) => e.TrimStart('.').ToLowerInvariant();

    private static async Task ResetCommandLog(FileSystem fs, string commandLogFile)
    {
        if (!string.IsNullOrEmpty(commandLogFile))
        {
            await fs.File.WriteAllTextAsync(commandLogFile, "");
        }
    }
}