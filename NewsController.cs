using Microsoft.AspNetCore.Mvc;
using MorsadBackend.Api.DTOs;
using MorsadBackend.Core.Interfaces;

namespace MorsadBackend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly INewsRepository   _news;
    private readonly ILookupRepository _lookup;

    public NewsController(INewsRepository news, ILookupRepository lookup)
    {
        _news   = news;
        _lookup = lookup;
    }

    /// GET /api/news/today
    [HttpGet("today")]
    public async Task<ActionResult<NewsFeedResponse>> GetToday()
    {
        var from     = DateTime.UtcNow.Date;
        var to       = from.AddDays(1);
        var articles = (await _news.GetTodayArticlesAsync(from, to)).ToList();

        var dtos = articles.Select(a =>
        {
            var tag = a.Tags.FirstOrDefault();
            return new NewsArticleDto(
                a.Id,
                a.Source?.NameAr ?? "",
                a.Source?.NameAr ?? "",
                a.Source?.Color  ?? "#888888",
                a.Source?.IsOfficial ?? false,
                a.Title,
                a.Link,
                FormatTime(a.PublishedAt),
                tag?.CategoryMinor?.NameAr  ?? "عام",
                tag?.CategoryMinor?.Color   ?? "#888888",
                MapSentiment(tag?.SentimentMinor?.Code),
                MapSentimentColor(tag?.SentimentMinor?.Code)
            );
        }).ToList();

        var pos = dtos.Count(d => d.Sentiment == "pos");
        var neg = dtos.Count(d => d.Sentiment == "neg");

        return Ok(new NewsFeedResponse(
            dtos.Count,
            dtos.Count(d => d.IsOfficial),
            dtos.Count(d => !d.IsOfficial),
            pos, neg,
            dtos.Count - pos - neg,
            dtos.FirstOrDefault()?.Title ?? "",
            dtos,
            DateTime.UtcNow
        ));
    }

    private static string FormatTime(DateTime dt)
    {
        var diff = DateTime.UtcNow - dt;
        if (diff.TotalMinutes < 60)  return $"منذ {(int)diff.TotalMinutes} دقيقة";
        if (diff.TotalHours   < 24)  return $"منذ {(int)diff.TotalHours}   ساعة";
        return $"منذ {(int)diff.TotalDays} يوم";
    }

    private static string MapSentiment(string? code) => code switch
    {
        "SENT_POS" => "pos",
        "SENT_NEG" => "neg",
        _          => "neu"
    };

    private static string MapSentimentColor(string? code) => code switch
    {
        "SENT_POS" => "#3fb950",
        "SENT_NEG" => "#f85149",
        _          => "#8b949e"
    };
}
