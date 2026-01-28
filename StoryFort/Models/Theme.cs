namespace StoryFort.Models;

public class Theme
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string FontName { get; set; } = string.Empty; 
    public string BackgroundTexture { get; set; } = string.Empty; // CSS value
    public string SpritePath { get; set; } = string.Empty; // Path to image
    
    // Additional Metadata could go here
    public string Description { get; set; } = string.Empty;
}

