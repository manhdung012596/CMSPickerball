using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddServerSideBlazor();
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
builder.Services.AddScoped<SoccerPitchMvc.Services.IDashboardService, SoccerPitchMvc.Services.DashboardService>();
builder.Services.AddScoped<SoccerPitchMvc.Services.IPitchService, SoccerPitchMvc.Services.PitchService>();
builder.Services.AddScoped<SoccerPitchMvc.Services.IBookingService, SoccerPitchMvc.Services.BookingService>();
builder.Services.AddScoped<SoccerPitchMvc.Services.ICustomerService, SoccerPitchMvc.Services.CustomerService>();
builder.Services.AddScoped<SoccerPitchMvc.Services.ILanguageService, SoccerPitchMvc.Services.LanguageService>();
builder.Services.AddScoped<SoccerPitchMvc.Services.IArticleService, SoccerPitchMvc.Services.ArticleService>();
builder.Services.AddScoped<SoccerPitchMvc.Services.IMasterdataService, SoccerPitchMvc.Services.MasterdataService>();
builder.Services.AddScoped<SoccerPitchMvc.Services.IPromotionService, SoccerPitchMvc.Services.PromotionService>();
builder.Services.AddScoped<SoccerPitchMvc.Services.IShopService, SoccerPitchMvc.Services.ShopService>();
builder.Services.AddScoped<SoccerPitchMvc.Services.IFinanceService, SoccerPitchMvc.Services.FinanceService>();
builder.Services.AddSingleton<AutoMapper.IMapper>(sp => new AutoMapper.MapperConfiguration(cfg =>
{
    cfg.AddProfile(new SoccerPitchMvc.Data.MappingProfile());
}, sp.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()).CreateMapper());
builder.Services.AddScoped<SoccerPitchMvc.Services.ToastService>();

var app = builder.Build();

// Thiết lập Culture mặc định thành tiếng Việt (vi-VN)
var defaultCulture = new System.Globalization.CultureInfo("vi-VN");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("vi-VN")
    .AddSupportedCultures("vi-VN")
    .AddSupportedUICultures("vi-VN");
app.UseRequestLocalization(localizationOptions);

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
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToController("Index", "Admin");

app.Run();
