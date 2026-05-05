using System;
using System.Collections.Generic;
using System.Linq;
using InteractiveСonsole.Project.Core.DataAccess;
using LinqToDB;
using LinqToDB.Async;

namespace InteractiveСonsole.Project.Infrastructure.DataAccess;

/// <summary>
/// Репозиторий списков задач для работы с базой данных PostgreSQL через linq2db
/// </summary>
internal class SqlToDoListRepository : IToDoListRepository
{
    private readonly IDataContextFactory<ToDoDataContext> _factory;

    public SqlToDoListRepository(IDataContextFactory<ToDoDataContext> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task<ToDoList?> Get(Guid id, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        var model = await dbContext.ToDoLists
            .Where(x => x.Id == id)
            .LoadWith(x => x.User)
            .FirstOrDefaultAsync(ct);

        return model != null ? ModelMapper.MapFromModel(model) : null;
    }

    public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        var models = await dbContext.ToDoLists
            .Where(x => x.UserId == userId)
            .LoadWith(x => x.User)
            .ToListAsync(ct);

        return models.Select(ModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task Add(ToDoList list, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        var model = ModelMapper.MapToModel(list);

        await dbContext.InsertAsync(model, token: ct);
    }

    public async Task Delete(Guid id, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();

        await dbContext.ToDoLists
            .Where(x => x.Id == id)
            .DeleteAsync(ct);
    }

    public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        return await dbContext.ToDoLists
            .Where(x => x.UserId == userId && x.Name == name)
            .AnyAsync(ct);
    }
}