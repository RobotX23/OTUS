# QWEN.md — OTUS InteractiveConsole

## Project Overview

**InteractiveConsole** — учебный проект для OTUS: Telegram-бот для управления списком задач (ToDo). Позволяет регистрироваться, создавать задачи, управлять ими (добавлять, удалять, завершать), группировать по спискам и получать статистику.

### Tech Stack

- **Language:** C#
- **Framework:** .NET 8.0
- **Library:** Telegram.Bot v22.9.0
- **Storage:** File-based JSON repositories
- **IDE:** Visual Studio 2022 (v17.12)

### Architecture

Трёхуровневая архитектура с разделением на слои:

```
InteractiveConsole/
├── Program.cs                          # Точка входа, инициализация бота и зависимостей
└── Project/
    ├── Core/                           # Доменная логика и интерфейсы
    │   ├── DataAccess/                 # Интерфейсы репозиториев
    │   │   ├── IToDoRepository.cs
    │   │   ├── IToDoListRepository.cs
    │   │   └── IUserRepository.cs
    │   ├── Entities/                   # Доменные модели
    │   │   ├── ToDoItem.cs             # Задача (состояние, дедлайн, список)
    │   │   ├── ToDoList.cs             # Список задач
    │   │   └── ToDoUser.cs             # Пользователь
    │   ├── Exceptions/                 # Кастомные исключения
    │   │   ├── TaskCountLimitException.cs
    │   │   ├── TaskLengthLimitException.cs
    │   │   └── DublicateTaskException.cs
    │   └── Services/                   # Сервисы бизнес-логики
    │       ├── ToDoService.cs          # IToDoService + SetLimits()
    │       ├── ToDoListService.cs
    │       ├── UserService.cs
    │       └── ToDoReportService.cs
    ├── Infrastructure/                 # Реализация хранения данных
    │   └── DataAccess/
    │       ├── FileUserRepository.cs
    │       ├── FileToDoRepository.cs
    │       └── FileToDoListRepository.cs
    └── TelegramBot/                    # Слой взаимодействия с Telegram
        ├── UpdateHandler.cs            # Обработка сообщений и callback-запросов
        ├── Dto/                        # DTO для callback-данных
        │   ├── CallbackDto.cs
        │   └── ToDoListCallbackDto.cs
        └── Scenarios/                  # Пошаговые сценарии диалога
            ├── IScenario.cs
            ├── IScenarioContextRepository.cs
            ├── InMemoryScenarioContextRepository.cs
            ├── ScenarioContext.cs      # Передача состояния между шагами
            ├── ScenarioResult.cs       # Completed / Transition
            ├── ScenarioType.cs
            ├── AddTaskScenario.cs
            ├── AddListScenario.cs
            └── DeleteListScenario.cs
```

## Building and Running

### Prerequisites

- .NET 8.0 SDK
- Telegram Bot Token (получить у @BotFather)

### Setup

1. Установите переменную окружения `TELEGRAM_BOT_TOKEN_EX1`:
   ```cmd
   setx TELEGRAM_BOT_TOKEN_EX1 "YOUR_BOT_TOKEN"
   ```

2. Сборка:
   ```cmd
   dotnet build InteractiveСonsole.sln
   ```

3. Запуск:
   ```cmd
   dotnet run --project InteractiveСonsole
   ```

4. Остановка: нажмите **A** в консоли.

## Available Bot Commands

| Команда | Описание |
|---------|----------|
| `/start` | Авторизация / регистрация |
| `/help` | Помощь |
| `/info` | Информация о релизе |
| `/exit` | Выход из сессии |
| `/addtask` | Добавить задачу (через сценарий) |
| `/show` | Показать задачи (inline-клавиатура по спискам) |
| `/remowetask <num>` | Удалить задачу по номеру |
| `/completetask <guid>` | Завершить задачу по ID |
| `/find <text>` | Поиск задачи по слову |
| `/report` | Отчёт статистики |

## Development Conventions

### Состояние и многопоточность

`UpdateHandler` — **singleton**, обрабатывающий обновления параллельно. Ключевое правило:

> **Данные пользователя не должны храниться в полях класса.** Используйте `ScenarioContext` для передачи состояния между шагами сценария.

**Известная проблема:** поля `name`, `user2`, `_update`, `flag` в `UpdateHandler` — изменяемые поля состояния, которые создают race condition при параллельной обработке. Данные нужно перенести в `ScenarioContext`.

### Лимиты задач

Параметры `maxtasks` и `maxline` — **деталь реализации**, скрыты от публичного интерфейса:

- `IToDoService` объявляет метод `void SetLimits(int maxTasks, int maxLine)`
- Свойства `maxtasks`/`maxline` — `private` в `ToDoService`
- Устанавливаются через `SetLimits()` при первом запуске пользователем

### Обработка ошибок

При `TaskCountLimitException`, `TaskLengthLimitException`, `DublicateTaskException`:

1. Исключение логируется в консоль через `HandleErrorAsync`
2. Пользователю отправляется: `Ошибка ввода: {ex.Message}`
3. Сценарий завершается, контекст сбрасывается, клавиатура возвращается в основное меню

Это применяется в обоих местах: `ProcessScenario` (сценарии) и `OnMessage` (прямые команды).

### Хранение данных

- JSON-файлы, один файл на сущность
- `index.json` хранит маппинг `ItemId -> UserId` для быстрого поиска в `FileToDoRepository`
- Папки: `ToDoItem/`, `ToDoUser/`, `ToDoList/`

### Сценарии

Интерфейс `IScenario` с методами:
- `CanHandle(ScenarioType)` — определяет, может ли сценарий обработать тип
- `HandleMessageAsync(...)` — обработка шага, возврат `ScenarioResult.Transition` или `ScenarioResult.Completed`

Состояние хранится в `ScenarioContext`: `CurretStep` (шаг) и `Data` (словарь ключ-значение).

## Notes

- Проект является учебным (домашнее задание OTUS)
- В коде присутствуют опечатки в названиях: `DublicateTaskException`, `remowetask`, `cansel`, `CurretStep` — могут быть намеренными
- Максимальное количество задач и длина задаются пользователем при первом взаимодействии (лимиты 1–100)
