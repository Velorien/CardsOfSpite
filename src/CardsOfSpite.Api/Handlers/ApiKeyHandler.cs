using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace CardsOfSpite.Api.Handlers;

class ApiKeyHandler : AuthenticationHandler<ApiKeyOptions>
{
    public ApiKeyHandler(
        IOptionsMonitor<ApiKeyOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("x-api-key", out var key))
            return Task.FromResult(AuthenticateResult.NoResult());

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var hash = Convert.ToHexString(hashBytes);

        if (Options.ApiKey != hash)
            return Task.FromResult(AuthenticateResult.NoResult());

        var identity = new ClaimsIdentity("ApiKey");
        var principal = new ClaimsPrincipal(identity);

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, "ApiKey")));
    }
}

class ApiKeyOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = null!;
}