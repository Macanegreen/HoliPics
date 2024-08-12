using HoliPics.Areas.Identity.Data;
using HoliPics.Authorization;
using HoliPics.Data;
using HoliPics.Options;
using HoliPics.Services.Implementations;
using HoliPics.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));



builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<HoliPicsUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();



builder.Services.AddControllersWithViews();

// Authorization handlers.
builder.Services.AddScoped<IAuthorizationHandler,
                      AlbumIsOwnerAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler,
                      GuestAuthorizationHandler>();

builder.Services.Configure<AzureBlobOptions>(builder.Configuration.GetSection("AzureBlob"));

// Image service for handling communication with azure blob
builder.Services.AddTransient<IImageService, ImageService>();
// Album delete service for handling the deletion of albums when user is deleted
builder.Services.AddTransient<IAlbumDeleteService, AlbumDeleteService>();
// Email sender service for handling the email confirmation
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddTransient<IEmailSenderService, EmailSenderService>();
}
else { builder.Services.AddTransient<IEmailSenderService, AzureEmailSenderService>(); }

builder.Services.Configure<EmailSenderOptions>(options =>
{
    options.HostAddress = builder.Configuration["ExternalProviders:MailKit:SMTP:Address"];
    options.HostPort = Convert.ToInt32(builder.Configuration["ExternalProviders:MailKit:SMTP:Port"]);
    options.HostUsername = builder.Configuration["ExternalProviders:MailKit:SMTP:Account"];
    options.HostPassword = builder.Configuration["ExternalProviders:MailKit:SMTP:Password"];
    options.SenderEmail = builder.Configuration["ExternalProviders:MailKit:SMTP:SenderEmail"];
    options.SenderName = builder.Configuration["ExternalProviders:MailKit:SMTP:SenderName"];    
});

builder.Services.Configure<AzureEmailSenderOptions>(builder.Configuration.GetSection("AzureEmailService"));


builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.ValueCountLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapRazorPages();

app.Run();
