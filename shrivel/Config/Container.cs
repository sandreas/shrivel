namespace shrivel.Config;

public class Container
{
    public Dictionary<string, string[]> Commands { get; set; } = new();
    public List<Instruction> Instructions { get; set; } = new();
}