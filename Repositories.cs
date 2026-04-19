namespace MorsadBackend.Core.Entities;

/// <summary>
/// تصنيفات الخبر — التصنيف + المشاعر من Claude
/// </summary>
public class ArticleTag
{
    public int Id { get; set; }

    public int ArticleId { get; set; }

    /// <summary>FK → LookupMinor (Major = CATEGORY)</summary>
    public int CategoryMinorId { get; set; }

    /// <summary>FK → LookupMinor (Major = SENTIMENT)</summary>
    public int SentimentMinorId { get; set; }

    /// <summary>درجة الثقة من Claude 0.0 → 1.0</summary>
    public float ConfidenceScore { get; set; }

    public DateTime TaggedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public NewsArticle Article { get; set; } = null!;
    public LookupMinor CategoryMinor { get; set; } = null!;
    public LookupMinor SentimentMinor { get; set; } = null!;
}

/// <summary>
/// تقييم المخاطر الأمنية — يُحسب كل 6 ساعات
/// </summary>
public class RiskAssessment
{
    public int Id { get; set; }

    /// <summary>FK → LookupMinor (Major = RISK_TYPE) — أخبار زائفة، تأثير خارجي، إلخ</summary>
    public int RiskTypeMinorId { get; set; }

    public float Score { get; set; }

    /// <summary>FK → LookupMinor (Major = TREND) — up / down / stable</summary>
    public int TrendMinorId { get; set; }

    public string? TopCase { get; set; }

    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    // Navigation
    public LookupMinor RiskTypeMinor { get; set; } = null!;
    public LookupMinor TrendMinor { get; set; } = null!;
}
