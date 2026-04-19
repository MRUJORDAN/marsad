namespace MorsadBackend.Api.DTOs;

// ── أخبار ──────────────────────────────────────────────────────
public record NewsArticleDto(
    int     Id,
    string  Source,
    string  SourceName,
    string  Color,
    bool    Official,
    string  Title,
    string  Link,
    string  Time,
    string  Category,
    string  Sentiment
);

public record NewsResponseDto(
    int                      Count,
    int                      OfficialCount,
    int                      GeneralCount,
    int                      PosCount,
    int                      NegCount,
    int                      NeuCount,
    string                   Ticker,
    IEnumerable<NewsArticleDto> News,
    DateTime                 FetchedAt
);

// ── مخاطر ──────────────────────────────────────────────────────
public record RiskDto(
    string  Id,
    string  Name,
    string  Color,
    float   Value,
    string  Trend,
    string? TopCase
);

public record RisksResponseDto(
    float              Overall,
    string             AlertLevel,
    IEnumerable<RiskDto> Risks,
    DateTime           AssessedAt
);

// ── صحة الخادم ─────────────────────────────────────────────────
public record HealthDto(
    string   Status,
    string   Service,
    int      TotalArticles,
    DateTime LastFetch
);
