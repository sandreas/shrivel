using System.ComponentModel;
using Newtonsoft.Json.Linq;
using shrivel.Filters.Implementations;

namespace shrivel.Filters;

public class FilterFactory
{
    public static IImageFilter Create(string filter)
    {
        var (filterId, parameters) = ParseFilter(filter);
        if(filterId == ImageFilterIdentifier.None)
        {
            throw new InvalidEnumArgumentException($"Invalid filter {filter}");
        }
/*
        var objects = filterParams.ToObject(typeof(object[]));
        if (objects is object[]o)
        {
            parameters = o;
        }
        filter = filter[..splitPos];
        */
        var casted = parameters?.ToObject(typeof(object[]));
        if (casted is not object[] objects)
        {
            objects = Array.Empty<object>();
        }
        return CreateFilterInstance(filterId,  objects);
    }

    private static IImageFilter CreateFilterInstance(ImageFilterIdentifier filterId, object[] parameters) =>
        filterId switch
        {
            // todo: crop
            ImageFilterIdentifier.AdaptiveThreshold => new AdaptiveThreshold(parameters),
            ImageFilterIdentifier.Resize => new Resize(parameters),
            ImageFilterIdentifier.Grayscale => new Grayscale(parameters),
            ImageFilterIdentifier.BlackWhite => new BlackWhite(parameters),
            ImageFilterIdentifier.Rotate => new Rotate(parameters),
            ImageFilterIdentifier.None => throw new ArgumentOutOfRangeException(nameof(filterId), filterId,  "Invalid filter: none"),
            _ => throw new ArgumentOutOfRangeException(nameof(filterId), filterId, "Invalid filter: unknown")
        };

    private static (ImageFilterIdentifier filterId, JArray? parameters) ParseFilter(string filter)
    {
        JArray? filterParameters = null;
        if(filter.Contains("["))
        {
            var splitPos = filter.IndexOf("[", StringComparison.Ordinal);
            var filterParamsJson = filter[splitPos..];
            filterParameters = JArray.Parse(filterParamsJson);
            filter = filter[..splitPos];

        }
        if(Enum.TryParse<ImageFilterIdentifier>(filter, true, out var filterId))
        {
            return (filterId, filterParameters);
        }

        return (ImageFilterIdentifier.None, filterParameters);
    }
}

