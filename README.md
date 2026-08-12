# AutoParts Hub

Выпускной проект по курсу OTUS C# Basic: онлайн-платформа для продажи новых и
бывших в употреблении автозапчастей. Проект создан как отдельное приложение на
основе практик из ветки **HomeWork17** репозитория домашних работ. Вместо
linq2db и ручных DDL-скриптов используется **EF Core Code First** с PostgreSQL
и версионируемыми миграциями.

## Что реализовано

MVP соответствует четырём базовым функциям из плана выпускного проекта:

1. Публичный каталог с постраничным выводом, поиском по названию, описанию и
   артикулу, фильтрами по категории, цене, состоянию и параметрам автомобиля.
2. Регистрация, вход по JWT, личный кабинет и гараж пользователя с VIN.
3. Корзина и оформление заказа с фиксацией цены, резервированием остатка,
   выбором доставки и оплаты, историей и статусами.
4. Административное API для категорий, товаров, совместимости, заказов и ролей.

Дополнительно реализованы:

- подбор товаров по VIN из локального справочника автомобилей;
- подписки на появление товара и снижение цены;
- фоновая задача обработки уведомлений;
- хранение уведомлений в личном кабинете;
- опциональная отправка уведомлений в Telegram;
- команды Telegram-бота **/find** и **/status**;
- Swagger/OpenAPI, Docker Compose, CI и модульные тесты доменных правил.

## Архитектура

~~~mermaid
flowchart TD
    Api["AutoPartsHub.Api<br/>HTTP, JWT, Swagger"] --> Application["AutoPartsHub.Application<br/>сценарии и контракты"]
    Infrastructure["AutoPartsHub.Infrastructure<br/>EF Core, PostgreSQL, Telegram"] --> Application
    Application --> Domain["AutoPartsHub.Domain<br/>сущности и бизнес-правила"]
    Infrastructure --> PostgreSQL[(PostgreSQL)]
~~~

Зависимости направлены внутрь:

- **Domain** не зависит от других проектов;
- **Application** зависит только от **Domain**;
- **Infrastructure** реализует интерфейсы **Application**;
- **Api** собирает приложение и отвечает за HTTP, JWT и обработку ошибок.

## Модель данных

EF Core создаёт таблицы Users, Vehicles, Categories, Products,
ProductCompatibilities, Carts, CartItems, Orders, OrderItems,
ProductSubscriptions и Notifications.

Первая миграция находится в каталоге
**src/AutoPartsHub.Infrastructure/Persistence/Migrations**.
Снимок цены и наименования хранится в OrderItems, поэтому история заказа не
меняется после редактирования каталога. Артикул, email, VIN, номер заказа,
категорийный slug и Telegram chat id защищены уникальными индексами.

## Быстрый запуск через Docker

Требуется Docker Desktop:

~~~bash
docker compose up --build
~~~

После старта:

- Swagger: <http://localhost:8080/swagger>
- API: <http://localhost:8080>
- PostgreSQL: localhost:5432

Для локальной демонстрации Docker создаёт администратора:

- email: **admin@autopartshub.local**
- пароль: **Admin123!**

Это только тестовые данные. Для любого внешнего окружения задайте собственные
секреты и не храните их в репозитории.

Остановить приложение:

~~~bash
docker compose down
~~~

Удалить также локальный том с тестовой БД:

~~~bash
docker compose down --volumes
~~~

## Запуск из Visual Studio или CLI

Требуются .NET SDK 9 и PostgreSQL 16+.

1. Создайте пустую базу **AutoPartsHub**.
2. Задайте секреты через User Secrets или переменные окружения:

~~~powershell
$env:ConnectionStrings__PostgreSQL = "Host=localhost;Port=5432;Database=AutoPartsHub;Username=postgres;Password=YOUR_PASSWORD"
$env:Jwt__SigningKey = "YOUR_RANDOM_SIGNING_KEY_WITH_AT_LEAST_32_BYTES"
$env:Seed__AdminEmail = "admin@example.com"
$env:Seed__AdminPassword = "YOUR_STRONG_PASSWORD"
dotnet run --project ./src/AutoPartsHub.Api
~~~

При старте API вызывает Database.MigrateAsync() и применяет только ещё не
выполненные миграции.

### Управление миграциями Code First

Установить инструмент один раз:

~~~bash
dotnet tool install --global dotnet-ef --version 9.*
~~~

Создать следующую миграцию:

~~~bash
dotnet ef migrations add MigrationName --project src/AutoPartsHub.Infrastructure --startup-project src/AutoPartsHub.Api --output-dir Persistence/Migrations
~~~

Применить миграции вручную:

~~~bash
dotnet ef database update --project src/AutoPartsHub.Infrastructure --startup-project src/AutoPartsHub.Api
~~~

## Основные маршруты API

| Доступ | Метод и маршрут | Назначение |
|---|---|---|
| Публичный | POST /api/auth/register | Регистрация |
| Публичный | POST /api/auth/login | JWT-токен |
| Публичный | GET /api/catalog/products | Поиск и фильтры |
| Публичный | GET /api/catalog/vin/{vin} | Подбор по сохранённому VIN |
| Покупатель | GET/POST /api/cart | Работа с корзиной |
| Покупатель | POST /api/orders | Оформление заказа |
| Покупатель | GET /api/orders | История заказов |
| Покупатель | POST /api/vehicles | Добавить автомобиль |
| Покупатель | POST /api/notifications/subscriptions | Подписка на товар |
| Admin | POST/PUT/DELETE /api/admin/products | Управление каталогом |
| Admin | GET /api/admin/orders | Все заказы |
| Admin | PUT /api/admin/orders/{id}/status | Смена статуса |
| Admin | PUT /api/admin/users/{id}/role | Назначение роли |

Полные модели запросов и ответы доступны в Swagger. Примеры также находятся в
**src/AutoPartsHub.Api/AutoPartsHub.Api.http**.

## Telegram

Telegram — дополнительный канал, отсутствие токена не мешает запуску API.

~~~powershell
$env:Telegram__BotToken = "TOKEN_FROM_BOTFATHER"
$env:Telegram__EnablePolling = "true"
~~~

Чтобы получать сообщения, авторизованный пользователь связывает чат с аккаунтом:

~~~http
POST /api/auth/telegram
Authorization: Bearer TOKEN
Content-Type: application/json

{ "chatId": 123456789 }
~~~

Поддерживаемые команды:

- **/find 21050-22010** — поиск по артикулу или названию;
- **/status ORD-20260812-1234ABCD** — статус своего заказа;
- **/help** — справка.

## Проверка

~~~bash
dotnet restore AutoPartsHub.sln
dotnet build AutoPartsHub.sln --configuration Release --no-restore
dotnet test AutoPartsHub.sln --configuration Release --no-build
~~~

Тесты проверяют резервирование остатка, неизменность корзины при ошибке,
снимок цены заказа, переходы статусов, VIN и срабатывание подписок.

## Следующие этапы

Роли Seller, SupportManager, Courier, ServicePartner и SuperAdmin заложены в
доменную модель, но их отдельные интерфейсы не входят в MVP. Следующее развитие:
реальный VIN-декодер, платёжный шлюз, интеграции поставщиков, полнотекстовый
поиск, возвраты, чат и отдельный пользовательский web-интерфейс.
