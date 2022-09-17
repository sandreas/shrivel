using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace shrivel.Filters;

public interface IImageFilter
{
    public ImageFilterIdentifier Id { get; }

     Task ApplyAsync(Image image);
}