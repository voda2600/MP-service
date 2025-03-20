using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Diagnostics;
using MP.Domain;
using MP.Services;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddSingleton<ITicketManager>(_ =>
    new TicketTicketManager("DefaultContext")); 

builder.Services.AddSingleton<ITicketService, TicketService>();

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 65536 * 1024; 
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 65536 * 1024; 
    options.Limits.MaxRequestLineSize = 65536;
});

builder.WebHost.ConfigureKestrel(options => { options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(3600); });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Web application starting...");

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        var errorLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        errorLogger.LogError(exception, "Unhandled exception occurred.");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("Internal server error");
    });
});

app.Lifetime.ApplicationStopped.Register(() => { logger.LogInformation("Web application stopped."); });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllers();

app.Run();