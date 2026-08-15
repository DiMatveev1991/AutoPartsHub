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

| Роль | Функции |
|---|---|
| **Customer** | регистрация, каталог и поиск, сохранение автомобиля, VIN-подбор, корзина, оформление и просмотр заказов, подписки и уведомления |
| **Admin** | все функции покупателя, а также создание категорий и товаров, просмотр всех заказов и изменение их статусов |

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

## Запросы и ответы MVP

Консоль и Telegram используют один `BotCommandHandler`, поэтому принимают
одинаковые запросы и возвращают одинаковые ответы. В таблицах приведён основной
успешный ответ; при ошибке формата или бизнес-правила бот отвечает
`Ошибка: <описание>`.

### Команды покупателя

| Запрос | Функция | Основной ответ |
|---|---|---|
| `/start` | регистрация при первом обращении и справка | `Добро пожаловать, <имя>!` и список команд |
| `/help` | показать справку | список доступных пользовательских и административных команд |
| `/catalog` | первые десять активных товаров | `Товары: <артикул — название, цена, остаток>` или `Каталог пуст.` |
| `/categories` | список категорий | `Категории: <название (slug)>` или `Категории пока не добавлены.` |
| `/find <текст>` | поиск по артикулу, названию или описанию | список найденных товаров или `Каталог пуст.` |
| `/vehicle VIN\|Марка\|Модель\|Год\|Двигатель` | сохранить автомобиль | `Автомобиль сохранён: <марка> <модель>, VIN <VIN>.` |
| `/vin <VIN>` | подобрать совместимые товары | список товаров или `Для этого автомобиля товары не найдены.` |
| `/addcart <артикул> <количество>` | добавить товар в корзину | `Товар добавлен.` и актуальное содержимое корзины |
| `/cart` | показать корзину | позиции, количество, цены и итог либо `Корзина пуста.` |
| `/remove <артикул>` | удалить позицию | `Товар удалён.` и актуальное содержимое корзины |
| `/checkout Имя\|Телефон\|Адрес\|Courier\|CashOnDelivery` | оформить заказ | `Заказ <номер> оформлен. Сумма <сумма> ₽, статус <статус>.` |
| `/orders` | история заказов пользователя | номера, статусы и суммы либо `У вас пока нет заказов.` |
| `/status <номер>` | статус собственного заказа | `Заказ <номер>: <статус>. Сумма <сумма> ₽.` |
| `/subscribe <артикул> [цена]` | подписка на наличие или снижение цены | подтверждение подписки на наличие либо целевую цену |
| `/notifications` | последние уведомления | до десяти сообщений со статусами либо `Уведомлений пока нет.` |

### Команды администратора

| Запрос | Функция | Основной ответ |
|---|---|---|
| `/addcategory Название\|slug` | создать категорию | `Категория <название> создана.` |
| `/addproduct slug\|артикул\|название\|описание\|New\|цена\|остаток` | создать товар | `Товар <артикул> создан.` |
| `/adminorders` | последние заказы всех пользователей | список заказов либо `Заказов пока нет.` |
| `/setstatus НОМЕР\|Shipped` | изменить статус заказа | `Статус заказа <номер> изменён на <статус>.` |

Неизвестная команда возвращает: `Неизвестная команда. Используйте /help.`

Допустимые значения состояния товара:Допустимые значения состояния товара: `New`, `Used`, `Refurbished`. Способы
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


### Схема конечной базы данных

Диаграмма соответствует актуальным сущностям, настройкам
`AutoPartsDbContext` и Code First-миграции. Обозначения: `PK` — первичный
ключ, `FK` — внешний ключ, `UK` — уникальный ключ.

~~~mermaid
erDiagram
    Users {
        uuid Id PK
        bigint TelegramChatId UK
        string DisplayName
        int Role
        datetime CreatedAt
    }
    Vehicles {
        uuid Id PK
        uuid UserId FK
        string Vin UK
        string Make
        string Model
        int Year
        string Engine
    }
    Categories {
        uuid Id PK
        string Name
        string Slug UK
    }
    Products {
        uuid Id PK
        uuid CategoryId FK
        string Article UK
        string Name
        string Description
        int Condition
        decimal Price
        int Stock
        bool IsActive
        uuid ConcurrencyToken
        datetime CreatedAt
        datetime UpdatedAt
    }
    ProductCompatibilities {
        uuid Id PK
        uuid ProductId FK
        string Make
        string Model
        int YearFrom
        int YearTo
        string Engine
    }
    Carts {
        uuid Id PK
        uuid UserId FK, UK
        datetime UpdatedAt
    }
    CartItems {
        uuid CartId PK, FK
        uuid ProductId PK, FK
        int Quantity
    }
    Orders {
        uuid Id PK
        uuid UserId FK
        string OrderNumber UK
        int Status
        string ContactName
        string Phone
        string DeliveryAddress
        int DeliveryMethod
        int PaymentMethod
        decimal Total
        datetime CreatedAt
        datetime UpdatedAt
    }
    OrderItems {
        uuid Id PK
        uuid OrderId FK
        uuid ProductId FK
        string Article
        string ProductName
        decimal UnitPrice
        int Quantity
    }
    ProductSubscriptions {
        uuid Id PK
        uuid UserId FK
        uuid ProductId FK
        int Type
        decimal TargetPrice
        bool IsActive
        datetime CreatedAt
    }
    Notifications {
        uuid Id PK
        uuid UserId FK
        string Type
        string Text
        int Status
        datetime CreatedAt
        datetime SentAt
        string Error
    }

    Users ||--o{ Vehicles : "владеет"
    Users ||--o| Carts : "имеет"
    Users ||--o{ Orders : "оформляет"
    Users ||--o{ ProductSubscriptions : "создаёт"
    Users ||--o{ Notifications : "получает"
    Categories ||--o{ Products : "содержит"
    Products ||--o{ ProductCompatibilities : "имеет"
    Carts ||--o{ CartItems : "содержит"
    Products ||--o{ CartItems : "добавляется в"
    Orders ||--o{ OrderItems : "содержит"
    Products ||--o{ OrderItems : "фиксируется в"
    Products ||--o{ ProductSubscriptions : "отслеживается"
~~~

Связи и поведение внешних ключей:

| Родитель → зависимая таблица | Кратность | Удаление |
|---|---|---|
| `Users → Vehicles` | один-ко-многим | `Cascade`: автомобили удаляются вместе с пользователем |
| `Users → Carts` | один-к-нулю-или-одному | `Cascade`: корзина удаляется вместе с пользователем |
| `Users → Orders` | один-ко-многим | `Restrict`: пользователь с заказами не удаляется |
| `Users → ProductSubscriptions` | один-ко-многим | `Cascade` |
| `Users → Notifications` | один-ко-многим | `Cascade` |
| `Categories → Products` | один-ко-многим | `Restrict`: категория с товарами не удаляется |
| `Products → ProductCompatibilities` | один-ко-многим | `Cascade` |
| `Carts → CartItems` | один-ко-многим | `Cascade` |
| `Products → CartItems` | один-ко-многим | `Restrict` |
| `Orders → OrderItems` | один-ко-многим | `Cascade` |
| `Products → OrderItems` | один-ко-многим | `Restrict`: история заказа сохраняет ссылку на товар |
| `Products → ProductSubscriptions` | один-ко-многим | `Cascade` |

Для товаров применяется мягкое удаление: административная операция вызывает
`Product.Deactivate()`, устанавливает `Products.IsActive = false` и не
удаляет строку. Каталог выбирает только активные товары, а резервирование
дополнительно проверяет `IsActive`. Для остальных сущностей действует
настроенное в таблице поведение `Cascade` или `Restrict`.

Основные ограничения и индексы:

- уникальны `Users.TelegramChatId`, `Vehicles.Vin`, `Categories.Slug`,
  `Products.Article`, `Carts.UserId` и `Orders.OrderNumber`;
- у `CartItems` составной первичный ключ `CartId + ProductId`, поэтому один
  товар не может дважды появиться отдельными строками в одной корзине;
- денежные поля хранятся с точностью `numeric(12,2)`;
- перечисления ролей, статусов, способов доставки и оплаты хранятся как `int`;
- `Products.ConcurrencyToken` используется EF Core для оптимистичной блокировки;
- дополнительные составные индексы ускоряют каталог, VIN-подбор, историю
  заказов, обработку подписок и очередь уведомлений.

## Настройка секретов

Реальные пароли и Telegram-токен в репозитории не хранятся.
`appsettings.json` содержит только несекретные переключатели. Для Docker
скопируйте шаблон и задайте собственный пароль:

~~~powershell
Copy-Item .env.example .env
# Откройте .env и замените CHANGE_ME на собственный пароль.
docker compose up --build
~~~

Для Bash:

~~~bash
cp .env.example .env
# Откройте .env и замените CHANGE_ME на собственный пароль.
docker compose up --build
~~~

Файл `.env` исключён из Git. `.env.example` содержит только названия
переменных и безопасные заполнители.

## Быстрый запуск в консоли

Требуются .NET SDK 9 и PostgreSQL. Сначала создайте локальный `.env` из
шаблона, задайте `POSTGRES_PASSWORD` и запустите только PostgreSQL:

~~~powershell
Copy-Item .env.example .env
docker compose up -d postgres
$env:ConnectionStrings__PostgreSQL = "Host=localhost;Port=5432;Database=AutoPartsHub;Username=postgres;Password=ВАШ_ПАРОЛЬ"
dotnet restore AutoPartsHub.sln
dotnet run --project src/AutoPartsHub.TelegramBot
~~~

В Bash используется та же строка через `export ConnectionStrings__PostgreSQL=...`.
По умолчанию включён консольный режим. После применения миграции приложение
добавит три демонстрационных товара. Введите `/start`, затем `/catalog`.
Команда `/exit` завершает приложение.

## Запуск Telegram-бота
## Запуск Telegram-бота

Создайте бота через BotFather и задайте настройки переменными окружения.

PowerShell:

~~~powershell
$env:ConnectionStrings__PostgreSQL = "Host=localhost;Port=5432;Database=AutoPartsHub;Username=postgres;Password=ВАШ_ПАРОЛЬ"
$env:Telegram__BotToken = "TOKEN_FROM_BOTFATHER"
$env:Telegram__EnablePolling = "true"
$env:Telegram__EnableConsole = "false"
$env:Telegram__AdminChatIds__0 = "YOUR_TELEGRAM_CHAT_ID"
dotnet run --project src/AutoPartsHub.TelegramBot
~~~

Bash:

~~~bash
export ConnectionStrings__PostgreSQL='Host=localhost;Port=5432;Database=AutoPartsHub;Username=postgres;Password=ВАШ_ПАРОЛЬ'
export Telegram__BotToken='TOKEN_FROM_BOTFATHER'
export Telegram__EnablePolling='true'
export Telegram__EnableConsole='false'
export Telegram__AdminChatIds__0='YOUR_TELEGRAM_CHAT_ID'
dotnet run --project src/AutoPartsHub.TelegramBot
~~~

Пользователь из `AdminChatIds` получает роль Admin при первой команде. Если id
добавили позже, роль обновится при следующем сообщении.

## Миграции EF Core

Design-time фабрика также не содержит пароля. Перед командами `dotnet ef`
задайте строку подключения в переменной окружения:

~~~powershell
$env:AUTOPARTS_DB_CONNECTION_STRING = "Host=localhost;Port=5432;Database=AutoPartsHub;Username=postgres;Password=ВАШ_ПАРОЛЬ"
~~~

Для Bash:

~~~bash
export AUTOPARTS_DB_CONNECTION_STRING='Host=localhost;Port=5432;Database=AutoPartsHub;Username=postgres;Password=ВАШ_ПАРОЛЬ'
~~~

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
