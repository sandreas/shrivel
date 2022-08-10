using Spectre.Console.Cli;

namespace shrivel.DependencyInjection;


public sealed class CustomTypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider;

    public CustomTypeResolver(IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public object? Resolve(Type? type)
    {
        return type == null ? null : _provider.GetService(type);
    }

    public void Dispose()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}