using Ecomm_project_1145_2.Data;
using Ecomm_project_1145_2.DataAccess.Repository;
using Ecomm_project_1145_2.DataAccess.Repository.IRepository;
using Ecomm_project_1145_2.Utiltity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var cs = builder.Configuration.GetConnectionString("conStr");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(cs));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>().
    AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();
//payment ke liye
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

//twilio
//var twilioAccountId = "";
//var twilioAuthToken = "";
//var twilioPhoneNumber = "";
//builder.Services.AddSingleton<ISmsService>(new twilios)
//Login Page Redirect
builder.Services.ConfigureApplicationCookie(Options =>
{
    Options.LoginPath = $"/Identity/Account/Login";
    Options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
    Options.LogoutPath = $"/Identity/Account/Logout";
});
// Open Authentication facebook google twitter
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "1600548857347465";
    options.AppSecret = "a765000351180262151de879148b6f41";
});
builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = "696960988778-4dnmq04vhs1l5ri9sd8jcr1uof04kdhp.apps.googleusercontent.com";
    options.ClientSecret = "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";
});
builder.Services.AddAuthentication().AddTwitter(options =>
{
    options.ConsumerKey = builder.Configuration[key: "Authentication:Twitter:ApiKey"];
    options.ConsumerSecret = builder.Configuration[key: "Authentication:Twitter:ApiKeySecret"];
});

//add session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
// builder.Services.AddScoped<ICoverTypeRepository, CoverTypeRepository>();
builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

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
//payment ke liye
StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSettings")["SecretKey"];
//add session
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
