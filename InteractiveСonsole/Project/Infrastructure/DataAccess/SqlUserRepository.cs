using LinqToDB;
using LinqToDB.Async;

namespace InteractiveСonsole.Project.Infrastructure.DataAccess;

/// <summary>
/// Репозиторий пользователей для работы с базой данных PostgreSQL через linq2db
/// </summary>
internal class SqlUserRepository : IUserRepository
{
    private readonly IDataContextFactory<ToDoDataContext> _factory;

    public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        var model = await dbContext.ToDoUsers
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        return model != null ? ModelMapper.MapFromModel(model) : null;
    }

    public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        var model = await dbContext.ToDoUsers
            .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId, ct);

        return model != null ? ModelMapper.MapFromModel(model) : null;
    }

    public async Task Add(ToDoUser user, CancellationToken ct = default)
    {
        using var dbContext = _factory.CreateDataContext();
        var model = ModelMapper.MapToModel(user);

        await dbContext.InsertAsync(model, token: ct);
    }
}