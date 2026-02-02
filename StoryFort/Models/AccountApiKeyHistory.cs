using System;

namespace StoryFort.Models;

public class AccountApiKeyHistory
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string ProtectedKey { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = false; // previous keys archived will be false

    public Account? Account { get; set; }
}
