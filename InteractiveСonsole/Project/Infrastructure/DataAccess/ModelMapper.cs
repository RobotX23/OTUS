using InteractiveСonsole.Project.Core.DataAccess.Models;

namespace InteractiveСonsole.Project.Infrastructure.DataAccess;

/// <summary>
/// Статический класс для маппинга между доменными сущностями и моделями для работы с БД
/// </summary>
internal static class ModelMapper
{
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

    public static ToDoItem MapFromModel(ToDoItemModel model)
    {
        if (model == null) return null!;

        var item = new ToDoItem
        {
            Id = model.Id,
            UserId = model.UserId,
            Name = model.Name,
            CreateAt = model.CreateAt,
            State = (ToDoItemState)model.State,
            StateChangeAt = model.StateChangeAt,
            Deadline = model.Deadline,
            ListId = model.ListId
        };

        if (model.User != null)
        {
            item.User = MapFromModel(model.User);
        }

        if (model.List != null)
        {
            item.List = MapFromModel(model.List);
        }

        return item;
    }

    public static ToDoItemModel MapToModel(ToDoItem entity)
    {
        if (entity == null) return null!;

        var model = new ToDoItemModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Name = entity.Name,
            CreateAt = entity.CreateAt,
            State = (Core.DataAccess.Models.ToDoItemState)entity.State,
            StateChangeAt = entity.StateChangeAt,
            Deadline = entity.Deadline,
            ListId = entity.ListId
        };

        if (entity.User != null)
        {
            model.User = MapToModel(entity.User);
        }

        if (entity.List != null)
        {
            model.List = MapToModel(entity.List);
        }

        return model;
    }

    public static ToDoList MapFromModel(ToDoListModel model)
    {
        if (model == null) return null!;

        var list = new ToDoList
        {
            Id = model.Id,
            UserId = model.UserId,
            Name = model.Name,
            CreateAt = model.CreateAt
        };

        if (model.User != null)
        {
            list.User = MapFromModel(model.User);
        }

        return list;
    }

    public static ToDoListModel MapToModel(ToDoList entity)
    {
        if (entity == null) return null!;

        var model = new ToDoListModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Name = entity.Name,
            CreateAt = entity.CreateAt
        };

        if (entity.User != null)
        {
            model.User = MapToModel(entity.User);
        }

        return model;
    }
}
