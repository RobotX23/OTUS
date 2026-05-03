using LinqToDB;
using LinqToDB.Async;

namespace InteractiveСonsole.Project.Infrastructure.DataAccess;

/// <summary>
/// Репозиторий задач для работы с базой данных PostgreSQL через linq2db
/// </summary>
internal class SqlToDoRepository : IToDoRepository
{
    private readonly IDataContextFactory<ToDoDataContext> _factory;

    public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        var items = await dbContext.ToDoItems
            .Where(x => x.UserId == userId)
            .LoadWith(x => x.User)
            .LoadWith(x => x.List)
            .LoadWith(x => x.List!.User)
            .ToListAsync(ct);

        return items.Select(ModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        var items = await dbContext.ToDoItems
            .Where(x => x.UserId == userId && x.State == 0)
            .LoadWith(x => x.User)
            .LoadWith(x => x.List)
            .LoadWith(x => x.List!.User)
            .ToListAsync(ct);

        return items.Select(ModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task<ToDoItem?> Get(Guid id, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        var item = await dbContext.ToDoItems
            .Where(x => x.Id == id)
            .LoadWith(x => x.User)
            .LoadWith(x => x.List)
            .LoadWith(x => x.List!.User)
            .FirstOrDefaultAsync(ct);

        return item != null ? ModelMapper.MapFromModel(item) : null;
    }

    public async Task Add(ToDoItem item, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        var model = ModelMapper.MapToModel(item);
        await dbContext.InsertAsync(model, token: ct);
    }

    public async Task Update(ToDoItem item, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        var model = ModelMapper.MapToModel(item);
        await dbContext.UpdateAsync(model, token: ct);
    }

    public async Task Delete(Guid id, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        await dbContext.ToDoItems
            .Where(x => x.Id == id)
            .DeleteAsync(ct);
    }

    public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        return await dbContext.ToDoItems
            .Where(x => x.UserId == userId && x.Name == name)
            .AnyAsync(ct);
    }

    public async Task<int> CountActive(Guid userId, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        return await dbContext.ToDoItems
            .Where(x => x.UserId == userId && x.State == 0)
            .CountAsync(ct);
    }

    public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct = default)
    {
        var allItems = await GetAllByUserId(userId, ct);
        return allItems.Where(predicate).ToList().AsReadOnly();
    }
}