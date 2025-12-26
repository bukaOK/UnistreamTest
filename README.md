# Задание Т3
## Порядок запуска
1. В директории с docker-compose.yml запустить docker-compose up -d
2. Запустить в папке с проектом `UnistreamTest.csproj` команду `dotnet ef database update`
3. Запустить в той же папке `dotnet run --urls http://localhost:5833`, либо на любом другом порту

### Post Scriptum
Поскольку был сделан акцент на нескольких инстансах, в качестве хранилища счетчика использовал redis, бонусом добавил pgbouncer для единого пула соединений
