using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

const string Id4Authority = "https://demo.identityserver.io/";
const string Id5Authority = "https://demo.duendesoftware.com/";

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
    .AddAuthentication(options =>
    {
        //options.DefaultScheme = "id4-cookie";
        //options.DefaultChallengeScheme = "id4-cookie-oidc";
    })
    .AddCookie("id4-cookie", options =>
    {
        options.CookieManager = new ChunkingCookieManager();
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddOpenIdConnect("id4-cookie-oidc", options =>
    {
        options.NonceCookie.Name = "id4-cookie-oidc-nonce";
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.Name = "id4-cookie-oidc-correlation";
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

        options.SignInScheme = "id4-cookie";
        options.Authority = Id4Authority;

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

    })
    .AddJwtBearer("id4-bearer", options =>
    {
        options.Authority = Id4Authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    })
    .AddCookie("id5-cookie", options =>
    {
        options.CookieManager = new ChunkingCookieManager();
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddOpenIdConnect("id5-cookie-oidc", options =>
    {
        options.NonceCookie.Name = "id5-cookie-oidc-nonce";
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.Name = "id5-cookie-oidc-correlation";
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

        options.SignInScheme = "id5-cookie";
        options.Authority = Id5Authority;

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
    })
    .AddJwtBearer("id5-bearer", options =>
    {
        options.Authority = Id5Authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services
    .AddAuthorization(options =>
    {
        options.AddPolicy(
            "id4-cookie-policy",
            policy =>
            {
                policy.AddAuthenticationSchemes("id4-cookie-oidc");
                policy.RequireAssertion(ctx => ctx.User.Identity?.AuthenticationType == "id4-cookie");
            }
        );

        options.AddPolicy(
            "id4-bearer-policy",
            policy =>
            {
                policy.AddAuthenticationSchemes("id4-bearer");
                policy.RequireAssertion(ctx => ctx.User.Identity?.AuthenticationType == "id4-bearer");
            }
        );

        options.AddPolicy(
            "id5-cookie-policy",
            policy =>
            {
                policy.AddAuthenticationSchemes("id5-cookie-oidc");
                policy.RequireAssertion(ctx => ctx.User.Identity?.AuthenticationType == "id5-cookie");
            }
        );

        options.AddPolicy(
            "id5-bearer-policy",
            policy =>
            {
                policy.AddAuthenticationSchemes("id5-bearer");
                policy.RequireAssertion(ctx => ctx.User.Identity?.AuthenticationType == "id5-bearer");
            }
        );
    });

var app = builder.Build();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.MapGet("/.hello", () => "Hello from the Gateway!");
app.Run();
