using Sandreas.Files;
using shrivel.Commands.Settings;
using shrivel.Converters;
using shrivel.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace shrivel.Commands;

public class ConvertCommand : AsyncCommand<ConvertCommandSettings>
{
    private readonly SpectreConsoleService _console;
    private readonly FileWalker _fileWalker;
    private readonly ImageMagickConverter _imageMagickConverter;
    private readonly SvgoConverter _svgConverter;
    private readonly CwebpConverter _cwebpConverter;

    public ConvertCommand(SpectreConsoleService console, FileWalker fileWalker,
        ImageMagickConverter imageMagickMagickConverter, SvgoConverter svgoConverter, CwebpConverter cwebpConverter)
    {
        _console = console;
        _fileWalker = fileWalker;
        _imageMagickConverter = imageMagickMagickConverter;
        _svgConverter = svgoConverter;
        _cwebpConverter = cwebpConverter;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ConvertCommandSettings settings)
    {
        var converters = new List<IImageConverter> { _imageMagickConverter, _svgConverter, _cwebpConverter };


        var files = _fileWalker.WalkRecursive(settings.Input).SelectFileInfo().Where(f => !_fileWalker.IsDir(f));
        foreach (var file in files)
        {
            foreach (var converter in converters)
            {
                var result = await converter.ConvertAsync(file?.ToString() ?? "");
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