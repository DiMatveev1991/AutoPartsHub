# AutoParts Hub

Выпускной проект по курсу **OTUS C# Basic**: Telegram-платформа для поиска и
заказа новых и бывших в употреблении автозапчастей. Тема и MVP взяты из плана
выпускного проекта в ветке `HomeWork17` репозитория домашних работ.

База данных реализована через **Entity Framework Core Code First** и PostgreSQL:
структура таблиц описывается C#-сущностями и конфигурацией `DbContext`, а
изменения схемы хранятся в миграциях.

## Возможности MVP

- регистрация пользователя по Telegram chat id при первой команде;
- каталог, категории и поиск по названию, описанию или артикулу;
- сохранение автомобиля и подбор совместимых деталей по VIN;
- корзина, удаление позиций и оформление заказа;
- история заказов и просмотр текущего статуса;
- подписки на появление товара и снижение цены;
- фоновое создание и отправка уведомлений;
- команды администратора для категорий, товаров и статусов заказов;
- консольный режим для демонстрации без Telegram-токена;
- PostgreSQL через DAL и EF Core Code First вместо коллекций `List<T>`.

В MVP две роли: **Customer** и **Admin**. Остальные роли из полного плана
площадки не добавлены, потому что они не нужны для защиты выпускного минимума.

## Архитектура

Проект сделан как модульный монолит из привычных учебных слоёв:

~~~mermaid
flowchart TD
    Bot["AutoPartsHub.TelegramBot<br/>PL: команды и Telegram API"] --> BLL["AutoPartsHub.BLL<br/>бизнес-сценарии"]
    Bot --> DAL["AutoPartsHub.DAL<br/>EF Core и PostgreSQL"]
    BLL --> Core["AutoPartsHub.Core<br/>сущности и интерфейсы"]
    DAL --> Core
    DAL --> DB[(PostgreSQL)]
~~~

- **Core** — сущности, перечисления, бизнес-правила и интерфейсы хранилища;
- **BLL** (Business Logic Layer) — каталог, корзина, заказы, VIN и подписки;
- **DAL** (Data Access Layer) — `DbContext`, репозиторий и миграции Code First;
- **TelegramBot** (Presentation Layer) — команды, консольный режим и DI;
- **Tests** — модульные тесты доменных правил.

`DLL` — это файл собранной .NET-библиотеки, а не отдельный архитектурный слой.
Code First отвечает за создание схемы БД и не заменяет DAL/BLL.

Зависимости направлены просто: BLL и DAL знают только о Core, а TelegramBot
собирает приложение. DAL не зависит от BLL.

## Документация исходного кода

Созданные вручную классы, интерфейсы, перечисления, записи, свойства и методы
снабжены XML-комментариями `summary`. Для позиционных `record` назначение
автоматически создаваемых свойств описано тегами `param`. Внутренние комментарии
оставлены возле неочевидной логики: транзакции оформления заказа, снимка цены,
оптимистичной блокировки, постраничной выборки и фоновой отправки уведомлений.

Во время сборки создаются XML-файлы документации. Параметр
`GenerateDocumentationFile` задан централизованно в `Directory.Build.props`, а
предупреждения считаются ошибками. Благодаря этому новая публичная сущность без
XML-документации будет обнаружена при обычной сборке. Автоматически созданные EF
Core файлы в `Persistence/Migrations` вручную не редактируются.

## Команды пользователя

| Команда | Назначение |
|---|---|
| `/start`, `/help` | регистрация и справка |
| `/catalog` | первые десять товаров |
| `/categories` | список категорий |
| `/find <текст>` | поиск по артикулу или названию |
| `/vehicle VIN\|Марка\|Модель\|Год\|Двигатель` | сохранить автомобиль |
| `/vin <VIN>` | подобрать совместимые товары |
| `/addcart <артикул> <количество>` | добавить товар в корзину |
| `/cart` | показать корзину |
| `/remove <артикул>` | удалить товар из корзины |
| `/checkout Имя\|Телефон\|Адрес\|Courier\|CashOnDelivery` | оформить заказ |
| `/orders` | история заказов |
| `/status <номер>` | статус заказа |
| `/subscribe <артикул> [цена]` | подписка на наличие или цену |
| `/notifications` | последние уведомления |

Команды администратора:

| Команда | Назначение |
|---|---|
| `/addcategory Название\|slug` | добавить категорию |
| `/addproduct slug\|артикул\|название\|описание\|New\|цена\|остаток` | добавить товар |
| `/adminorders` | показать все заказы |
| `/setstatus НОМЕР\|Shipped` | изменить статус заказа |

Допустимые значения состояния товара: `New`, `Used`, `Refurbished`. Способы
доставки: `Pickup`, `Courier`, `TransportCompany`. Способы оплаты:
`CardOnline`, `CashOnDelivery`.

## Модель данных и Code First

Миграция создаёт таблицы `Users`, `Vehicles`, `Categories`, `Products`,
`ProductCompatibilities`, `Carts`, `CartItems`, `Orders`, `OrderItems`,
`ProductSubscriptions` и `Notifications`.

Интерфейс `IAutoPartsRepository` находится в Core, а реализация
`AutoPartsRepository` — в DAL. Поэтому бизнес-логика не связана напрямую с EF
Core. Цена и название копируются в `OrderItem` при оформлении: старый заказ не
изменится после обновления каталога.

## Быстрый запуск в консоли

Требуются .NET SDK 9 и PostgreSQL. Запустить только PostgreSQL через Docker:

~~~bash
docker compose up -d postgres
~~~

Затем:

~~~bash
dotnet restore AutoPartsHub.sln
dotnet run --project src/AutoPartsHub.TelegramBot
~~~

По умолчанию включён консольный режим. После применения миграции приложение
добавит три демонстрационных товара. Введите `/start`, затем `/catalog`.
Команда `/exit` завершает приложение.

## Запуск Telegram-бота

Создайте бота через BotFather и задайте настройки переменными окружения.

PowerShell:

~~~powershell
$env:ConnectionStrings__PostgreSQL = "Host=localhost;Port=5432;Database=AutoPartsHub;Username=postgres;Password=YOUR_PASSWORD"
$env:Telegram__BotToken = "TOKEN_FROM_BOTFATHER"
$env:Telegram__EnablePolling = "true"
$env:Telegram__EnableConsole = "false"
$env:Telegram__AdminChatIds__0 = "YOUR_TELEGRAM_CHAT_ID"
dotnet run --project src/AutoPartsHub.TelegramBot
~~~

Bash:

~~~bash
export ConnectionStrings__PostgreSQL='Host=localhost;Port=5432;Database=AutoPartsHub;Username=postgres;Password=YOUR_PASSWORD'
export Telegram__BotToken='TOKEN_FROM_BOTFATHER'
export Telegram__EnablePolling='true'
export Telegram__EnableConsole='false'
export Telegram__AdminChatIds__0='YOUR_TELEGRAM_CHAT_ID'
dotnet run --project src/AutoPartsHub.TelegramBot
~~~

Пользователь из `AdminChatIds` получает роль Admin при первой команде. Если id
добавили позже, роль обновится при следующем сообщении.

## Миграции EF Core

Установить инструмент:

~~~bash
dotnet tool install --global dotnet-ef --version 9.*
~~~

Создать следующую миграцию:

~~~bash
dotnet ef migrations add MigrationName --project src/AutoPartsHub.DAL --startup-project src/AutoPartsHub.TelegramBot --output-dir Persistence/Migrations
~~~

Применить миграции вручную:

~~~bash
dotnet ef database update --project src/AutoPartsHub.DAL --startup-project src/AutoPartsHub.TelegramBot
~~~

При обычном запуске ещё не выполненные миграции применяются автоматически.

## Проверка

~~~bash
dotnet restore AutoPartsHub.sln
dotnet build AutoPartsHub.sln --configuration Release --no-restore
dotnet test AutoPartsHub.sln --configuration Release --no-build
~~~

Тесты проверяют валидацию Telegram-пользователя, корзину и остатки, оформление
заказа и снимок цены, допустимые переходы статусов, VIN и подписки.

## Соответствие заданию OTUS

1. Тема — онлайн-площадка автозапчастей.
2. Роли и функции описаны выше; в MVP оставлены Customer и Admin.
3. Запросы и ответы представлены командами Telegram и сообщениями бота.
4. Минимум реализован: каталог, пользователь, корзина/заказ, администрирование.
5. Есть консольный вариант интерфейса для локальной демонстрации.
6. Коллекции заменены интерфейсом репозитория и PostgreSQL, консоль — Telegram API.
7. Использованы темы курса: ООП, интерфейсы, LINQ, async/await, DI, фоновые задачи,
   PostgreSQL, EF Core Code First и модульные тесты.
