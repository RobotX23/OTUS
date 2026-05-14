
-- 1. Таблица пользователей (ToDoUsers)
CREATE TABLE IF NOT EXISTS "ToDoUsers" (
    "UserId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "TelegramUserId" BIGINT NOT NULL,
    "TelegramUserName" TEXT,
    "RegistereAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. Таблица списков задач (ToDoLists)
CREATE TABLE IF NOT EXISTS "ToDoLists" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "Name" TEXT NOT NULL,
    "CreateAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT "FK_ToDoLists_ToDoUsers" 
        FOREIGN KEY ("UserId") REFERENCES "ToDoUsers"("UserId") ON DELETE CASCADE
);

-- 3. Таблица задач (ToDoItems)
CREATE TABLE IF NOT EXISTS "ToDoItems" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "ListId" UUID, -- NULL означает задачу "Без списка"
    "Name" TEXT NOT NULL,
    "CreateAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "State" INT NOT NULL DEFAULT 0, -- 0 = Active, 1 = Completed
    "StateChangeAt" TIMESTAMPTZ,
    "Deadline" TIMESTAMPTZ NOT NULL,

    CONSTRAINT "FK_ToDoItems_ToDoUsers" 
        FOREIGN KEY ("UserId") REFERENCES "ToDoUsers"("UserId") ON DELETE CASCADE,
    
    CONSTRAINT "FK_ToDoItems_ToDoLists" 
        FOREIGN KEY ("ListId") REFERENCES "ToDoLists"("Id") ON DELETE SET NULL,
    
    CONSTRAINT "CK_ToDoItems_State" 
        CHECK ("State" IN (0, 1))
);


-- 4. Таблица уведомлений (Notifications)
CREATE TABLE IF NOT EXISTS "Notifications" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "Type" TEXT NOT NULL,
    "Text" TEXT NOT NULL,
    "ScheduledAt" TIMESTAMPTZ NOT NULL,
    "IsNotified" BOOLEAN NOT NULL DEFAULT FALSE,
    "NotifiedAt" TIMESTAMPTZ,

    CONSTRAINT "FK_Notifications_ToDoUsers" 
        FOREIGN KEY ("UserId") REFERENCES "ToDoUsers"("UserId") ON DELETE CASCADE
);


-- Уникальный индекс для TelegramUserId (гарантирует 1 аккаунт Telegram = 1 пользователь)
CREATE UNIQUE INDEX IF NOT EXISTS "UX_ToDoUsers_TelegramUserId" ON "ToDoUsers"("TelegramUserId");

-- Индексы для внешних ключей (ускоряют JOIN и фильтрацию по связям)
CREATE INDEX IF NOT EXISTS "IX_ToDoLists_UserId" ON "ToDoLists"("UserId");
CREATE INDEX IF NOT EXISTS "IX_ToDoItems_UserId" ON "ToDoItems"("UserId");
CREATE INDEX IF NOT EXISTS "IX_ToDoItems_ListId" ON "ToDoItems"("ListId");
CREATE INDEX IF NOT EXISTS "IX_Notifications_UserId" ON "Notifications"("UserId");

-- Индекс для быстрой фильтрации по статусу (Active / Completed)
CREATE INDEX IF NOT EXISTS "IX_ToDoItems_State" ON "ToDoItems"("State");

CREATE INDEX IF NOT EXISTS "IX_Notifications_ScheduledAt_IsNotified" 
    ON "Notifications"("ScheduledAt", "IsNotified") 
    WHERE "IsNotified" = FALSE;