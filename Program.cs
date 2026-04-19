using Microsoft.EntityFrameworkCore;
using MorsadBackend.Core.Interfaces;
using MorsadBackend.Infrastructure.Data;
using MorsadBackend.Infrastructure.Repositories;
using MorsadBackend.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ── DB ────────────────────────────────────────────────────────
builder.Services.AddDbContext<MorsadDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("MorsadDb"),
        sql => sql.CommandTimeout(60)));

// ── Repos ─────────────────────────────────────────────────────
builder.Services.AddScoped<INewsRepository,   NewsRepository>();
builder.Services.AddScoped<ILookupRepository, LookupRepository>();
builder.Services.AddScoped<IRiskRepository,   RiskRepository>();

// ── Worker (تشتغل كل 6 ساعات جنباً إلى جنب مع الـ API) ───────
builder.Services.AddScoped<RssFetcherService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ClaudeAnalyzerService>(sp =>
{
    var news   = sp.GetRequiredService<INewsRepository>();
    var lookup = sp.GetRequiredService<ILookupRepository>();
    var risk   = sp.GetRequiredService<IRiskRepository>();
    var http   = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    var log    = sp.GetRequiredService<ILogger<ClaudeAnalyzerService>>();
    var key    = builder.Configuration["Claude:ApiKey"] ?? "";
    return new ClaudeAnalyzerService(news, lookup, risk, http, key, log);
});
builder.Services.AddHostedService<MorsadBackend.Api.MorsadWorkerHosted>();

// ── CORS — يسمح لمرصاد HTML بالاتصال ─────────────────────────
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddControllers();

var app = builder.Build();

// تطبيق Migrations تلقائياً
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MorsadDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors();
app.MapControllers();
app.MapGet("/", () => "مرصاد API — running");

await app.RunAsync();
