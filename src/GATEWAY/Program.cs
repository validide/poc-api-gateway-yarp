using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

const string Id4Authority = "https://demo.identityserver.io";
const string Id5Authority = "https://demo.duendesoftware.com";

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
JwtSecurityTokenHandler.DefaultInboundClaimFilter.Clear();

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services
    .AddCors(options =>
    {
        options.AddPolicy("AllowAllPolicy_DO_NOT_RUN_IN_PRODUCTION", builder =>
        {
            builder.AllowAnyOrigin();
        });
    });

builder.Services
    .AddAuthentication()
    .AddCookie("cookie-id4", ConfigureCookieAuthenticationOptions())
    .AddOpenIdConnect("oidc-cookie-id4", ConfigureOpenIdOptions("id4", Id4Authority, "cookie-id4"))
    .AddJwtBearer("bearer-id4", ConfigureJwtBearerOptions(Id4Authority))
    .AddCookie("cookie-id5", ConfigureCookieAuthenticationOptions())
    .AddOpenIdConnect("oidc-cookie-id5", ConfigureOpenIdOptions("id5", Id5Authority, "cookie-id5"))
    .AddJwtBearer("bearer-id5", ConfigureJwtBearerOptions(Id5Authority));

builder.Services
    .AddAuthorization(options =>
    {
        options.AddPolicy(
            "cookie-id4-policy",
            policy =>
            {
                policy.AddAuthenticationSchemes("oidc-cookie-id4");
                policy.RequireAssertion(GetAuthorizationHandler(Id4Authority));
            }
        );

        options.AddPolicy(
            "bearer-id4-policy",
            policy =>
            {
                policy.AddAuthenticationSchemes("bearer-id4");
                policy.RequireAssertion(GetAuthorizationHandler(Id4Authority));
            }
        );

        options.AddPolicy(
            "cookie-id5-policy",
            policy =>
            {
                policy.AddAuthenticationSchemes("oidc-cookie-id5");
                policy.RequireAssertion(GetAuthorizationHandler(Id5Authority));
            }
        );

        options.AddPolicy(
            "bearer-id5-policy",
            policy =>
            {
                policy.AddAuthenticationSchemes("bearer-id5");
                policy.RequireAssertion(GetAuthorizationHandler(Id5Authority));
            }
        );
    });

var app = builder.Build();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.MapGet("/.hello", async context =>
{
    context.Response.StatusCode = (int)HttpStatusCode.OK;
    await context.Response.WriteAsync("Hello from the Gateway!").ConfigureAwait(false);
});
app.Run();


static Func<AuthorizationHandlerContext, bool> GetAuthorizationHandler(string issuer)
{
    return ctx =>
    {
        if (!(ctx.User.Identity?.IsAuthenticated ?? false))
            return false;

        return ctx.User.Claims.Any(a => String.Equals(a.Issuer, issuer, StringComparison.OrdinalIgnoreCase));
    };
}

static Action<OpenIdConnectOptions> ConfigureOpenIdOptions(string discriminator, string authority, string signInScheme)
{
    return options =>
    {
        options.NonceCookie.Name = $"oidc-nonce-{discriminator}.";
        //options.NonceCookie.SameSite = SameSiteMode.Strict;
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.Name = $"oidc-correlation-{discriminator}.";
        //options.CorrelationCookie.SameSite = SameSiteMode.Strict;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CallbackPath = $"/signin-oidc-{discriminator}";

        options.SignInScheme = signInScheme;
        options.Authority = authority;

        options.ClientId = "interactive.public";
        // options.ClientSecret = "secret";


        // code flow + PKCE (PKCE is turned on by default)
        options.ResponseType = "code";
        options.UsePkce = true;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        // keeps id_token smaller
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
    };
}

static Action<JwtBearerOptions> ConfigureJwtBearerOptions(string authority)
{
    return options =>
    {
        options.Authority = authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    };
}

static Action<CookieAuthenticationOptions> ConfigureCookieAuthenticationOptions()
{
    return options =>
    {
        options.CookieManager = new ChunkingCookieManager();
        //options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    };
}
