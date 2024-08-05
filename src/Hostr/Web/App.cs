using Microsoft.AspNetCore.Authentication.JwtBearer;
using Hostr.Domain;

namespace Hostr.Web;

public static class App
{
    public static WebApplication Make(Cx cx)
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
         .AddJwtBearer(options => options.TokenValidationParameters = User.GetJwtValidationParameters(cx));

        var corsPolicyId = "defaultPolicy";

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: corsPolicyId,
                              policy =>
                              {
                                  policy
                                    .AllowAnyMethod()
                                    .AllowCredentials()
                                    .SetIsOriginAllowed((host) => true)
                                    .AllowAnyHeader();
                              });
        });

        builder.Services.AddAuthorization();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new RecordConverter());
        });

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors(corsPolicyId);

        app.MapGet("/ping", () => "pong");
        new Routes.Login().Bind(app);

        new Routes.GetCalendars().Bind(app);
        new Routes.GetEvents().Bind(app);

        app.MapPost("/stop", () => app.StopAsync()).
            RequireAuthorization();

        return app;
    }

}