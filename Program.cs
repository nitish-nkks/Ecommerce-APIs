using System;
using System.Security.Claims;
using System.Text;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Mappings;
using Ecommerce_APIs.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

//Pincode
builder.Services.AddHttpClient<PincodeService>();

// Configure EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("EcommerceDb"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("EcommerceDb"))));


// Register custom services
builder.Services.AddScoped<JwtService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };
});

builder.Services.AddAuthorization();

// Add Controllers and JSON settings
builder.Services.AddControllers()
    .AddJsonOptions(x =>
        x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Ecommerce API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token.\n\nExample: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Sentry
builder.WebHost.UseSentry(o =>
{
    o.Dsn = "http://cef9dd8dfd068caa010a4acc13d3951c@172.16.2.183:9000/26"; // TEMP empty DSN
    o.TracesSampleRate = 1.0;
});

// Email Service
builder.Services.AddScoped<IEmailService, EmailService>();
// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddCors(options =>
{
        options.AddPolicy("AllowQA",
        policy => policy
            .WithOrigins("http://localhost:5173", "https://abc-api-qa.abisaio.com", "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();


app.UseCors("AllowQA");

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();