using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data;

namespace EFCoreTests;

/// <summary>
/// Stub class for testing purposes
/// Context is generated only once and then reused
/// This is because every service uses it's own context but we need shared
/// </summary>
public abstract class DbFactory : IDbContextFactory<ApplicationDbContext>
{
    private const string ConnectionString =
        @"Host=localhost;Database=efcoretest;Username=hynky;Password=sirecek007";

    private static readonly object Lock = new();
    private static bool _databaseInitialized;

    public DbFactory()
    {
        lock (Lock)
        {
            if (!_databaseInitialized)
            {
                using (var context = CreateDbContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    // While not something to do in production code it is fine for testing.
                    // ReSharper disable once VirtualMemberCallInConstructor
                    FillData(context);
                    context.SaveChanges();
                }

                _databaseInitialized = true;
            }
        }
    }

    public Task<ApplicationDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = new())
    {
        return Task.FromResult(CreateDbContext());
    }

    public ApplicationDbContext CreateDbContext()
    {
        return new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString, o => o.UseNodaTime()).Options);
    }

    protected abstract void FillData(ApplicationDbContext context);
}