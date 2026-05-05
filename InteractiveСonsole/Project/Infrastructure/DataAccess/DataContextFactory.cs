namespace InteractiveСonsole.Project.Infrastructure.DataAccess;

/// <summary>
/// Фабрика для создания экземпляров <see cref="ToDoDataContext"/>.
/// Обеспечивает создание новых контекстов данных с использованием строки подключения.
/// </summary>
public class DataContextFactory : IDataContextFactory<ToDoDataContext>
{
    private readonly string _connectionString;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="DataContextFactory"/>.
    /// </summary>
    /// <param name="connectionString">Строка подключения к базе данных PostgreSQL</param>
    public DataContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc cref="IDataContextFactory{TDataContext}.CreateDataContext"/>
    public ToDoDataContext CreateDataContext()
    {
        return new ToDoDataContext(_connectionString);
    }
}
