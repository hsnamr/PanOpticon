using Microsoft.Extensions.Options;
using PanOpticon.Models;

namespace PanOpticon.Services;

public class AuthService
{
    private readonly PanOpticonOptions _options;

    public AuthService(IOptions<PanOpticonOptions> options)
    {
        _options = options.Value;
    }

    public UserContext? GetCurrentUser(string? authToken, string? authorization)
    {
        var token = authToken;
        if (string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authorization["Bearer ".Length..].Trim();
        }

        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        return new UserContext(
            UserId: 1,
            CompanyId: _options.DefaultCompanyId,
            CompanyTimeZone: _options.DefaultCompanyTimeZone,
            ProductId: _options.DefaultProductId,
            FeatureId: _options.DefaultFeatureId,
            PageId: _options.DefaultPageId,
            Token: token);
    }
}
