
namespace shrivel.Converters;

public interface IImageConverter
{
    public Task<IEnumerable<string>> ConvertAsync(string sourceFilePath, string fileNameTemplate, int? size);

}