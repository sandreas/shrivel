namespace shrivel.Config;

public class Instruction
{
    public string Command { get; set; } = "";
    public Dictionary<string, string[]> Conditions { get; set; } = new();
    public List<Dictionary<string, string?>> Runs { get; set; } = new();
}