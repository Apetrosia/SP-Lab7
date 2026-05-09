using GreenSwampApp.Services;
using GreenSwampApp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using GreenSwampApp.Data;

using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=App_Data/greenswamp.db"));

builder.Services.AddRazorPages();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ReturnUrlParameter = "ReturnUrl";
    });

builder.Services.AddScoped<ICsvExportService, CsvExportService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

var app = builder.Build();

// Auto-create SQLite database and schema on first run.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
    SeedData.Initialize(dbContext);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMiddleware<LoggingMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePagesWithReExecute("/NotFound");

app.MapControllerRoute(
    name: "profile",
    pattern: "profile/{username}",
    defaults: new { controller = "Profile", action = "Index" });

app.MapControllerRoute(
    name: "postDetail",
    pattern: "feed/post/{postId}",
    defaults: new { controller = "Feed", action = "PostDetail" });

app.MapControllerRoute(
    name: "ponds",
    pattern: "ponds/{tag}",
    defaults: new { controller = "Ponds", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Feed}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

[Route("/subscribe")]
[ApiController]
public class SubscribeController : ControllerBase
{
    private readonly IEmailService _emailService;

    public SubscribeController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe([FromForm] EmailRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _emailService.SendSubscriptionConfirmationAsync(request.Email);
        return Ok(new { message = "Subscription successful" });
    }
}

public class EmailRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }
}