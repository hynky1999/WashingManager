global using NodaTime;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Auth.Models;
using PrackyASusarny.Data;
using PrackyASusarny.Data.EFCoreServices;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Utils;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString =
    builder.Configuration.GetConnectionString("PostgresConn");

Action<DbContextOptionsBuilder> contextOpts = options =>
    options.UseNpgsql(connectionString, o => o.UseNodaTime());
builder.Services.AddDbContextFactory<ApplicationDbContext>(contextOpts);
builder.Services.AddDbContext<ApplicationDbContext>(contextOpts);
builder.Services.AddIdentity<ApplicationUser, Role>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredUniqueChars = 1;
    }).AddEntityFrameworkStores<ApplicationDbContext>()
    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions
        .DefaultProvider)
    ;


builder.Services.AddRazorPages().AddRazorPagesOptions(options =>
{
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Admin",
        "UserManagement");
});
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<IBorrowPersonService, BorrowPersonService>();
builder.Services.AddSingleton<IReservationsService, ReservationService>();
builder.Services.AddSingleton<IUsageService, UsageService>();
builder.Services.AddSingleton<IUserService, UserService>();


builder.Services.AddSingleton<IBorrowService, BorrowService>();
builder.Services
    .AddSingleton<IUsageChartingService<WashingMachine>,
        UsageChartingService<WashingMachine>>();
builder.Services
    .AddSingleton<IUsageChartingService<DryingRoom>,
        UsageChartingService<DryingRoom>>();

builder.Services
    .AddSingleton<ICrudService<WashingMachine>, CrudService<WashingMachine>>();
builder.Services
    .AddSingleton<ICrudService<DryingRoom>, CrudService<DryingRoom>>();
builder.Services.AddSingleton<ICrudService<Manual>, CrudService<Manual>>();
builder.Services.AddSingleton<ICrudService<Location>, CrudService<Location>>();
builder.Services.AddSingleton<ICrudService<Borrow>, CrudService<Borrow>>();
builder.Services
    .AddSingleton<ICrudService<BorrowPerson>, CrudService<BorrowPerson>>();
builder.Services
    .AddSingleton<ICrudService<BorrowableEntity>,
        CrudService<BorrowableEntity>>();
builder.Services.AddSingleton<IClock, SystemClock>(_ => SystemClock.Instance);
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
builder.Services.AddSingleton<IUsageService, UsageService>();
builder.Services
    .AddSingleton<IBorrowableEntityService, BorrowableEntityService>();
builder.Services.AddSingleton<ILocationService, LocationService>();
builder.Services.AddAntDesign();


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.UserManagement,
        policy => policy.RequireClaim(Claims.ManageUsers, true.ToString()));
    options.AddPolicy(Policies.ModelManagement,
        policy => policy.RequireClaim(Claims.ManageModels, true.ToString()));
    options.AddPolicy(Policies.BorrowManagement,
        policy => policy.RequireClaim(Claims.ManageBorrows, true.ToString()));
});

builder.Services.AddLocalization(options =>
    options.ResourcesPath = "Resources");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var factory = services
        .GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    var context = factory.CreateDbContext();
    await context.Database.EnsureCreatedAsync();
    var pass = builder.Configuration.GetValue<string>("SeedPW");
    await SeedData.Initialize(services, pass);
}

app.Run();