var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy_DO_NOT_RUN_IN_PRODUCTION", builder =>
    {
        builder.AllowAnyOrigin();
    });
});

var app = builder.Build();
app.UseCors();
app.MapReverseProxy();
app.MapGet("/.hello", () => "Hello from the Gateway!");
app.Run();
