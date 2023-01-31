global using NodaTime;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Auth.Models;
using PrackyASusarny.Auth.Utils;
using PrackyASusarny.Data;
using PrackyASusarny.Data.Constants;
using PrackyASusarny.Data.EFCoreServices;
using PrackyASusarny.Data.Models;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Middlewares;
using PrackyASusarny.ServerServices;
using PrackyASusarny.Utils;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString =
    $"Host={builder.Configuration.GetConnectionString("PostgresHost")};Database={builder.Configuration.GetConnectionString("PostgresDatabase")};Username={builder.Configuration.GetConnectionString("PostgresUsername")};Password={builder.Configuration.GetConnectionString("PostgresPassword")}";


builder.Configuration.GetConnectionString("PostgresConn");

void ContextOpts(DbContextOptionsBuilder options) =>
    options.UseNpgsql(connectionString, o => o.UseNodaTime());

builder.Services.AddSingleton<IContextHookMiddleware, ContextHookMiddleware>();

builder.Services.AddDbContextFactory<ApplicationDbContext>(
    (Action<DbContextOptionsBuilder>) ContextOpts);
builder.Services.AddDbContext<ApplicationDbContext>(
    (Action<DbContextOptionsBuilder>) ContextOpts);
builder.Services.AddIdentity<ApplicationUser, Role>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
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

// Constants
builder.Services.AddSingleton<IRates, Rates>();

builder.Services.AddSingleton<IReservationConstant, ReservationConstant>();

builder.Services.AddSingleton<IUsageConstants, UsageContants>();


// EF services

builder.Services.AddSingleton<ICurrencyService, CurrencyService>();
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

// ICrud Services
builder.Services
    .AddSingleton<ICrudService<WashingMachine>, CrudService<WashingMachine>>();
builder.Services
    .AddSingleton<ICrudService<DryingRoom>, CrudService<DryingRoom>>();
builder.Services.AddSingleton<ICrudService<Manual>, CrudService<Manual>>();
builder.Services.AddSingleton<ICrudService<Location>, CrudService<Location>>();
builder.Services.AddSingleton<ICrudService<Borrow>, CrudService<Borrow>>();
builder.Services
    .AddSingleton<ICrudService<Reservation>, CrudService<Reservation>>();
builder.Services
    .AddSingleton<ICrudService<BorrowPerson>, CrudService<BorrowPerson>>();
builder.Services
    .AddSingleton<ICrudService<BorrowableEntity>,
        CrudService<BorrowableEntity>>();
builder.Services
    .AddSingleton<ICrudService<ApplicationUser>,
        CrudService<ApplicationUser>>();


builder.Services.AddSingleton<IReservationManager, ReservationManager>();

builder.Services.AddHostedService<ReservationHostedService>();


builder.Services.AddSingleton<IClock, SystemClock>(_ => SystemClock.Instance);
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
builder.Services.AddSingleton<IUsageService, UsageService>();
builder.Services
    .AddSingleton<IBorrowableEntityService, BorrowableEntityService>();
builder.Services.AddSingleton<ILocationService, LocationService>();
builder.Services.AddAntDesign();


// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.UserManagement,
        policy => policy.RequireClaim(Claims.ManageUsers, true.ToString()));
    options.AddPolicy(Policies.ModelManagement,
        policy => policy.RequireClaim(Claims.ManageModels, true.ToString()));
    options.AddPolicy(Policies.BorrowManagement,
        policy => policy.RequireClaim(Claims.ManageBorrows, true.ToString()));
});

// Localization
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
    var resManager = services.GetService<IReservationManager>();
    var middleware = services.GetService<IContextHookMiddleware>();
    var beService = services.GetService<IBorrowableEntityService>();
    var roleManager = services.GetService<RoleManager<Role>>();
    var userManager = services.GetService<UserManager<ApplicationUser>>();
    var ctxFactory =
        services.GetService<IDbContextFactory<ApplicationDbContext>>();

    // admin User
    var admin = builder.Configuration.GetSection("Users:Admin:Cfg")
        .Get<ApplicationUser>();
    var adminPass = builder.Configuration
        .GetValue<string>("Users:Admin:Password");
    var manager = builder.Configuration.GetSection("Users:Manager:Cfg")
        .Get<ApplicationUser>();
    var managerPass =
        builder.Configuration.GetValue<string>("Users:Manager:Password");


    await SeedData.Initialize(ctxFactory!, userManager!, roleManager!, admin,
        adminPass, manager, managerPass, initData: false);
    ProgramInit.InitializeHooks(middleware!, resManager!);
    await ProgramInit.InitializeQueues(resManager!, ctxFactory!, beService!);
}

app.Run();