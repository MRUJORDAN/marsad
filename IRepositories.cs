namespace MorsadBackend.Core.Entities;

/// <summary>
/// جدول التصنيفات الرئيسية — مثال: SOURCE_TYPE, CATEGORY, SENTIMENT, RISK_TYPE
/// </summary>
public class LookupMajor
{
    public int Id { get; set; }

    /// <summary>مفتاح برمجي — مثال: CATEGORY</summary>
    public string Code { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<LookupMinor> Minors { get; set; } = new List<LookupMinor>();
}
