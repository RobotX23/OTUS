using InteractiveСonsole.Project.Core.DataAccess.Models;
using InteractiveСonsole.Project.Core.Entities;
using InteractiveСonsole.Project.Infrastructure.DataAccess.Models;

namespace InteractiveСonsole.Project.Infrastructure.DataAccess;

internal static class ModelMapper
{
    // ==================== USER ====================
    public static ToDoUser MapFromModel(ToDoUserModel model)
    {
        if (model == null) return null!;
        return new ToDoUser
        {
            UserId = model.UserId,
            TelegramUserId = model.TelegramUserId,
            TelegramUserName = model.TelegramUserName,
            RegistereAt = model.RegistereAt
        };
    }

    public static ToDoUserModel MapToModel(ToDoUser entity)
    {
        if (entity == null) return null!;
        return new ToDoUserModel
        {
            UserId = entity.UserId,
            TelegramUserId = entity.TelegramUserId,
            TelegramUserName = entity.TelegramUserName,
            RegistereAt = entity.RegistereAt
        };
    }

    // ==================== LIST ====================
    public static ToDoList MapFromModel(ToDoListModel model)
    {
        if (model == null) return null!;
        return new ToDoList
        {
            Id = model.Id,
            UserId = model.UserId,
            Name = model.Name,
            CreateAt = model.CreateAt,
            User = model.User != null ? MapFromModel(model.User) : null
        };
    }

    public static ToDoListModel MapToModel(ToDoList entity)
    {
        if (entity == null) return null!;
        return new ToDoListModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Name = entity.Name,
            CreateAt = entity.CreateAt,
            User = entity.User != null ? MapToModel(entity.User) : null
        };
    }

    // ==================== ITEM ====================
    public static ToDoItem MapFromModel(ToDoItemModel model)
    {
        if (model == null) return null!;
        return new ToDoItem
        {
            Id = model.Id,
            UserId = model.UserId,
            Name = model.Name,
            CreateAt = model.CreateAt,
            // ✅ Безопасное приведение двух разных enum через int
            State = (InteractiveСonsole.ToDoItemState)(int)model.State,
            StateChangeAt = model.StateChangeAt,
            Deadline = model.Deadline,
            ListId = model.ListId,
            User = model.User != null ? MapFromModel(model.User) : null,
            List = model.List != null ? MapFromModel(model.List) : null
        };
    }

    public static ToDoItemModel MapToModel(ToDoItem entity)
    {
        if (entity == null) return null!;
        return new ToDoItemModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Name = entity.Name,
            CreateAt = entity.CreateAt,
            State = (Project.Core.DataAccess.Models.ToDoItemState)(int)entity.State,
            StateChangeAt = entity.StateChangeAt,
            Deadline = entity.Deadline,
            ListId = entity.ListId
            // Навигационные свойства НЕ маппим: linq2db для INSERT/UPDATE использует только FK
        };
    }

    // ==================== NOTIFICATION ====================
    public static Notification MapFromModel(NotificationModel model)
    {
        if (model == null) return null!;
        return new Notification
        {
            Id = model.Id,
            UserId = model.UserId,
            User = model.User != null ? MapFromModel(model.User) : null,
            Type = model.Type,
            Text = model.Text,
            ScheduledAt = model.ScheduledAt,
            IsNotified = model.IsNotified,
            NotifiedAt = model.NotifiedAt
        };
    }

    public static NotificationModel MapToModel(Notification entity)
    {
        if (entity == null) return null!;
        return new NotificationModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Type = entity.Type,
            Text = entity.Text,
            ScheduledAt = entity.ScheduledAt,
            IsNotified = entity.IsNotified,
            NotifiedAt = entity.NotifiedAt
        };
    }
}