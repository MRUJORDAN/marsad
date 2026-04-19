namespace MorsadBackend.Core.Entities;

/// <summary>
/// جدول التصنيفات الفرعية — مثال: سياسة، اقتصاد، إيجابي، سلبي
/// </summary>
public class LookupMinor
{
    public int Id { get; set; }

    public int MajorId { get; set; }

    /// <summary>مفتاح برمجي — مثال: CATEGORY_POL</summary>
    public string Code { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    /// <summary>لون عرض في الواجهة — مثال: #c8a84b</summary>
    public string? Color { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public LookupMajor Major { get; set; } = null!;
    public ICollection<NewsSource> NewsSources { get; set; } = new List<NewsSource>();
    public ICollection<ArticleTag> CategoryTags { get; set; } = new List<ArticleTag>();
    public ICollection<ArticleTag> SentimentTags { get; set; } = new List<ArticleTag>();
    public ICollection<RiskAssessment> RiskAssessments { get; set; } = new List<RiskAssessment>();
}
