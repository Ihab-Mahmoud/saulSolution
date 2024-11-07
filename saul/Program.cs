using Microsoft.EntityFrameworkCore;
using Saul.DataAccess.Data;
using Saul.DataAccess.Repository;
using Saul.DataAccess.Repository.IRepository;
using Saul.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Saul.Utility;
using Stripe;
using Saul.DbInitializer;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(option => {
    option.LoginPath = $"/Identity/Account/Login";
    option.LogoutPath = $"/Identity/Account/Logout";
    option.AccessDeniedPath = $"/Identity/Account/AccessDenied";

});
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Sets session timeout
    options.Cookie.HttpOnly = true;                // Only accessible on the server side
    options.Cookie.IsEssential = true;             // Required if tracking consent feature is enabled
});
builder.Services.AddAuthentication()
        .AddFacebook(Options =>
        {
            Options.AppId = "1078703436694540";
            Options.AppSecret = "fbff0da80400233142c4fb38928b8479";
        });
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
StripeConfiguration.ApiKey=builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
seedDatabase();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();




void seedDatabase(){

    using (var scope = app.Services.CreateScope())
    {
      var dbInitializer =  scope.ServiceProvider.GetRequiredService<IDbInitializer>();
      dbInitializer.Initialize();
    }
}