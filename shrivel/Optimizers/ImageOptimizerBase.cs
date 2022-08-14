
using System.IO.Abstractions;

namespace shrivel.Optimizers;

public abstract class ImageOptimizerBase: IImageOptimizer
{
    protected readonly FileSystem Fs;

    protected ImageOptimizerBase(FileSystem fs)
    {
        Fs = fs;
    }
    public abstract Task<IEnumerable<string>> OptimizeAsync(string filePath);
}