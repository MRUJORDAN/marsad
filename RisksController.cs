using Microsoft.AspNetCore.Mvc;
using MorsadBackend.Api.DTOs;
using MorsadBackend.Core.Interfaces;

namespace MorsadBackend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RisksController : ControllerBase
{
    private readonly IRiskRepository _risks;

    public RisksController(IRiskRepository risks) => _risks = risks;

    /// GET /api/risks/latest
    [HttpGet("latest")]
    public async Task<ActionResult<RisksResponse>> GetLatest()
    {
        var assessments = (await _risks.GetLatestAssessmentsAsync()).ToList();
        if (!assessments.Any())
            return Ok(new RisksResponse(Enumerable.Empty<RiskDto>(), 0, DateTime.UtcNow));

        var dtos = assessments
            .OrderByDescending(r => r.Score)
            .Select(r => new RiskDto(
                r.RiskTypeMinor?.Code    ?? "",
                r.RiskTypeMinor?.NameAr  ?? "",
                r.RiskTypeMinor?.Color   ?? "#888888",
                r.Score,
                r.TrendMinor?.Code ?? "TREND_STABLE",
                r.TopCase
            ));

        var overall = assessments.Any() ? assessments.Average(r => r.Score) : 0;

        return Ok(new RisksResponse(dtos, (float)overall, assessments.First().AssessedAt));
    }
}
