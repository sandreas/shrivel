using System.IO.Abstractions;

namespace shrivel.Config;

public class ImageNoUpscaleCondition: ConditionBase
{

    public ImageNoUpscaleCondition(string type, string[] parameters): base(type, parameters)
    {
        
    }

    public override async Task<bool> IsFulfilledAsync(string sourceFile, IDictionary<string, string> vars)
    {
        try
        {
            var size = 0;
            if(Parameters.Length > 0 && int.TryParse(Parameters.First(), out var parameterSize))
            {
                size = parameterSize;
            }
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(sourceFile);
            return Math.Max(image.Width, image.Height) >= size;
        }
        catch (Exception)
        {
            // ignored
        }

        return false;
    }
}
