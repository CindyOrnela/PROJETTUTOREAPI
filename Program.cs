using System.Text;
using AlertApi.Services;
using KwattAlertAPI.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ?? Ajout des services
builder.Services.AddSingleton<FirebaseService>();
builder.Services.AddSingleton<VonageService>();
builder.Services.AddSingleton<ESP32Service>();
builder.Services.AddSingleton<MqttService>();

builder.Services.AddControllers();

// ?? Configuration de l'authentification JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "KwattAlertAPI",
        ValidAudience = "KwattUsers",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("ta-clé-très-longue-et-sécurisée"))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ?? Middleware dans le bon ordre
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/test", () => "Test route OK");

app.Run();