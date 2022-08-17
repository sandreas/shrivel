namespace shrivel.Config;

public abstract class ConditionBase: ICondition
{
    public readonly string Type;
    public readonly string[] Parameters;

    public ConditionBase(string type, string[] parameters)
    {
        Type = type;
        Parameters = parameters;
    }

    public abstract Task<bool> IsFulfilledAsync(string sourceFile, IDictionary<string, string> vars);
}