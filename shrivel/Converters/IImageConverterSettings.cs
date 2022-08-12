namespace shrivel.Converters;

public interface IImageConverterSettings
{

    public string Input { get; } 

    public string Output { get; } 
    
    public int[] Sizes { get;  } 
    public string FileNameTemplate { get;  }    

}