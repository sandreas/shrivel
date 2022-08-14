namespace shrivel.Optimizers;

public interface IImageOptimizer
{
    public  Task<IEnumerable<string>> OptimizeAsync(string filePath);
}