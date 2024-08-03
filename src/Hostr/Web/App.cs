using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Hostr.Web;

public static class App
{
    public static WebApplication Make(Cx cx)
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
         .AddJwtBearer(options =>
         {
             options.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateLifetime = true,
                 ValidateIssuerSigningKey = true,
                 IssuerSigningKey = cx.JwtKey
             };
         });

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

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors(corsPolicyId);

        app.MapGet("/ping", () => "pong");

        new Routes.Login().Bind(app);

        app.MapPost("/stop", () => app.StopAsync()).
            RequireAuthorization();

        return app;
    }

}