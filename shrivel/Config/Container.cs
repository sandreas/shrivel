namespace shrivel.Config;

public class Container
{
    public ContainerSettings Settings { get; set; } = new();

    public Dictionary<string, string[]> Commands { get; set; } = new();
    public Dictionary<string, string[]> CleanupActions { get; set; } = new();
    public List<Instruction> Instructions { get; set; } = new();
}