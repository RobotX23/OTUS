
-- 1. Пользователи (ToDoUsers)
INSERT INTO "ToDoUsers" ("UserId", "TelegramUserId", "TelegramUserName", "RegistereAt") VALUES
('00000000-0000-0000-0000-000000000001', 111111111, 'Robot', NOW()),
('00000000-0000-0000-0000-000000000002', 222222222, 'Robot1', NOW());

-- 2. Списки задач (ToDoLists)
INSERT INTO "ToDoLists" ("Id", "UserId", "Name", "CreateAt") VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '00000000-0000-0000-0000-000000000001', 'Test1', NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '00000000-0000-0000-0000-000000000001', 'Test2', NOW()),
('cccccccc-cccc-cccc-cccc-cccccccccccc', '00000000-0000-0000-0000-000000000002', 'Test3', NOW());

-- 3. Задачи (ToDoItems)
INSERT INTO "ToDoItems" ("Id", "UserId", "ListId", "Name", "CreateAt", "State", "StateChangeAt", "Deadline") VALUES
('11111111-1111-1111-1111-111111111111', '00000000-0000-0000-0000-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Test Item1', NOW(), 0, NULL, NOW() + INTERVAL '1 day'),
('22222222-2222-2222-2222-222222222222', '00000000-0000-0000-0000-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Test Item2', NOW(), 1, NOW(), NOW()),
('33333333-3333-3333-3333-333333333333', '00000000-0000-0000-0000-000000000001', NULL, 'Test Item3', NOW(), 0, NULL, NOW() + INTERVAL '2 hours'),
('44444444-4444-4444-4444-444444444444', '00000000-0000-0000-0000-000000000001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Test Imem4', NOW(), 0, NULL, NOW() + INTERVAL '5 hours'),
('55555555-5555-5555-5555-555555555555', '00000000-0000-0000-0000-000000000002', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Test Item5', NOW(), 0, NULL, NOW() + INTERVAL '3 days');