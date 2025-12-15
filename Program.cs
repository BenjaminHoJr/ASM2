using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using WebApplication1.Models;
using WebApplication1.Services;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for deployment (allow all origins for demo, restrict in production)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Entity Framework Core with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwt = builder.Configuration.GetSection("Jwt");
var secret = jwt["Key"];
if (string.IsNullOrWhiteSpace(secret))
{
    throw new InvalidOperationException("Missing configuration: Jwt:Key. Add Jwt section to appsettings.json or set environment variable 'Jwt__Key'.");
}
var key = Encoding.UTF8.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // production: true + HTTPS
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "API Docs";
});

app.UseHttpsRedirection();
app.UseCors();
app.UseRouting();

app.UseAuthentication(); // <- phải trước UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Test send email once at startup (use a scope)
// using (var scope = app.Services.CreateScope())
// {
//     var svc = scope.ServiceProvider.GetRequiredService<EmailService>();
//     var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
//     try
//     {
//         // thay bằng email bạn muốn nhận; có thể lấy từ config
//         await svc.SendEmailAsync("tungtt64@fpt.edu.vn", "Hơi thở của Nước", "<p>Hello từ Program.cs</p>");
//         logger.LogInformation("Startup test email sent.");
//     }
//     catch (System.Exception ex)
//     {
//         logger.LogError(ex, "Failed to send startup test email.");
//     }
// }

//Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();