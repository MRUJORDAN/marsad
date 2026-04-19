// ═══════════════════════════════════════════════════
// مرصاد Backend — ملف واحد كامل
// كل الكود في Program.cs — لا مجلدات لا مشاريع فرعية
// ═══════════════════════════════════════════════════

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddHttpClient();
builder.Services.AddHostedService<MorsadWorker>();
builder.Services.AddSingleton<NewsStore>();

var app = builder.Build();
app.UseCors();

// ── Health Check ──────────────────────────────────
app.MapGet("/", () => Results.Ok(new {
    status  = "ok",
    service = "مرصاد API",
    time    = DateTime.UtcNow
}));

app.MapGet("/api/health", (NewsStore store) => Results.Ok(new {
    status       = "ok",
    service      = "مرصاد — نظام الإنذار المبكر للإعلام الرقمي",
    totalNews    = store.News.Count,
    lastFetch    = store.LastFetch,
    serverTime   = DateTime.UtcNow
}));

// ── أخبار اليوم ───────────────────────────────────
app.MapGet("/api/news/today", (NewsStore store) =>
{
    var news = store.News.OrderByDescending(n => n.FetchedAt).Take(50).ToList();
    var pos  = news.Count(n => n.Sentiment == "pos");
    var neg  = news.Count(n => n.Sentiment == "neg");
    return Results.Ok(new {
        total       = news.Count,
        positive    = pos,
        negative    = neg,
        neutral     = news.Count - pos - neg,
        topHeadline = news.FirstOrDefault()?.Title ?? "",
        articles    = news.Select(n => new {
            source      = n.Source,
            sourceName  = n.SourceName,
            color       = n.Color,
            isOfficial  = n.IsOfficial,
            title       = n.Title,
            link        = n.Link,
            time        = TimeAgo(n.FetchedAt),
            category    = n.Category,
            sentiment   = n.Sentiment,
        }),
        fetchedAt = store.LastFetch
    });
});

// ── مخاطر أمنية ───────────────────────────────────
app.MapGet("/api/risks/latest", (NewsStore store) =>
{
    var risks = store.Risks;
    if (!risks.Any())
        return Results.Ok(new { risks = Array.Empty<object>(), overall = 0, assessedAt = DateTime.UtcNow });

    return Results.Ok(new {
        risks     = risks,
        overall   = risks.Average(r => r.Score),
        assessedAt = store.LastFetch
    });
});

await app.RunAsync();

// ── Helper ────────────────────────────────────────
static string TimeAgo(DateTime dt)
{
    var diff = DateTime.UtcNow - dt;
    if (diff.TotalMinutes < 60)   return $"منذ {(int)diff.TotalMinutes} دقيقة";
    if (diff.TotalHours   < 24)   return $"منذ {(int)diff.TotalHours} ساعة";
    return $"منذ {(int)diff.TotalDays} يوم";
}

// ══════════════════════════════════════════════════
// مخزن الأخبار في الذاكرة
// ══════════════════════════════════════════════════
public class NewsStore
{
    public List<NewsItem>  News     { get; set; } = new();
    public List<RiskItem>  Risks    { get; set; } = new();
    public DateTime        LastFetch { get; set; } = DateTime.UtcNow;
}

public class NewsItem
{
    public string Source     { get; set; } = "";
    public string SourceName { get; set; } = "";
    public string Color      { get; set; } = "#888";
    public bool   IsOfficial { get; set; }
    public string Title      { get; set; } = "";
    public string Link       { get; set; } = "#";
    public string Category   { get; set; } = "عام";
    public string Sentiment  { get; set; } = "neu";
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}

public class RiskItem
{
    public string Code    { get; set; } = "";
    public string NameAr  { get; set; } = "";
    public string Color   { get; set; } = "#888";
    public float  Score   { get; set; }
    public string Trend   { get; set; } = "stable";
    public string TopCase { get; set; } = "";
}

// ══════════════════════════════════════════════════
// Background Worker — كل 6 ساعات
// ══════════════════════════════════════════════════
public class MorsadWorker : BackgroundService
{
    private readonly NewsStore     _store;
    private readonly IHttpClientFactory _http;
    private readonly ILogger<MorsadWorker> _log;
    private readonly string        _claudeKey;

    private static readonly (string Id, string Name, string Color, bool Official, string? Rss)[] SOURCES = {
        ("petra",   "وكالة بترا",      "#c8a84b", true,  null),
        ("jrtv",    "التلفزيون الأردني","#0056a3", true,  null),
        ("mamlaka", "قناة المملكة",    "#c0392b", true,  null),
        ("ammon",   "عمون",            "#f4a261", false, "https://www.ammonnews.net/rss.php?type=n"),
        ("saraya",  "سرايا",           "#e76f51", false, "https://www.sarayanews.com/feed"),
        ("khaberni","خبرني",           "#457b9d", false, "https://khaberni.com/feed"),
        ("jo24",    "jo24",            "#2a9d8f", false, "https://www.jo24.net/feed"),
        ("wkeel",   "الوكيل",          "#8e44ad", false, "https://alwakelwebsite.com/feed"),
        ("roya",    "رؤيا",            "#e74c3c", false, "https://www.royanews.tv/feed"),
        ("akhbar",  "أخبار الأردن",    "#1a6fa4", false, "https://www.akhbaralyom.net/feed"),
    };

    private static readonly string[] PROXIES = {
        "https://api.allorigins.win/raw?url=",
        "https://corsproxy.io/?",
    };

    public MorsadWorker(NewsStore store, IHttpClientFactory http, ILogger<MorsadWorker> log, IConfiguration config)
    {
        _store     = store;
        _http      = http;
        _log       = log;
        _claudeKey = config["Claude:ApiKey"] ?? "";
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("مرصاد Worker بدأ");
        await RunCycle();
        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));
        while (await timer.WaitForNextTickAsync(ct))
            await RunCycle();
    }

    private async Task RunCycle()
    {
        _log.LogInformation("مرصاد: بدء دورة الرصد {T}", DateTime.Now.ToString("HH:mm"));
        try
        {
            var allNews = new List<NewsItem>();

            // ١. جلب RSS
            var client = _http.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            foreach (var src in SOURCES.Where(s => s.Rss != null))
            {
                try
                {
                    var items = await FetchRSS(client, src.Id, src.Name, src.Color, src.Official, src.Rss!);
                    allNews.AddRange(items);
                    _log.LogInformation("  [{Name}] {Count} خبر", src.Name, items.Count);
                }
                catch (Exception ex)
                {
                    _log.LogWarning("  [{Name}] فشل: {Msg}", src.Name, ex.Message);
                }
            }

            // ٢. المصادر الرسمية عبر Claude
            if (!string.IsNullOrEmpty(_claudeKey))
            {
                var official = await FetchOfficialNews(client);
                allNews.InsertRange(0, official);
            }

            // ٣. تحليل بـ Claude
            if (!string.IsNullOrEmpty(_claudeKey) && allNews.Count > 0)
                await AnalyzeNews(client, allNews);

            // ٤. تقييم المخاطر
            var risks = BuildRisks(allNews);

            // ٥. حفظ
            _store.News      = allNews;
            _store.Risks     = risks;
            _store.LastFetch = DateTime.UtcNow;

            _log.LogInformation("مرصاد: اكتملت الدورة — {Count} خبر", allNews.Count);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "خطأ في دورة مرصاد");
        }
    }

    private async Task<List<NewsItem>> FetchRSS(HttpClient client, string id, string name, string color, bool official, string rssUrl)
    {
        var result = new List<NewsItem>();
        foreach (var proxy in PROXIES)
        {
            try
            {
                var url  = proxy + Uri.EscapeDataString(rssUrl);
                var xml  = await client.GetStringAsync(url);
                if (!xml.Contains("<item") && !xml.Contains("<entry")) continue;
                var doc  = XDocument.Parse(xml);
                var ns   = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;
                var items= doc.Descendants("item").Concat(doc.Descendants(ns + "entry")).Take(6);
                foreach (var item in items)
                {
                    var title = item.Element("title")?.Value?.Trim() ?? "";
                    if (title.Length < 10) continue;
                    var link  = item.Element("link")?.Value?.Trim()
                             ?? item.Element("guid")?.Value?.Trim() ?? rssUrl;
                    result.Add(new NewsItem {
                        Source    = id,   SourceName = name,
                        Color     = color, IsOfficial = official,
                        Title     = title, Link       = link,
                        Category  = "عام", Sentiment  = "neu",
                        FetchedAt = DateTime.UtcNow
                    });
                }
                if (result.Count > 0) break;
            }
            catch { continue; }
        }
        return result;
    }

    private async Task<List<NewsItem>> FetchOfficialNews(HttpClient client)
    {
        if (string.IsNullOrEmpty(_claudeKey)) return new();
        try
        {
            var prompt = "ابحث في أحدث 3 أخبار من: وكالة بترا (petra.gov.jo)، التلفزيون الأردني (jrtv.gov.jo)، قناة المملكة (almamlakatv.com/categories/19).\nأرجع JSON فقط:\n{\"news\":[{\"source\":\"petra\",\"sourceName\":\"وكالة بترا\",\"color\":\"#c8a84b\",\"official\":true,\"title\":\"العنوان\",\"link\":\"الرابط\",\"category\":\"سياسة\",\"sentiment\":\"pos\"}]}";
            var text = await CallClaude(client, prompt, withSearch: true);
            if (text == null) return new();
            var m = System.Text.RegularExpressions.Regex.Match(text, @"\{[\s\S]*\}");
            if (!m.Success) return new();
            var parsed = JsonSerializer.Deserialize<JsonElement>(m.Value);
            var list   = new List<NewsItem>();
            foreach (var n in parsed.GetProperty("news").EnumerateArray())
            {
                list.Add(new NewsItem {
                    Source     = n.GetProperty("source").GetString()     ?? "",
                    SourceName = n.GetProperty("sourceName").GetString() ?? "",
                    Color      = n.TryGetProperty("color", out var c)    ? c.GetString() ?? "#888" : "#888",
                    IsOfficial = n.TryGetProperty("official", out var o) && o.GetBoolean(),
                    Title      = n.GetProperty("title").GetString()      ?? "",
                    Link       = n.TryGetProperty("link", out var l)     ? l.GetString() ?? "#" : "#",
                    Category   = n.TryGetProperty("category", out var ca)? ca.GetString() ?? "عام" : "عام",
                    Sentiment  = n.TryGetProperty("sentiment", out var s)? s.GetString() ?? "neu" : "neu",
                    FetchedAt  = DateTime.UtcNow
                });
            }
            return list;
        }
        catch { return new(); }
    }

    private async Task AnalyzeNews(HttpClient client, List<NewsItem> items)
    {
        try
        {
            var titles = string.Join("\n", items.Take(20).Select((n, i) => $"{i}. [{n.SourceName}] {n.Title}"));
            var prompt = $"صنّف هذه الأخبار الأردنية.\nلكل خبر: index، category (سياسة/اقتصاد/أمن/مجتمع/دولي/رياضة/تعليم/تكنولوجيا)، sentiment (pos/neg/neu).\nأرجع JSON فقط:\n{{\"r\":[{{\"i\":0,\"c\":\"سياسة\",\"s\":\"neu\"}}]}}\n\n{titles}";
            var text   = await CallClaude(client, prompt, withSearch: false);
            if (text == null) return;
            var m = System.Text.RegularExpressions.Regex.Match(text, @"\{[\s\S]*\}");
            if (!m.Success) return;
            var parsed = JsonSerializer.Deserialize<JsonElement>(m.Value);
            foreach (var r in parsed.GetProperty("r").EnumerateArray())
            {
                var idx = r.GetProperty("i").GetInt32();
                if (idx < items.Count)
                {
                    items[idx].Category = r.TryGetProperty("c", out var cc) ? cc.GetString() ?? "عام" : "عام";
                    items[idx].Sentiment= r.TryGetProperty("s", out var ss) ? ss.GetString() ?? "neu" : "neu";
                }
            }
        }
        catch { }
    }

    private List<RiskItem> BuildRisks(List<NewsItem> news)
    {
        var neg = news.Count(n => n.Sentiment == "neg");
        var total = Math.Max(news.Count, 1);
        var negRatio = (float)neg / total * 100;

        return new List<RiskItem> {
            new() { Code="RISK_FAKE",    NameAr="أخبار زائفة",      Color="#f85149", Score=Math.Min(negRatio*0.8f, 80), Trend="stable" },
            new() { Code="RISK_FOREIGN", NameAr="تأثير خارجي",      Color="#e3782e", Score=Math.Min(negRatio*0.6f, 70), Trend="stable" },
            new() { Code="RISK_HATE",    NameAr="خطاب الكراهية",    Color="#d29922", Score=Math.Min(negRatio*0.5f, 60), Trend="stable" },
            new() { Code="RISK_ECO",     NameAr="تحريض اقتصادي",   Color="#8957e5", Score=Math.Min(negRatio*0.4f, 55), Trend="stable" },
            new() { Code="RISK_CYBER",   NameAr="تهديدات سيبرانية", Color="#39d0d8", Score=Math.Min(negRatio*0.3f, 50), Trend="stable" },
        };
    }

    private async Task<string?> CallClaude(HttpClient client, string prompt, bool withSearch)
    {
        try
        {
            var body = new Dictionary<string, object> {
                ["model"]      = "claude-haiku-4-5-20251001",
                ["max_tokens"] = 1500,
                ["messages"]   = new[] { new { role = "user", content = prompt } }
            };
            if (withSearch)
                body["tools"] = new[] { new { type = "web_search_20250305", name = "web_search" } };

            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            req.Headers.Add("x-api-key", _claudeKey);
            req.Headers.Add("anthropic-version", "2023-06-01");
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var res  = await client.SendAsync(req);
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            var doc  = JsonSerializer.Deserialize<JsonElement>(json);
            return doc.GetProperty("content")[0].GetProperty("text").GetString();
        }
        catch { return null; }
    }
}
