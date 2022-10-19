using System.Linq.Expressions;
using AntDesign.TableModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Utils;

namespace PrackyASusarny.Data.EFCoreServices;

public class CrudService<T> : ICrudService<T> where T : class
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IEntityType _entityType;
    private readonly Func<T, object> _idGetter;
    private readonly Expression<Func<T, object>> _idGetterExpr;
    private readonly ILogger<CrudService<T>> _logger;

    public CrudService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<CrudService<T>> logger,
        Expression<Func<T, object>> idGetter)
    {
        _dbFactory = dbFactory;
        _idGetterExpr = idGetter;
        _idGetter = idGetter.Compile();
        _entityType = dbFactory.CreateDbContext().Model.FindEntityType(typeof(T)) ??
                      throw new InvalidOperationException("Entity type not found");
        _logger = logger;
    }

    public CrudService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<CrudService<T>> logger) : this(
        dbFactory, logger, GetKeyGetter(dbFactory))
    {
    }


    public async Task<List<T>> GetAllAsync(QueryModel<T>? queryModel = null, bool eager = false)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var query = dbContext.Set<T>().AsQueryable();
        query = GetBoilerplate(query, queryModel, eager);

        return await query.ToListAsync();
    }

    public async Task<List<TResult>> GetAllAsync<TResult, TKey>(Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>[]? filters = null, SortOption<T, TKey>[]? sortKeys = null, bool eager = false)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var query = dbContext.Set<T>().AsQueryable();
        query = GetBoilerplate(query, filters, sortKeys, eager);

        return await query.Select(selector).ToListAsync();
    }

    public async Task<T?> GetByIdAsync(object id, bool eager = false)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var query = dbContext.Set<T>().AsQueryable();
        query = GetBoilerplate(query, eager: eager);
        var result = query.Where(GetIdEquals(id)).SingleOrDefaultAsync();
        return await result;
    }

    public async Task<TResult?> GetByIdAsync<TResult>(object id, Expression<Func<T, TResult>> selector,
        bool eager = false)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var query = dbContext.Set<T>().AsQueryable();
        query = GetBoilerplate(query, eager: eager);
        var result = query.Where(GetIdEquals(id)).Select(selector).SingleOrDefaultAsync();
        return await result;
    }

    public async Task CreateAsync(T entity)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var dbset = dbContext.Set<T>();
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
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var dbset = dbContext.Set<T>();
        dbset.Attach(entity);
        dbContext.Entry(entity).State = EntityState.Modified;
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "Error updating entity");
            throw new Errors.Folder.DbUpdateException(e.Message);
        }
    }

    public async Task DeleteAsync(T entity)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var dbSet = dbContext.Set<T>();
        dbSet.Attach(entity);
        dbSet.Remove(entity);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "Error while deleting entity");
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

    private static Expression<Func<T, object>> GetKeyGetter(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        // Only supports non composite keys
        using var context = dbFactory.CreateDbContext();
        var key = context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
        var property = key?.Properties[0];
        if (property is null) throw new InvalidOperationException("No primary key found");

        var propertyInfo = property.PropertyInfo;
        if (propertyInfo is null) throw new InvalidOperationException("No property info found");
        return propertyInfo.GetConcretePropertyExpression<T, object>();
    }

    private IQueryable<T> GetBoilerplate<TKey>(IQueryable<T> query, Expression<Func<T, bool>>[]? filters = null,
        SortOption<T, TKey>[]? sortKeys = null, bool eager = false)
    {
        query = GetBoilerplate(query, filters, eager);
        if (sortKeys != null) query.SortWithKeys(sortKeys);

        return query;
    }

    private IQueryable<T> GetBoilerplate(IQueryable<T> query, QueryModel<T>? queryModel = null,
        bool eager = false)
    {
        if (eager) query = query.MakeEager(_entityType);

        if (filters != null) query.FilterWithExpressions(filters);

        return query;
    }
}