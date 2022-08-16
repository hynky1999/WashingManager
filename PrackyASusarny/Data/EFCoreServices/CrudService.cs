using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Utils;

namespace PrackyASusarny.Data.EFCoreServices;

public abstract class CrudService<T> : ICrudService<T> where T : class
{
    private readonly Func<ApplicationDbContext, DbSet<T>> _dbSetGetter;
    private readonly Func<T, object> _idGetter;
    private readonly Expression<Func<T, object>> _idGetterExpr;
    private readonly ILogger<CrudService<T>> _logger;
    protected readonly IDbContextFactory<ApplicationDbContext> DbFactory;

    public CrudService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<CrudService<T>> logger,
        Func<ApplicationDbContext, DbSet<T>> dbSetGetter, Expression<Func<T, object>> idGetter)
    {
        DbFactory = dbFactory;
        _dbSetGetter = dbSetGetter;
        _idGetterExpr = idGetter;
        _idGetter = idGetter.Compile();
        _logger = logger;
    }

    public CrudService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<CrudService<T>> logger)
    {
        DbFactory = dbFactory;
        _logger = logger;
        _dbSetGetter = GetDbSetGetter();
        _idGetterExpr = GetKeyGetterAndSetter(dbFactory);
        _idGetter = _idGetterExpr.Compile();
    }


    public async Task<List<T>> GetAllAsync(bool eager = false)
    {
        using var dbContext = await DbFactory.CreateDbContextAsync();
        var dbSet = _dbSetGetter(dbContext);
        var query = dbSet.AsQueryable();
        if (eager)
        {
            query = query.MakeEager(dbSet.EntityType);
        }

        return await query.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(object id, bool eager)
    {
        await using var dbContext = await DbFactory.CreateDbContextAsync();
        var dbSet = _dbSetGetter(dbContext);
        var query = dbSet.AsQueryable();
        if (eager)
        {
            query = query.MakeEager(dbSet.EntityType);
        }

        var result = query.Where(GetIdEquals(id)).SingleOrDefaultAsync();
        return await result;
    }

    public async Task CreateAsync(T entity)
    {
        await using var dbContext = await DbFactory.CreateDbContextAsync();
        var dbset = _dbSetGetter(dbContext);
        dbset.Attach(entity);
        dbContext.Entry(entity).State = EntityState.Added;
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            throw new Errors.Folder.DbUpdateException(e.Message);
        }
    }

    public async Task UpdateAsync(T entity)
    {
        await using var dbContext = await DbFactory.CreateDbContextAsync();
        var dbset = _dbSetGetter(dbContext);
        _dbSetGetter(dbContext).Attach(entity);
        dbContext.Entry(entity).State = EntityState.Modified;
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e.Message);
            throw new Errors.Folder.DbUpdateException(e.Message);
        }
    }

    public async Task DeleteAsync(T entity)
    {
        await using var dbContext = await DbFactory.CreateDbContextAsync();
        var dbSet = _dbSetGetter(dbContext);
        dbSet.Attach(entity);
        dbSet.Remove(entity);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e.Message);
            throw new Errors.Folder.DbUpdateException(e.Message);
        }
    }

    public object GetId(T entity)
    {
        return _idGetter(entity);
    }

    public Expression<Func<T, bool>> GetIdEquals(object id)
    {
        var equal = Expression.Equal(_idGetterExpr.Body, Expression.Constant(id, typeof(object)));
        var lamda = Expression.Lambda<Func<T, bool>>(equal, _idGetterExpr.Parameters);
        return lamda;
    }

    private Expression<Func<T, object>> GetKeyGetterAndSetter(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        // Only supports non composite keys
        using var context = dbFactory.CreateDbContext();
        var key = context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
        var property = key?.Properties[0];
        if (property is null)
        {
            throw new InvalidOperationException("No primary key found");
        }

        var propertyInfo = property.PropertyInfo;
        return propertyInfo.GetConcretePropertyExpression<T, object>();
    }

    private Func<ApplicationDbContext, DbSet<T>> GetDbSetGetter()
    {
        var dbsetMethod = typeof(ApplicationDbContext).GetMethods().Single(
            m =>
            {
                return m.IsPublic
                       && m.Name == "Set"
                       && m.IsGenericMethod
                       && m.GetParameters().Length == 1;
            });
        return dbsetMethod.MakeGenericMethod(typeof(T)).CreateDelegate<Func<ApplicationDbContext, DbSet<T>>>();
    }
}