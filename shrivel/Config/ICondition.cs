namespace shrivel.Config;

public interface ICondition
{
    public Task<bool> IsFulfilledAsync(string sourceFile, IDictionary<string,string> vars);
}