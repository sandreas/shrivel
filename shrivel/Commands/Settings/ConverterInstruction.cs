namespace shrivel.Commands.Settings;

public class ConverterInstruction
{
    public ConverterIdentifier ConverterIdentifier { get; set; } = ConverterIdentifier.None;
    public string FileNameTemplate { get; set; } = "";
    public int? Size { get; set; }

    public ConverterInstruction()
    {
        
    }
    
    public ConverterInstruction(string instructionString)
    {
        var parts = instructionString.Split(";");
        if(parts.Length > 0 && Enum.TryParse<ConverterIdentifier>(parts[0], true, out var id))
        {
            ConverterIdentifier = id;
        }
        if(parts.Length > 1)
        {
            FileNameTemplate = parts[1];
        }
        
        if(parts.Length > 2 && int.TryParse(parts[2], out var size))
        {
            Size = size;
        }
    }
}