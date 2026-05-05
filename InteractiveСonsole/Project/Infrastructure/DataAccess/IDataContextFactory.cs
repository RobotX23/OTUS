using LinqToDB;

namespace InteractiveСonsole.Project.Infrastructure.DataAccess;

/// <summary>
/// Фабрика для создания экземпляров <see cref="DataConnection"/> на каждую сессию.
/// Позволяет создавать новые контексты данных по требованию, обеспечивая правильное управление жизненным циклом соединений.
/// </summary>
/// <typeparam name="TDataContext">Тип контекста данных, наследующий <see cref="DataConnection"/></typeparam>
public interface IDataContextFactory<TDataContext> where TDataContext : LinqToDB.Data.DataConnection
{
    /// <summary>
    /// Создаёт новый экземпляр контекста данных.
    /// </summary>
    /// <returns>Новый экземпляр <typeparamref name="TDataContext"/></returns>
    TDataContext CreateDataContext();
}
