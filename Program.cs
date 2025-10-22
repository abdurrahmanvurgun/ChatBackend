using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ChatApp.Backend.Services;
using ChatApp.Backend.Models;
using ChatBackend.Data;
using ChatBackend.Services;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.Backend.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Servislerin eklenmesi ve yapılandırılması
builder.Services.AddControllers()
    .AddNewtonsoftJson();

// CORS: front-end uygulamaları için izin ver (development için geniş bırakıldı, prod'da kısıtlayın)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Entity Framework Core için PostgreSQL (Npgsql) bağlantısı
builder.Services.AddDbContext<MessagingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servis bağımlılıkları
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IUserService, UserService>(); // User servisini ekledik

// SignalR servisi ekleniyor
builder.Services.AddSignalR();

// JWT ayarları
var jwtKey = builder.Configuration.GetValue<string>("Jwt:Key") ?? "VerySecretKey12345";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    // SignalR bağlantılarında token query string ile gönderiliyorsa bu event ile alınır
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"].FirstOrDefault();
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,     // Gerekirse true yapıp issuer ayarlayın
        ValidateAudience = false    // Gerekirse true yapıp audience ayarlayın
    };
});

builder.Services.AddAuthorization(options =>
{
    // AdminOnly policy checks for role claim 'Admin'
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Swagger yapılandırması
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Geliştirme ortamı için middleware eklemeleri
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS middleware must run early to handle preflight requests before authentication
app.UseCors();

// Yetkilendirme için auth middleware sırası
app.UseAuthentication();
app.UseAuthorization();

// SignalR endpoint'i ekleniyor
app.MapHub<ChatHub>("/chatHub");

app.MapControllers();

// Seed admin user and apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var config = services.GetRequiredService<IConfiguration>();
        var context = services.GetRequiredService<MessagingContext>();
        // Apply pending migrations (safe if DB already up-to-date)
        context.Database.Migrate();

        var adminEmail = config.GetValue<string>("Admin:Email") ?? "admin@example.com";
        var adminPassword = config.GetValue<string>("Admin:Password") ?? "Admin123!";
        var adminName = config.GetValue<string>("Admin:Name") ?? "Admin";
        var adminSurname = config.GetValue<string>("Admin:Surname") ?? "User";

        var existing = context.Users.FirstOrDefault(u => u.Email.ToLower() == adminEmail.ToLower());
        if (existing == null)
        {
            var admin = new ChatApp.Backend.Models.User
            {
                Name = adminName,
                Surname = adminSurname,
                Email = adminEmail,
                Username = "admin",
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow
            };

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<ChatApp.Backend.Models.User>();
            admin.PasswordHash = hasher.HashPassword(admin, adminPassword);

            context.Users.Add(admin);
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetService<ILogger<Program>>();
        logger?.LogError(ex, "Error while seeding admin user");
    }
}

app.Run();