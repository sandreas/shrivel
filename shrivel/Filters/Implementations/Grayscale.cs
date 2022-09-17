using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace shrivel.Filters.Implementations;

public class Grayscale: AbstractImageFilter
{
    public Grayscale(object parameters)
    {
        
    }

    public override ImageFilterIdentifier Id => ImageFilterIdentifier.Resize;
    public override Task ApplyAsync(Image image)
    {
        image.Mutate(x => x.Grayscale());
        return Task.CompletedTask;
    }
}