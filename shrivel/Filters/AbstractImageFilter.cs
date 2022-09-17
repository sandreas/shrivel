using SixLabors.ImageSharp;

namespace shrivel.Filters;

public abstract class AbstractImageFilter:IImageFilter
{
    public abstract ImageFilterIdentifier Id { get; }
    public abstract Task ApplyAsync(Image image);
    protected bool _parseInt(object[] obj, int index, string message, out int output)
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