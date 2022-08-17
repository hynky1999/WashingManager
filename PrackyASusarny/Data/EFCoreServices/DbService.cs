using Microsoft.EntityFrameworkCore;

namespace PrackyASusarny.Data.EFCoreServices;

public class DbService
{
    protected readonly IDbContextFactory<ApplicationDbContext> DbFactory;

    public DbService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        DbFactory = dbFactory;
    }
}