using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace shrivel.Filters.Implementations;

public class Rotate: IImageFilter
{
    private readonly int _angle ;
    public Rotate(object[] parameters)
    {
        
        if (parameters.Length == 0)
        {
            throw new ArgumentException("rotate filter requires at least 1 argument");
        }

        if(_parseInt(parameters, 0, "rotate: first argument has to be an integer", out var angle))
        {
            _angle = angle;
        }
    }

    public ImageFilterIdentifier Id => ImageFilterIdentifier.Resize;
    public Task ApplyAsync(Image image)
    {
        image.Mutate(x => x.Rotate(_angle));
        return Task.CompletedTask;
    }
    
    private bool _parseInt(object[] obj, int index, string message, out int output)
    {
        output = 0;
        if (obj.Length < index)
        {
            return false;
        }
        if(!int.TryParse( obj[index].ToString(), out var value)) {
            throw new ArgumentException(message);
        }

        output = value;
        return true;
    }
}