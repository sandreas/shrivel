using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace shrivel.Filters.Implementations;

public class Resize: AbstractImageFilter
{
    private readonly int _width ;
    private readonly int _height;
    private readonly bool _keepRatio = true;
    public Resize(object[] parameters)
    {
        
        if (parameters.Length == 0)
        {
            throw new ArgumentException("resize filter requires at least 1 argument");
        }

        if(_parseInt(parameters, 0, "resize: first argument has to be an integer", out var size))
        {
            _width = size;
            _height = size;
        }
        
        if(_parseInt(parameters, 1, "resize: second argument has to be an integer", out var height))
        {
            _height = height;
        }
    }

    public override ImageFilterIdentifier Id => ImageFilterIdentifier.Resize;
    public override Task ApplyAsync(Image image)
    {
        var width = _width;
        var height = _height;
        if(_keepRatio){
            if(image.Width > image.Height)
            {
                height = 0;
            } else {
                width = 0;
            }
        }
        image.Mutate(x => x.Resize(width, height));
        return Task.CompletedTask;
    }
    
    
}