using Microsoft.EntityFrameworkCore;

namespace PrackyASusarny.Data.EFCoreServices;

public abstract class ModelService<T>
{
    protected IDbContextFactory<ApplicationDbContext> _dbFactory;

    public ModelService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }
}