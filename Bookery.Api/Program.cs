using Bookery.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

var ocelotConfiguration = builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", false, true)
    .AddEnvironmentVariables()
    .Build();

builder.Services.AddOcelot(ocelotConfiguration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = AuthConfig.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = AuthConfig.GetSymmetricSecurityKey(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path.HasValue && path.Value.Contains("Node") && !path.Value.EndsWith('/'))
    {
        context.Request.Path = path + "/";
    }

    await next.Invoke();
});

app.UseOcelot().Wait();

app.Run();