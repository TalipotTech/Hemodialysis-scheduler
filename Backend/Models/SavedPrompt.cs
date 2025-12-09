namespace HDScheduler.API.Models
{
    public class SavedPrompt
    {
        public int Id { get; set; }
        public string PromptText { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
