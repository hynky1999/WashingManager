using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PrackyASusarny.Data.EFCoreServices;

public class CrudService<T> : ICrudService<T> where T : class, new()
{
    protected readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly Func<ApplicationDbContext, DbSet<T>> _dbSetGetter;
    private readonly Delegate _idSetter;

    private readonly ILogger _logger;

    public CrudService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger logger,
        Func<ApplicationDbContext, DbSet<T>> dbSetGetter, Delegate idSetter)
    {
        _dbFactory = dbFactory;
        _dbSetGetter = dbSetGetter;
        _idSetter = idSetter;
        _logger = logger;
    }

    public async Task<List<T>> GetAllAsync()
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var query = _dbSetGetter(dbContext);
        return await query.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var query = _dbSetGetter(dbContext);
        return await query.FindAsync(id);
    }

    public async void CreateAsync(T entity)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        _dbSetGetter(dbContext).Add(entity);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            throw new Errors.Folder.DbUpdateException(e.Message);
        }
    }

    public async void UpdateAsync(T entity)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var query = _dbSetGetter(dbContext).Update(entity);
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

    public async void DeleteAsync(int id)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var dbSet = _dbSetGetter(dbContext);
        var defaultEntity = new T();
        _idSetter.DynamicInvoke(defaultEntity, id);
        dbSet.Attach(defaultEntity);
        dbSet.Remove(defaultEntity);
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

    {
        _dbFactory = dbFactory;
        _dbSetGetter = CreateDbSetGetter(propertyInfo);
        using (var db = _dbFactory.CreateDbContext())
        {
            var entityType = _dbSetGetter(db).EntityType;
            _idSetter = CreateIdSetter(entityType);
        }

        _logger = logger;
    }

    private Func<ApplicationDbContext, DbSet<T>> CreateDbSetGetter(PropertyInfo propertyInfo)
    {
        var parameter = Expression.Parameter(typeof(ApplicationDbContext), "context");
        var property = Expression.Property(parameter, propertyInfo);
        var lambda = Expression.Lambda<Func<ApplicationDbContext, DbSet<T>>>(property, parameter).Compile();
        return lambda;
    }

    private Delegate CreateIdSetter(IEntityType entityType)
    {
        var keys = entityType.FindPrimaryKey()?.Properties.Select(p => p.PropertyInfo).ToArray();
        // Only one key supported as of now
        if (keys is null || keys.Length != 1 || keys[0] is null)
        {
            throw new NotSupportedException("Only one key supported as of now");
        }

        var propertyInfo = keys[0]!;
        var parameterType = Expression.Parameter(typeof(T));
        var parameterValue = Expression.Parameter(propertyInfo.PropertyType);
        var body = Expression.Assign(Expression.Property(parameterType, propertyInfo), parameterValue);

        var lambda = Expression.Lambda(typeof(Action<>).MakeGenericType(typeof(T), propertyInfo.PropertyType), body,
            new ParameterExpression[] {parameterType, parameterValue}).Compile();
        return lambda;
    }
}