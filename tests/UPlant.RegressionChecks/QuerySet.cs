using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using UPlant.Models.DB;

sealed class QuerySet<T>(CheckContext owner) : DbSet<T>, IQueryable<T>, IAsyncEnumerable<T> where T : class
{
    public List<T> Rows { get; } = [];
    public override Microsoft.EntityFrameworkCore.Metadata.IEntityType EntityType =>
        owner.Model.FindEntityType(typeof(T));
    IQueryable<T> Query => new AsyncRows<T>(Rows);
    Type IQueryable.ElementType => typeof(T);
    Expression IQueryable.Expression => Query.Expression;
    IQueryProvider IQueryable.Provider => Query.Provider;
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => Rows.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Rows.GetEnumerator();
    public override IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken token = default) =>
        new AsyncIterator<T>(Rows.GetEnumerator());
    public override EntityEntry<T> Add(T entity) => owner.Add(entity);
    public override T Find(params object[] keys)
    {
        var property = typeof(T).GetProperty("id") ?? typeof(T).GetProperty("Id");
        return Rows.FirstOrDefault(x => Equals(property.GetValue(x), keys[0]));
    }
}

sealed class AsyncRows<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public AsyncRows(IEnumerable<T> rows) : base(rows) { }
    public AsyncRows(Expression expression) : base(expression) { }
    IQueryProvider IQueryable.Provider => new AsyncProvider(this);
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken token = default) =>
        new AsyncIterator<T>(this.AsEnumerable().GetEnumerator());
}

sealed class AsyncIterator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(inner.MoveNext());
    public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
}

sealed class AsyncProvider(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression) =>
        (IQueryable)Activator.CreateInstance(typeof(AsyncRows<>).MakeGenericType(expression.Type.GetGenericArguments()[0]), expression);
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new AsyncRows<TElement>(expression);
    public object Execute(Expression expression) => inner.Execute(expression);
    public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);
    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken token = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var result = typeof(IQueryProvider).GetMethod(nameof(Execute), 1, [typeof(Expression)])
            .MakeGenericMethod(resultType).Invoke(inner, [expression]);
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            .MakeGenericMethod(resultType).Invoke(null, [result]);
    }
}

sealed class CheckContext : Entities
{
    public int SaveCalls { get; private set; }
    public bool FailSave { get; set; }
    public Type[] SavedTypes { get; private set; } = [];

    // Query sets are in memory; the relational provider is used only for graph metadata.
    public CheckContext() : base(new DbContextOptionsBuilder<Entities>()
        .UseSqlServer("Server=unused;Database=unused;Integrated Security=true").Options)
    {
        foreach (var property in typeof(Entities).GetProperties()
            .Where(x => x.CanWrite && x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)))
        {
            property.SetValue(this, Activator.CreateInstance(typeof(QuerySet<>)
                .MakeGenericType(property.PropertyType.GetGenericArguments()[0]), this));
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken token = default)
    {
        SaveCalls++;
        SavedTypes = ChangeTracker.Entries().Where(x => x.State == EntityState.Added)
            .Select(x => x.Entity.GetType()).ToArray();
        if (FailSave) throw new DbUpdateException("Simulated save failure");
        return Task.FromResult(1);
    }
}
