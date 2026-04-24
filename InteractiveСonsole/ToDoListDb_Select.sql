-- ѕолучить все задачи пользовател€
SELECT * FROM "ToDoItems"
WHERE "UserId" = '00000000-0000-0000-0000-000000000001';

-- ѕолучить только активные (не выполненные) задачи пользовател€
SELECT * FROM "ToDoItems"
WHERE "UserId" = '00000000-0000-0000-0000-000000000001' 
AND "State" = 0;

-- ѕолучить конкретную задачу по еЄ ID
SELECT * FROM "ToDoItems"
WHERE "Id" = '11111111-1111-1111-1111-111111111111';

-- ѕроверить, есть ли у пользовател€ задача с таким именем
SELECT EXISTS(
    SELECT * FROM "ToDoItems"
    WHERE "UserId" = '00000000-0000-0000-0000-000000000001' AND "Name" = 'Test Items'
);

-- ѕосчитать количество активных задач пользовател€
SELECT COUNT(*) FROM "ToDoItems"
WHERE "UserId" = '00000000-0000-0000-0000-000000000001' AND "State" = 0;

-- ѕоиск задачи по началу первой части задачи
SELECT * FROM "ToDoItems"
WHERE "UserId" = '00000000-0000-0000-0000-000000000001' AND "Name" LIKE 'Test%';