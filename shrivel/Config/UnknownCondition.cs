namespace shrivel.Config;

public class UnknownCondition: ConditionBase
{
    public UnknownCondition(string type, string[] parameters) : base(type, parameters)
    {
    }
    
    public override  Task<bool> IsFulfilledAsync(string sourceFile, IDictionary<string, string> vars)
    {
        return  Task.FromResult(false);
    }
    
}