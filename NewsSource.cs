namespace MorsadBackend.Api.DTOs;

// ── News ──────────────────────────────────────────────────────

public record NewsArticleDto(
    int    Id,
    string Source,
    string SourceName,
    string Color,
    bool   IsOfficial,
    string Title,
    string Link,
    string Time,
    string Category,
    string CategoryColor,
    string Sentiment,
    string SentimentColor
);

public record NewsFeedResponse(
    int                      Total,
    int                      Official,
    int                      General,
    int                      Positive,
    int                      Negative,
    int                      Neutral,
    string                   TopHeadline,
    IEnumerable<NewsArticleDto> Articles,
    DateTime                 FetchedAt
);

// ── Risks ─────────────────────────────────────────────────────

public record RiskDto(
    string Code,
    string NameAr,
    string Color,
    float  Score,
    string Trend,
    string? TopCase
);

public record RisksResponse(
    IEnumerable<RiskDto> Risks,
    float                Overall,
    DateTime             AssessedAt
);

// ── Health ────────────────────────────────────────────────────

public record HealthResponse(
    string   Status,
    string   Service,
    DateTime ServerTime,
    int      TotalArticlesToday,
    int      TotalSources
);
