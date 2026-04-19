using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MorsadBackend.Api.DTOs;
using MorsadBackend.Infrastructure.Data;

namespace MorsadBackend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly MorsadDbContext _db;
    public HealthController(MorsadDbContext db) => _db = db;

    /// GET /api/health  — Render يستخدمه للـ health check
    [HttpGet]
    public async Task<ActionResult<HealthResponse>> Get()
    {
        var today   = DateTime.UtcNow.Date;
        var count   = await _db.NewsArticles.CountAsync(a => a.FetchedAt >= today);
        var sources = await _db.NewsSources.CountAsync(s => s.IsActive);

        return Ok(new HealthResponse(
            "ok",
            "مرصاد — نظام الإنذار المبكر للإعلام الرقمي",
            DateTime.UtcNow,
            count,
            sources
        ));
    }
}
