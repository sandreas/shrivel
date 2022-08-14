using Sandreas.Files;
using shrivel.Commands.Settings;
using shrivel.Converters;
using shrivel.DependencyInjection;
using shrivel.Optimizers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace shrivel.Commands;

public class ConvertCommand : AsyncCommand<ConvertCommandSettings>
{
    private readonly SpectreConsoleService _console;
    private readonly FileWalker _fileWalker;
    private readonly ImageMagickConverter _imagick;
    private readonly SvgoConverter _svg;
    private readonly CwebpConverter _cwebp;
    private readonly string[] _vectorExtensions = { ".svg" };
    private readonly JpegoptimOptimizer _jpgOpt;
    private readonly PngquantOptimizer _pngOpt;

    public ConvertCommand(SpectreConsoleService console, FileWalker fileWalker,
        ImageMagickConverter imagick, SvgoConverter svgo, CwebpConverter cwebp, JpegoptimOptimizer jpgOpt, PngquantOptimizer pngOpt)
    {
        _console = console;
        _fileWalker = fileWalker;
        _imagick = imagick;
        _svg = svgo;
        _cwebp = cwebp;
        _jpgOpt = jpgOpt;
        _pngOpt = pngOpt;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ConvertCommandSettings settings)
    {
        var converters = new List<IImageConverter> { _imagick, _svg, _cwebp };


        var files = _fileWalker.WalkRecursive(settings.Input).SelectFileInfo().Where(f => !_fileWalker.IsDir(f));
        var instructions = settings.Instructions.ToList();

        var fs = _fileWalker.FileSystem;
        if (fs.File.Exists(settings.InstructionsFile))
        {
            instructions.AddRange((await _fileWalker.FileSystem.File.ReadAllLinesAsync(settings.InstructionsFile))
                .Where(l => l.Length > 0).Select(l => new ConverterInstruction(l.Trim())));
        }

        foreach (var file in files)
        {
            var imageSize = int.MaxValue;
            if (!_vectorExtensions.Contains(file.Extension.ToLowerInvariant()))
            {
                try
                {
                    using var image = await SixLabors.ImageSharp.Image.LoadAsync(file.FullName);
                    imageSize = Math.Max(image.Width, image.Height);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error on {file}: {e.Message}");
                }
            }

            foreach (var instruction in instructions)
            {
                if (instruction.Size != null && instruction.Size > imageSize)
                {
                    continue;
                }

                IImageConverter? converter = instruction.ConverterIdentifier switch
                {
                    ConverterIdentifier.Imagick => _imagick, // This is the first switch expression arm
                    ConverterIdentifier.Svgo => _svg,
                    ConverterIdentifier.Cwebp => _cwebp,
                    // ConverterIdentifier.Avifenc => _avivencConverter,
                    _ => null
                };
                if (converter != null)
                {
                    var createdFiles = await converter.ConvertAsync(file?.ToString() ?? "", instruction.FileNameTemplate,
                        instruction.Size);
                    
                    foreach(var fileToOpt in createdFiles)
                    {
                        await _jpgOpt.OptimizeAsync(fileToOpt);
                        await _pngOpt.OptimizeAsync(fileToOpt);
                    }
                    
                }
            }
        }

        // svg, jpg, png, gif, webp, avif
        // destination: formats, sizes, fileNameTemplates, baselineFormat (for size comparison)
        // 
        /*
         todo:
         converter -> converts image to size and format
            convert
            cwebp
            convert + avifenc
         optimizer -> optimizes size of an image
            svg, jpg, png
         
         https://web.dev/compress-images-avif/
         https://stackoverflow.com/questions/65765174/convert-png-images-to-lossy-avif
         https://publishing-project.rivendellweb.net/image-formats-for-the-web-heic-and-avif/
         
         raster images:
            find out dimensions of raster images
            conversion step with `convert`?
                convert "${SRC_IMAGE}" -resize "${size}x${size}" "${SRC_IMAGE_LOSSLESS}"
         
         "$SVGO" --multipass "$SRC_IMAGE" -o "$DST_WITHOUT_EXT.$SRC_EXT"
         jpegoptim --max=75 --all-progressive --strip-all "${dst}"
         pngquant --skip-if-larger --strip --quiet --ext .png --force "${dst}"
         
         cwebp -resize ${size} 0 -q 75 "${SRC_IMAGE}" -o "${DST_FULL_PATH}" -quiet
         
         avifenc \
            --codec aom \
            --jobs all \
            --min 0 --max 63 -a end-usage=q -a cq-level=28 -a tune=ssim \
            --ignore-icc \
            --speed 0 \
            "${SRC_IMAGE_LOSSLESS}" \
            --output "${DST_FULL_PATH}" > /dev/null && rm "${SRC_IMAGE_LOSSLESS}"
         */
        _console.WriteLine("convert executed");
        return await Task.FromResult((int)ReturnCode.Success);
    }
}