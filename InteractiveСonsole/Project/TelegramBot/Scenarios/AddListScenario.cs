using InteractiveСonsole.Project.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InteractiveСonsole.Project.TelegramBot.Scenarios
{
    internal class AddListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;

        public AddListScenario(IUserService userService, IToDoListService toDoListService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _toDoListService = toDoListService ?? throw new ArgumentNullException(nameof(toDoListService));
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddList;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct, CallbackQuery callbackQuery)
        {
            switch (context.CurretStep)
            {
                case null:
                    var user = await _userService.GetUser(message.From!.Id, ct);
                    context.Data["user"] = user;
                    context.CurretStep = "Name";
                    return ScenarioResult.Transition;

                case "Name":
                    if (message.Text == "Назад")
                        return ScenarioResult.Completed;
                    ValidateName(message.Text);
                    var dict = (Dictionary<string, object>)context.Data!;
                    if (!dict.TryGetValue("user", out var userObj) || userObj is not ToDoUser toDoUser)
                        throw new InvalidOperationException("Пользовотель отсутствует");
                    await _toDoListService.Add(toDoUser, message.Text!.Trim(), ct);
                    await botClient.SendMessage(message.Chat.Id, "Список создан.", cancellationToken: ct);
                    return ScenarioResult.Completed;

                default:
                    return ScenarioResult.Completed;
            }
        }

        void ValidateName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название списка не может быть пустым.");
        }
    }
}
