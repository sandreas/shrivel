using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace shrivel.Filters.Implementations;

public class BlackWhite: AbstractImageFilter
{
    public BlackWhite(object parameters)
    {
        
    }

    public override ImageFilterIdentifier Id => ImageFilterIdentifier.BlackWhite;
    public override Task ApplyAsync(Image image)
    {
        image.Mutate(x => x.BlackWhite());
        return Task.CompletedTask;
    }
}