using System.Linq.Expressions;
using AntDesign.TableModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PrackyASusarny.Data.ServiceInterfaces;
using PrackyASusarny.Middlewares;
using PrackyASusarny.Utils;

namespace PrackyASusarny.Data.EFCoreServices;

public class CrudService<T> : ICrudService<T> where T : class
{
    private readonly IContextHookMiddleware _contextHookMiddleware;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IEntityType _entityType;
    private readonly Func<T, object> _idGetter;
    private readonly Expression<Func<T, object>> _idGetterExpr;

    public CrudService(IDbContextFactory<ApplicationDbContext> dbFactory,
        IContextHookMiddleware contextHookMiddleware,
        Expression<Func<T, object>> idGetter)
    {
        _dbFactory = dbFactory;
        _idGetterExpr = idGetter;
        _idGetter = idGetter.Compile();
        _contextHookMiddleware = contextHookMiddleware;
        _entityType =
            dbFactory.CreateDbContext().Model.FindEntityType(typeof(T)) ??
            throw new InvalidOperationException("Entity type not found");
    }

    public CrudService(IDbContextFactory<ApplicationDbContext> dbFactory,
        IContextHookMiddleware middleware) : this(
        dbFactory, middleware, GetKeyGetter(dbFactory))
    {
    }


    public async Task<T[]> GetAllAsync(QueryModel<T>? queryModel,
        bool eager = false)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var query = dbContext.Set<T>().AsQueryable();
        query = GetBoilerplate(query, queryModel, eager);
        if (queryModel != null) query = queryModel.CurrentPagedRecords(query);

        return await query.ToArrayAsync();
    }

    public async Task<int> GetCountAsync(QueryModel<T>? queryModel)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var query = dbContext.Set<T>().AsQueryable();
        query = GetBoilerplate(query, queryModel);
        return await query.CountAsync();
    }

    public async Task<T?> GetByIdAsync(object id, bool eager = false)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var query = dbContext.Set<T>().AsQueryable();
        query = GetBoilerplate(query, eager: eager);
        var result = query.Where(GetIdEquals(id)).SingleOrDefaultAsync();
        return await result;
    }

    public async Task<T> CreateAsync(T entity)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var dbset = dbContext.Set<T>();
        dbset.Attach(entity);
        dbContext.Entry(entity).State = EntityState.Added;
        await dbContext.SaveChangeAsyncRethrow();
        // Fire and forget
        _contextHookMiddleware.OnSave(EntityState.Added, entity)
            .FireAndForget();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var dbset = dbContext.Set<T>();
        dbset.Attach(entity);
        dbContext.Entry(entity).State = EntityState.Modified;
        await dbContext.SaveChangeAsyncRethrow();
        // Fire and forget
        _contextHookMiddleware.OnSave(EntityState.Modified, entity)
            .FireAndForget();
        return entity;
    }

    public async Task DeleteAsync(T entity)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var dbSet = dbContext.Set<T>();
        dbSet.Attach(entity);
        dbSet.Remove(entity);
        await dbContext.SaveChangeAsyncRethrow();
        // Fire and forget
        _contextHookMiddleware.OnSave(EntityState.Deleted, entity)
            .FireAndForget();
    }

    public object GetId(T entity)
    {
        return _idGetter(entity);
    }

    public async Task<TResult?> GetByIdAsync<TResult>(object id,
        Expression<Func<T, TResult>> selector,
        bool eager = false)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var query = dbContext.Set<T>().AsQueryable();
        query = GetBoilerplate(query, eager: eager);
        var result = query.Where(GetIdEquals(id)).Select(selector)
            .SingleOrDefaultAsync();
        return await result;
    }

    public Expression<Func<T, bool>> GetIdEquals(object id)
    {
        var equal = Expression.Equal(_idGetterExpr.Body,
            Expression.Constant(id, typeof(object)));
        var lamda =
            Expression.Lambda<Func<T, bool>>(equal, _idGetterExpr.Parameters);
        return lamda;
    }

    private static Expression<Func<T, object>> GetKeyGetter(
        IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        // Only supports non composite keys
        using var context = dbFactory.CreateDbContext();
        var key = context.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
        var property = key?.Properties[0];
        if (property is null)
            throw new InvalidOperationException("No primary key found");

        var propertyInfo = property.PropertyInfo;
        if (propertyInfo is null)
            throw new InvalidOperationException("No property info found");
        return propertyInfo.GetConcretePropertyExpression<T, object>();
    }

    private IQueryable<T> GetBoilerplate(IQueryable<T> query,
        QueryModel<T>? queryModel = null,
        bool eager = false)
    {
        if (eager) query = query.MakeEager(_entityType);

        if (queryModel is not null)
            // I Would od this manually but no way to call SortList and I don't have access to internal functions
            query = query.ExecuteTableQuery(queryModel);

        return query;
    }
}