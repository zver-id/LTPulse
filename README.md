# LTPulse

**LTPulse** — дашборд KPI для IT-команд поддержки. Система периодически тянет обращения из helpdesk **ТехКас** (через COM-автоматизацию), считает командные и индивидуальные метрики (бэклог, SLA-зоны, возраст обращений, оценки, внешние сообщения), сохраняет их в **PostgreSQL**, отдаёт через **ASP.NET Core Web API** и отображает в **React** SPA. Фоновая **Windows-служба** (TechKasConnector) раскладывает задачи «посчитать отчёт» по **RabbitMQ** и обрабатывает их.

---

## Архитектура

```
┌────────────┐   COM    ┌──────────────────────────┐
│  ТехКас    │◄────────►│  TechKasConnector        │  (Windows Service)
└────────────┘          │  • сбор обращений/оценок │
                        │  • расчёт метрик         │
┌────────────┐  REST    │  • эскалации Mattermost  │
│  Mattermost │◄────────►│                          │
└────────────┘          └───────────┬──────────────┘
┌────────────┐  amqp               │  publishes / consumes
│  RabbitMQ  │◄────────────────────┤
└────────────┘                     ▼
                        ┌──────────────────────────┐      ┌────────────────┐
                        │  PostgreSQL              │◄────►│  DBCore       │
                        └──────────────────────────┘      │ (NHibernate)   │
                                                          └────────────────┘
┌────────────┐   /api/*    ┌──────────────────────────┐
│  Браузер   │◄───────────►│  WebAPI (ASP.NET Core)   │
│  (React)   │             └──────────────────────────┘
└────────────┘
```

- **Источники данных:** ТехКас (обращения, оценки), Mattermost (эскалации), внешний «клубный» портал (баллы внешних сообщений).
- **Хранилище:** PostgreSQL.
- **Обмен задачами:** RabbitMQ (очередь `task_queue`).
- **Отображение:** React SPA за nginx; API и фронт раздаёт HAProxy.

---

## Состав решения

Решение `Server/LTPulse.sln` (все проекты **net8.0**, `Nullable=enable`).

| Проект | Тип | Назначение |
|---|---|---|
| **WebAPI** | `Microsoft.NET.Sdk.Web` | REST API для SPA |
| **Application** | классический | Бизнес-сервисы, генератор Excel-отчёта, клиент RabbitMQ |
| **CommonModels** | классический | Доменные модели, интерфейсы, атрибуты |
| **DBCore** | классический | Репозиторий NHibernate/FluentNHibernate (PostgreSQL) |
| **TechKasConnector** | `Microsoft.NET.Sdk.Worker` | Windows-служба: ТехКас COM + Mattermost + потребитель RabbitMQ + расчёт метрик |
| **DataBaseInitialization** | консольное (Exe) | Сицер: загрузка справочников и метрик из Excel |
| **DBCore.Tests** | NUnit | Тесты DBCore |
| **TechKasConnector.Tests** | NUnit | Тесты клиента Mattermost |

Отдельно: `client/` — React SPA (Vite), `haproxy/`, `docker-compose.yml`.

### Зависимости проектов

**WebAPI** — `AutoMapper 15.1.0`, `Swashbuckle.AspNetCore 6.6.2`, `NLog 6.0.7` / `NLog.Web 6.1.0`; refs: Application, CommonModels.

**Application** — `ClosedXML 0.105.0`, `EPPlus 8.4.2`, `RabbitMQ.Client 7.2.0`; refs: CommonModels, DBCore. Встроенный ресурс `ReportGeneration/report_template.xlsx`.

**CommonModels** — `Newtonsoft.Json 13.0.4`.

**DBCore** — `FluentNHibernate 3.4.1`, `NHibernate 5.6.0`, `Npgsql 10.0.0-rc.1`; ref: CommonModels.

**TechKasConnector** — `HtmlAgilityPack 1.12.4`, `Microsoft.Extensions.Hosting(.WindowsServices) 8.0.1`, `NLog`; ref: Application. `InternalsVisibleTo: TechKasConnector.Tests`.

**Тесты** — `NUnit 3.14.0`, `NUnit3TestAdapter 4.5.0`, `Microsoft.NET.Test.Sdk 17.8.0`, `coverlet.collector 6.0.0`.

---

## Бэкенд

### WebAPI

Точка входа — `Server/WebAPI/Program.cs`: NLog вместо встроенного логгера, Swagger (только в Development), открытая CORS-политика (`AllowAnyOrigin/Method/Header`), DI: `NhibernateHelper` (singleton), `IRepository→DbRepository` (scoped), сервисы (`MetricsService`, `TeamService`, `EmployeeService`, `EmployeeStatsService`), AutoMapper (`ObjectToDTO`).

**Контроллеры:**

| Контроллер | Маршрут | Эндпоинты |
|---|---|---|
| `TicketsController` | `api/Tickets` | `GET` (список), `GET /byMetric`, `POST` (обновить комментарий) |
| `TeamsController` | `/api/Teams` | `GET` (все команды) |
| `MetricsController` | `/api/Metrics` | `GET` (по дням), `GET /filtered`, `GET /employee` |
| `MetricGroupController` | `api/MetricGroup` | `GET` (группы метрик) |
| `GradeController` | `api/Grade` | `GET` (оценки, с `Schema`) |
| `EmployeeController` | `api/Employee` | `GET` (сотрудники команды), `POST` (добавить) |
| `EmployeeStatsController` | `/api/EmployeeStats` | `GET ?teamId&dayCount` (KPI по сотрудникам) |
| `FileReportController` | `/api/FileReport` | `GET ?teamId` (XLSX-отчёт) |
| `TableSchemaController` | `api/tableSchema` | `GET ?tableName=ticket` (схема колонок) |

DTO — в `WebAPI/DTO`, маппинг — `WebAPI/Mappings/ObjectToDTO.cs`.

### Application

Сервисы принимают `IRepository` (базовый класс `GenericService`):

- **`TicketService`** — выборка обращений по команде/дате/типу, по метрике.
- **`TeamService`** — все команды, команда по id.
- **`MetricsService`** — метрики сгруппированные по дате (`day` + `MetricType.Name → Value`).
- **`GradeService`** — все оценки, «негативные» (Score==2, непроработанные).
- **`EmployeeService`** — сотрудники команды, добавление сотрудника.
- **`EmployeeStatsService`** — `GetStats(teamId, dayCount)`: KPI по каждому сотруднику (назначено/закрыто/бэклог, среднее время решения, SLA %, эскалации на линию/разработчиков, цветные зоны, возраст >2/3 недель/месяца, процент оценок).
- **`ReportGenerator`** — генерация `report.xlsx` (EPPlus): таблицы, графики по `MetricGroup` (Line/Area/Column), лист «Инциденты».

**RabbitMQ:** `RabbitMqConnection`, `RabbitMQClient` (продюсер, очередь `task_queue`), `RabbitMQConsumer`, payload `GenerateTeamReportRequest { TeamId, DaysAgo }`.

### CommonModels

Интерфейсы: `IRepository`, `IHasId`, `IDto`. Атрибуты: `DisplayName`, `DisplayNameGetter`, `Unique`.

Модели (все `IHasId`, `virtual`-свойства — автомаппинг NHibernate):

- **`Ticket`** — обращение (естественный id из ТехКас), включая `LineEscalationsData` / `DevsEscalationsData`.
- **`Team`** ↔ **`Employee`** (many-to-many `Employee_Teams`).
- **`Metric`** — дата, команда, сотрудник (nullable), тип, значение; many-to-many с `Ticket` (`Ticket_Metric`) и `Grade` (`Grades_Metric`); уникальность `Date+MetricType+Team`.
- **`MetricType`** → **`MetricGroup`**.
- **`Grade`** → `Ticket`, `Score`, `isResearched`.
- **`Priority`** (`TimeToSolve`, мин), **`TicketState`**, **`Job`**, **`SpecialDate`** (праздники), **`TechKasFilter`**, **`SystemParameter`**, **`MattermostChannel`** (`ChannelType`: `Line`/`Dev`).

### DBCore

- `NhibernateHelper` — `Fluently.Configure()` (PostgreSQL, ShowSql), автомаппинг `AutoMap.AssemblyOf<Ticket>(StoreConfiguration)` + override'ы, **`SchemaUpdate` при старте** (авто-миграция схемы).
- `StoreConfiguration` — мапит только namespace `CommonModels.Models`, кроме enum'ов.
- `DbRepository` — реализация `IRepository`, сессии кэшируются по `{Type}_{Id}` (dirty-tracking).
- Override'ы (`DBCore/Overrides/`): Id по assigned (Ticket, Grade), уникальные имена (Team, MetricType, Employee), many-to-many связей, составной уникальный ключ Metric.

### TechKasConnector (Windows-служба)

Запускается как **Windows-служба** (`make_service.ps1`), требует **Windows + установленный клиент ТехКас** (COM ProgID `SBLogon.LoginPoint`, `systemcode=TEHKASNPO`).

Два hosted-сервиса:

1. **`SchedulerService`** — создаёт `Job` для новых команд (интервал 180 мин), каждые минуту рассылает `GenerateTeamReportRequest` в RabbitMQ по наступившим джобам.
2. **`MetricsCalculatorService`** — потребитель `task_queue` (конкурентность = ядра − 1). На каждое сообщение: `MetricCalculator.Init(teamId)` → `ProcessAllMetrics()` → `ProcessEmployeeMetrics()`.

**`MetricCalculator`**:
- `Init` — открывает `TechKasReference("ПДД")`, применяет фильтры из БД (`TechKasFilter`) и по сотрудникам (`TechKASNumber`), строит списки обращений/оценок, затем `PopulateEscalations()`.
- `ProcessAllMetrics` / `ProcessEmployeeMetricsFor` — набор метрик: «Старше 2/3/4 недель», «Хвост», цветные зоны (0-8/8-16/16-24/>24), SLA-зоны (<0.25…>0.75), типы поступления, «Всего в работе», оценки, затраченные часы, «Внешние сообщения», метрики по месяцам.
- `PopulateEscalations` — для каждого обращения ищет его id в каналах Mattermost (таблица `MattermostChannel`) и сохраняет ссылки.

**Компоненты:**
- **`TechKasElements/`** — обёртка над COM (`TechKasReference`, `TechKasReferenceRecord`, `TechKasElement`, `TechKasElementDetail`, `ReferenceFilterManager`), справочники `Requisites/` (`TechKasRequisites`, `TicketType`, `TicketStatus`).
- **`Mattermost/`** — `MattermostClient` (поиск по истории каналов, сборка ссылок `{TeamUrl}/pl/{postId}`), `MattermostOptions`.
- **`ExternalMessageCalculator`** — парсинг внешнего портала (HtmlAgilityPack) для баллов «Внешних сообщений».
- **`CalendarCalculator`** — рабочий календарь по `SpecialDate` (рабочие минуты, праздники).
- **`Autoclicker`** — P/Invoke `user32`: автоклик «Да» в диалоге ТехКас.

### DataBaseInitialization

Консольный сицер: заполняет команды, метрики-типы, приоритеты, состояния; парсит `Aurora.xlsx`/`atlas.xlsx` в строки `Metric`. ⚠️ Строка подключения PostgreSQL захардкожена в `Program.cs`.

### Тесты

- **`DBCore.Tests`** — интеграционные (реальный локальный PostgreSQL).
- **`TechKasConnector.Tests`** — сериализация моделей Mattermost и поведение `MattermostClient` (через stub `HttpMessageHandler`).

---

## Фронтенд (`client/`)

**Стек:** React 19, Vite 7, TypeScript 5.9, Redux Toolkit (RTK Query), **Ant Design 6**, Recharts 3, dayjs.

- `package.json` — скрипты: `dev`, `build` (`tsc -b && vite build`), `lint`, `preview`.
- `main.tsx` — `<Provider store>` + `<Router/>`.
- `storage/store.ts` — store с 8 RTK Query-редьюсерами: `teams`, `metrics`, `tickets`, `grade`, `metricGroups`, `employee`, `tableSchema`, `employeeStats`.
- `storage/services/api-url.ts` — базовый URL из `VITE_API_URL`.
- `useLocalStorageState` — persist-хук (выбранная команда, период).

**Маршруты** (`components/router/Router.tsx`):

| Путь | Страница |
|---|---|
| `/` | `MainScreen` — статистика: навигация (команда + период) + `ViewRouter` |
| `/teams` | `TeamsSettings` |
| `/employee-stats` | `EmployeeStats` — таблица KPI по сотрудникам (antd Table) |

**Режимы главной страницы** (`viewRouter`): «Графики» (line/area/bar по группам метрик) и вкладки обращений (Инциденты/Консультации/Запросы/Старше N недель), «Оценки».

**Ключевые компоненты:** `ticketsScreen` (таблица обращений с редактированием), `gradeScreen` (таблица оценок), `employeeStats` (таблица KPI), `lineChart`/`areaChart`/`barChart` (графики Recharts), `metricDetailsModal` (обращения по метрике), `editItemModal`.

### Сборка и раздача

- `client/Dockerfile` — multi-stage: `node:22-alpine` (сборка) → `nginx:stable-alpine` (раздача `dist`).
- `client/nginx.conf` — SPA-fallback `try_files … /index.html`, кэш статики.

---

## Инфраструктура

`docker-compose.yml` (сеть `app-network`):

| Сервис | Образ | Порт | Назначение |
|---|---|---|---|
| `webserver` | `./Server` | 5000 (внутр.) | WebAPI |
| `webclient` | `./client` | 80 (внутр.) | nginx + SPA |
| `haproxy` | `haproxy:latest` | **80 (внешний)** | роутинг |

`haproxy/haproxy.cfg`: `path_beg /api` → `webserver:5000`, остальное → `webclient:80`.

PostgreSQL, RabbitMQ и Mattermost — **внешние** сервисы (указываются в connection-строках).

⚠️ `Server/Dockerfile` копирует уже собранные `bin/*` (без стадии сборки) — перед `docker compose build` решение нужно собрать.

### Локальный запуск стека

```bash
# 1. Собрать бэкенд в Server/bin (для Dockerfile)
dotnet build Server/LTPulse.sln -c Release

# 2. Поднять API + фронт + прокси
docker compose up --build
```

Фронт доступен на `http://localhost` (HAProxy). API — `http://localhost/api/...`.

### Локальная разработка (без Docker)

**Бэкенд:**
```bash
dotnet run --project Server/WebAPI          # http://localhost:5012 (dev)
dotnet run --project Server/TechKasConnector# Windows + ТехКас
```

**Фронт:**
```bash
cd client
npm install
npm run dev                                  # Vite dev-server
```
URL API задаётся в `client/.env` (`VITE_API_URL=http://localhost:5012`).

### Windows-служба TechKasConnector

```powershell
cd Server\TechKasConnector
.\make_service.ps1     # создаёт службу "TechKas Connector 1.1"
```

---

## Конфигурация

Шаблон — `Server/TechKasConnector/appsettings.json.example`:

```jsonc
{
  "ConnectionStrings": {
    "RabbitMQ":   "amqp://user:password@host:5672/vhost",
    "PostgreSQL": "Host=localhost;Port=5432;Database=LTPulse;Username=user;Password=password"
  },
  "ClubDetails": { "Url": "https://club.example.com", "Name": "user@domain.ru", "Password": "password" },
  "Mattermost": {
    "Url":     "https://mattermost.example.com/api/v4/",
    "Token":   "personal-access-token",
    "TeamUrl": "https://mattermost.example.com/team-slug"
  }
}
```

| Секция | Использование |
|---|---|
| `ConnectionStrings.PostgreSQL` | DBCore / WebAPI / TechKasConnector |
| `ConnectionStrings.RabbitMQ` | TechKasConnector (и WebAPI) |
| `ClubDetails` | `ExternalMessageCalculator` (внешние сообщения) |
| `Mattermost` | `MattermostClient` (эскалации) |
| `NLog` | JSON-лог в файл + консоль |

`MattermostChannel` (список каналов эскалаций) хранится **в БД**, а не в конфиге — инициализация: `ConvertScripts/init_mattermost_channels.sql`.

---

## Скрипты БД

- `ConvertScripts/init_mattermost_channels.sql` — 6 каналов Mattermost (4 линии + 2 разработчика).
- `ConvertScripts/add_tems_to_employee_after_many_to_many_change.sql` — миграция `Employee.team_id` → таблицу `employee_teams`.

---

## Примечания и ограничения

- **TechKasConnector** работает только на Windows с установленным клиентом ТехКас; разворачивается как служба (`make_service.ps1`), не в Docker.
- Схема БД обновляется автоматически при старте (`SchemaUpdate`) — без миграций в руках.
- Строки подключения PostgreSQL захардкожены в `DataBaseInitialization` и `DBCore.Tests`.
- `Server/Dockerfile` не содержит стадии сборки — копирует готовые `bin/*`.
- `client/App.tsx` — неиспользуемый шаблон Vite; входной компонент — `Router`.
