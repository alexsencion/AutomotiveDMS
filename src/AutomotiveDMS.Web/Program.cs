
using AutomotiveDMS.Application.Extensions;
using AutomotiveDMS.Infrastructure.Extensions;
using AutomotiveDMS.Web.Extensions;
using AutomotiveDMS.Web.Middleware;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting AutomotiveDMS Web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    });

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplicationServices();
    builder.Services.AddWebServices(builder.Configuration);

    var app = builder.Build();

    await app.InitialiseDatabaseAsync();

    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseRouting();

    app.UseSession();

    app.UseAuthentication();

    app.UseAuthorization();

    app.UseMiddleware<AuditMiddleware>();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");

    Log.Information("AutomotiveDMS Web applicaton started successfully");

    await app.RunAsync();
}
catch(Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "AutomotiveDMS applicaton terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}