using System;
using System.Threading;
using System.Threading.Tasks;
using App.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EFCoreTests;

/// <summary>
/// Stub class for testing purposes
/// Context is generated only once and then reused
/// This is because every service uses it's own context but we need shared
/// </summary>
public abstract class DbFactory : IDbContextFactory<ApplicationDbContext>,
    IDisposable
{
    private const string ConnectionString =
        @"Host={0};Database=efcoretest_{1};Username={2};Password={3}";

    private string _databaseName;

    public DbFactory(string name)
    {
        // Set new db name to use
        _databaseName = name;
        using (var context = CreateDbContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            // While not something to do in production code it is fine for testing.
            // ReSharper disable once VirtualMemberCallInConstructor
            FillData(context);
            context.SaveChanges();
        }
    }

    public Task<ApplicationDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = new())
    {
        return Task.FromResult(CreateDbContext());
    }

    public ApplicationDbContext CreateDbContext()
    {
        var _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json");
        
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env != null)
        {
            _configuration.AddJsonFile($"appsettings.{env}.json", true);
        }
        var configuration = _configuration.AddEnvironmentVariables().Build();
        var pass = configuration.GetConnectionString("PostgresPassword");
        var user = configuration.GetConnectionString("PostgresUsername");
        var host = configuration.GetConnectionString("PostgresHost");
        var conn_string = string.Format(ConnectionString, host, _databaseName,
            user, pass);
        return new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(conn_string, o => o.UseNodaTime()).Options);
    }

    public void Dispose()
    {
        using var context = CreateDbContext();
        context.Database.EnsureDeleted();
    }

    protected abstract void FillData(ApplicationDbContext context);
}