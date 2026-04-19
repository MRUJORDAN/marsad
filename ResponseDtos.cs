using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using MorsadBackend.Core.Entities;

namespace MorsadBackend.Infrastructure.Data;

public class MorsadDbContext : DbContext
{
    public MorsadDbContext(DbContextOptions<MorsadDbContext> options) : base(options) { }

    public DbSet<LookupMajor>    LookupMajors    => Set<LookupMajor>();
    public DbSet<LookupMinor>    LookupMinors    => Set<LookupMinor>();
    public DbSet<NewsSource>     NewsSources     => Set<NewsSource>();
    public DbSet<NewsArticle>    NewsArticles    => Set<NewsArticle>();
    public DbSet<ArticleTag>     ArticleTags     => Set<ArticleTag>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ── LookupMajor ──────────────────────────────────────────
        mb.Entity<LookupMajor>(e =>
        {
            e.ToTable("LookupMajor");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.NameAr).HasMaxLength(150).IsRequired();
            e.Property(x => x.NameEn).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ── LookupMinor ──────────────────────────────────────────
        mb.Entity<LookupMinor>(e =>
        {
            e.ToTable("LookupMinor");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(80).IsRequired();
            e.Property(x => x.NameAr).HasMaxLength(150).IsRequired();
            e.Property(x => x.NameEn).HasMaxLength(150).IsRequired();
            e.Property(x => x.Color).HasMaxLength(20);
            e.HasIndex(x => x.Code).IsUnique();

            e.HasOne(x => x.Major)
             .WithMany(x => x.Minors)
             .HasForeignKey(x => x.MajorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── NewsSource ───────────────────────────────────────────
        mb.Entity<NewsSource>(e =>
        {
            e.ToTable("NewsSource");
            e.HasKey(x => x.Id);
            e.Property(x => x.NameAr).HasMaxLength(150).IsRequired();
            e.Property(x => x.BaseUrl).HasMaxLength(300).IsRequired();
            e.Property(x => x.RssUrl).HasMaxLength(300);
            e.Property(x => x.Color).HasMaxLength(20);

            e.HasOne(x => x.TypeMinor)
             .WithMany(x => x.NewsSources)
             .HasForeignKey(x => x.TypeMinorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── NewsArticle ──────────────────────────────────────────
        mb.Entity<NewsArticle>(e =>
        {
            e.ToTable("NewsArticle");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(500).IsRequired();
            e.Property(x => x.Link).HasMaxLength(1000).IsRequired();
            e.Property(x => x.Summary).HasMaxLength(2000);
            e.HasIndex(x => x.Link).IsUnique();
            e.HasIndex(x => x.PublishedAt);
            e.HasIndex(x => new { x.SourceId, x.PublishedAt });

            e.HasOne(x => x.Source)
             .WithMany(x => x.Articles)
             .HasForeignKey(x => x.SourceId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ArticleTag ───────────────────────────────────────────
        mb.Entity<ArticleTag>(e =>
        {
            e.ToTable("ArticleTag");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Article)
             .WithMany(x => x.Tags)
             .HasForeignKey(x => x.ArticleId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.CategoryMinor)
             .WithMany(x => x.CategoryTags)
             .HasForeignKey(x => x.CategoryMinorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.SentimentMinor)
             .WithMany(x => x.SentimentTags)
             .HasForeignKey(x => x.SentimentMinorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── RiskAssessment ───────────────────────────────────────
        mb.Entity<RiskAssessment>(e =>
        {
            e.ToTable("RiskAssessment");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AssessedAt);

            e.HasOne(x => x.RiskTypeMinor)
             .WithMany(x => x.RiskAssessments)
             .HasForeignKey(x => x.RiskTypeMinorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.TrendMinor)
             .WithMany()
             .HasForeignKey(x => x.TrendMinorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Seed Data — Lookup Tables ────────────────────────────
        SeedLookups(mb);
    }

    private static void SeedLookups(ModelBuilder mb)
    {
        // Major
        var majors = new[]
        {
            new LookupMajor { Id=1, Code="SOURCE_TYPE", NameAr="نوع المصدر",     NameEn="Source Type",  SortOrder=1, CreatedAt=new DateTime(2026,1,1) },
            new LookupMajor { Id=2, Code="CATEGORY",    NameAr="تصنيف الخبر",    NameEn="Category",     SortOrder=2, CreatedAt=new DateTime(2026,1,1) },
            new LookupMajor { Id=3, Code="SENTIMENT",   NameAr="تحليل المشاعر",  NameEn="Sentiment",    SortOrder=3, CreatedAt=new DateTime(2026,1,1) },
            new LookupMajor { Id=4, Code="RISK_TYPE",   NameAr="نوع المخاطرة",   NameEn="Risk Type",    SortOrder=4, CreatedAt=new DateTime(2026,1,1) },
            new LookupMajor { Id=5, Code="TREND",       NameAr="الاتجاه",        NameEn="Trend",        SortOrder=5, CreatedAt=new DateTime(2026,1,1) },
        };
        mb.Entity<LookupMajor>().HasData(majors);

        // Minor
        mb.Entity<LookupMinor>().HasData(
            // SOURCE_TYPE
            new LookupMinor { Id=1,  MajorId=1, Code="SRC_OFFICIAL",  NameAr="رسمي",               NameEn="Official",          Color="#c8a84b", SortOrder=1, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=2,  MajorId=1, Code="SRC_INDEP",     NameAr="مستقل",              NameEn="Independent",       Color="#00bcd4", SortOrder=2, CreatedAt=new DateTime(2026,1,1) },
            // CATEGORY
            new LookupMinor { Id=10, MajorId=2, Code="CAT_POL",       NameAr="سياسة",              NameEn="Politics",          Color="#2f81f7", SortOrder=1, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=11, MajorId=2, Code="CAT_ECO",       NameAr="اقتصاد",             NameEn="Economy",           Color="#3fb950", SortOrder=2, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=12, MajorId=2, Code="CAT_SEC",       NameAr="أمن",                NameEn="Security",          Color="#f85149", SortOrder=3, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=13, MajorId=2, Code="CAT_SOC",       NameAr="مجتمع",              NameEn="Society",           Color="#d29922", SortOrder=4, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=14, MajorId=2, Code="CAT_INT",       NameAr="دولي",               NameEn="International",     Color="#8957e5", SortOrder=5, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=15, MajorId=2, Code="CAT_SPT",       NameAr="رياضة",              NameEn="Sports",            Color="#39d0d8", SortOrder=6, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=16, MajorId=2, Code="CAT_EDU",       NameAr="تعليم",              NameEn="Education",         Color="#e3782e", SortOrder=7, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=17, MajorId=2, Code="CAT_TECH",      NameAr="تكنولوجيا",          NameEn="Technology",        Color="#00bcd4", SortOrder=8, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=18, MajorId=2, Code="CAT_ENV",       NameAr="بيئة",               NameEn="Environment",       Color="#26a65b", SortOrder=9, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=19, MajorId=2, Code="CAT_MED",       NameAr="إعلام",              NameEn="Media",             Color="#c8a84b", SortOrder=10, CreatedAt=new DateTime(2026,1,1) },
            // SENTIMENT
            new LookupMinor { Id=20, MajorId=3, Code="SENT_POS",      NameAr="إيجابي",             NameEn="Positive",          Color="#3fb950", SortOrder=1, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=21, MajorId=3, Code="SENT_NEG",      NameAr="سلبي",               NameEn="Negative",          Color="#f85149", SortOrder=2, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=22, MajorId=3, Code="SENT_NEU",      NameAr="محايد",              NameEn="Neutral",           Color="#8b949e", SortOrder=3, CreatedAt=new DateTime(2026,1,1) },
            // RISK_TYPE
            new LookupMinor { Id=30, MajorId=4, Code="RISK_FAKE",     NameAr="أخبار زائفة",        NameEn="Fake News",         Color="#f85149", SortOrder=1, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=31, MajorId=4, Code="RISK_FOREIGN",  NameAr="تأثير خارجي",        NameEn="Foreign Influence", Color="#e3782e", SortOrder=2, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=32, MajorId=4, Code="RISK_HATE",     NameAr="خطاب الكراهية",      NameEn="Hate Speech",       Color="#d29922", SortOrder=3, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=33, MajorId=4, Code="RISK_ECO",      NameAr="تحريض اقتصادي",      NameEn="Economic Incite",   Color="#8957e5", SortOrder=4, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=34, MajorId=4, Code="RISK_CYBER",    NameAr="تهديدات سيبرانية",   NameEn="Cyber Threats",     Color="#39d0d8", SortOrder=5, CreatedAt=new DateTime(2026,1,1) },
            // TREND
            new LookupMinor { Id=40, MajorId=5, Code="TREND_UP",      NameAr="تصاعدي",             NameEn="Up",                Color="#f85149", SortOrder=1, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=41, MajorId=5, Code="TREND_DOWN",    NameAr="تنازلي",             NameEn="Down",              Color="#3fb950", SortOrder=2, CreatedAt=new DateTime(2026,1,1) },
            new LookupMinor { Id=42, MajorId=5, Code="TREND_STABLE",  NameAr="مستقر",              NameEn="Stable",            Color="#8b949e", SortOrder=3, CreatedAt=new DateTime(2026,1,1) }
        );

        // News Sources
        mb.Entity<NewsSource>().HasData(
            new NewsSource { Id=1,  TypeMinorId=1, NameAr="وكالة بترا",         BaseUrl="https://petra.gov.jo",          RssUrl=null,                                             Color="#c8a84b", IsOfficial=true,  IsActive=true, CreatedAt=new DateTime(2026,1,1) },
            new NewsSource { Id=2,  TypeMinorId=1, NameAr="التلفزيون الأردني",  BaseUrl="https://www.jrtv.gov.jo",       RssUrl=null,                                             Color="#0056a3", IsOfficial=true,  IsActive=true, CreatedAt=new DateTime(2026,1,1) },
            new NewsSource { Id=3,  TypeMinorId=1, NameAr="قناة المملكة",       BaseUrl="https://www.almamlakatv.com",   RssUrl=null,                                             Color="#c0392b", IsOfficial=true,  IsActive=true, CreatedAt=new DateTime(2026,1,1) },
            new NewsSource { Id=4,  TypeMinorId=2, NameAr="عمون",               BaseUrl="https://www.ammonnews.net",     RssUrl="https://www.ammonnews.net/rss.php?type=n",       Color="#f4a261", IsOfficial=false, IsActive=true, CreatedAt=new DateTime(2026,1,1) },
            new NewsSource { Id=5,  TypeMinorId=2, NameAr="سرايا",              BaseUrl="https://www.sarayanews.com",    RssUrl="https://www.sarayanews.com/feed",                Color="#e76f51", IsOfficial=false, IsActive=true, CreatedAt=new DateTime(2026,1,1) },
            new NewsSource { Id=6,  TypeMinorId=2, NameAr="خبرني",              BaseUrl="https://khaberni.com",          RssUrl="https://khaberni.com/feed",                      Color="#457b9d", IsOfficial=false, IsActive=true, CreatedAt=new DateTime(2026,1,1) },
            new NewsSource { Id=7,  TypeMinorId=2, NameAr="jo24",               BaseUrl="https://www.jo24.net",          RssUrl="https://www.jo24.net/feed",                      Color="#2a9d8f", IsOfficial=false, IsActive=true, CreatedAt=new DateTime(2026,1,1) },
            new NewsSource { Id=8,  TypeMinorId=2, NameAr="الوكيل",             BaseUrl="https://alwakelwebsite.com",    RssUrl="https://alwakelwebsite.com/feed",                Color="#8e44ad", IsOfficial=false, IsActive=true, CreatedAt=new DateTime(2026,1,1) },
            new NewsSource { Id=9,  TypeMinorId=2, NameAr="رؤيا",               BaseUrl="https://www.royanews.tv",       RssUrl="https://www.royanews.tv/feed",                   Color="#e74c3c", IsOfficial=false, IsActive=true, CreatedAt=new DateTime(2026,1,1) },
            new NewsSource { Id=10, TypeMinorId=2, NameAr="أخبار الأردن",       BaseUrl="https://www.akhbaralyom.net",   RssUrl="https://www.akhbaralyom.net/feed",               Color="#1a6fa4", IsOfficial=false, IsActive=true, CreatedAt=new DateTime(2026,1,1) }
        );
    }
}
