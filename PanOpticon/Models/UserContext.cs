namespace PanOpticon.Models;

public record UserContext(
    int UserId,
    int CompanyId,
    string CompanyTimeZone,
    int ProductId,
    int FeatureId,
    int PageId,
    string Token);
