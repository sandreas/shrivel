using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace shrivel.Filters.Implementations;

public class AdaptiveThreshold: AbstractImageFilter
{
    public AdaptiveThreshold(object parameters)
    {
        
    }

    public override ImageFilterIdentifier Id => ImageFilterIdentifier.AdaptiveThreshold;
    public override Task ApplyAsync(Image image)
    {
        image.Mutate(img => img.AdaptiveThreshold());
        return Task.CompletedTask;
    }
}