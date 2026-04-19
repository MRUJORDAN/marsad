namespace MorsadBackend.Core.Entities;

/// <summary>
/// الأخبار المجلوبة من المصادر
/// </summary>
public class NewsArticle
{
    public int Id { get; set; }

    public int SourceId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string? Summary { get; set; }

    public DateTime PublishedAt { get; set; }
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;

    /// <summary>هل تم تحليله بـ Claude؟</summary>
    public bool IsProcessed { get; set; }

    // Navigation
    public NewsSource Source { get; set; } = null!;
    public ICollection<ArticleTag> Tags { get; set; } = new List<ArticleTag>();
}
