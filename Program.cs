using System.Text;
using Microsoft.EntityFrameworkCore;
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

// Entity Framework Core için SQL Server bağlantısı
builder.Services.AddDbContext<MessagingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,     // Gerekirse true yapıp issuer ayarlayın
        ValidateAudience = false    // Gerekirse true yapıp audience ayarlayın
    };
});

builder.Services.AddAuthorization();

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

// Yetkilendirme için auth middleware sırası
app.UseAuthentication();
app.UseAuthorization();

// SignalR endpoint'i ekleniyor
app.MapHub<ChatHub>("/chatHub");

app.MapControllers();

app.Run();