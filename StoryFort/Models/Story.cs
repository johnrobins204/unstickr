using System.Text.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoryFort.Models;

public class Story
{
    public int Id { get; set; }
    public string Title { get; set; } = "Untitled Story";
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    // New Continuous Content Field
    public string Content { get; set; } = "";
    // Pages removed: Content is now the single source of truth
    public ICollection<StoryEntityLink> EntityLinks { get; set; } = new List<StoryEntityLink>();

    public string Genre { get; set; } = "General";
    // Flexible story metadata (MVP)
    public string Metadata { get; set; } = "{}";

    [NotMapped]
    public Dictionary<string, JsonElement> MetadataMap
    {
        get
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Metadata) ?? new Dictionary<string, JsonElement>();
            }
            catch
            {
                return new Dictionary<string, JsonElement>();
            }
        }
        set
        {
            Metadata = JsonSerializer.Serialize(value);
        }
    }
}

