namespace MorsadBackend.Core.Entities;

/// <summary>
/// مصادر الأخبار المعتمدة — بترا، عمون، سرايا، إلخ
/// </summary>
public class NewsSource
{
    public int Id { get; set; }

    /// <summary>FK → LookupMinor (Major = SOURCE_TYPE)</summary>
    public int TypeMinorId { get; set; }

    public string NameAr { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? RssUrl { get; set; }
    public string Color { get; set; } = "#888888";
    public bool IsOfficial { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastFetched { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public LookupMinor TypeMinor { get; set; } = null!;
    public ICollection<NewsArticle> Articles { get; set; } = new List<NewsArticle>();
}
