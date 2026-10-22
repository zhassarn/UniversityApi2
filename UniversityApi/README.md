# University Course Management API

REST Web API на **ASP.NET Core 8** для управления студентами, преподавателями, курсами и записями студентов на курсы.
БД: **PostgreSQL** (через Entity Framework Core). Документация: **Swagger / OpenAPI**.

## Технологии
ASP.NET Core Web API · EF Core 8 + Npgsql · AutoMapper · Repository Pattern · Dependency Injection · Swagger (Swashbuckle) · ILogger

## Быстрый старт (часть 1)
> ⚠ После части 2 запуск изменился (миграции, секреты) — см. раздел **«Часть 2»** ниже.

```bash
docker compose up -d

# 2. Затически при старте)
dotnet restore
dotnet run
```
Swagger UI: `http://localhost:<port>/swagger` (порт виден в консоли при запуске; корень `/` перенаправляет на Swagger).
Строка подключения — в `appsettings.json` (`ConnectionStrings:DefaultConnection`).

### Дамп БД с тестовыми данными
```bash
docker exec -t university_db pg_dump -U postgres university_db > db/university_db.sql
```

## Архитектура
```
Client
  ↓ HTTP
Controller            (маршрутизация, HTTP-коды, ReturnResult<T>)
  ↓
Service               (бизнес-правила, логирование, AutoMapper)
  ↓
Repository            (доступ к данным)
  ↓
Entity Framework Core (ApplicationDbContext)
  ↓
PostgreSQL
```
Сквозная часть: `ExceptionHandlingMiddleware` — централизованная обработка ошибок.

### Структура проекта
| Папка | Назначение |
|---|---|
| `Controllers` | StudentsController, TeachersController, CoursesController, EnrollmentsController |
| `Models` | Entity: Student, Teacher, Course, Enrollment |
| `DTO` | Create/Update/Response DTO + Data Annotations валидация |
| `Data` | `ApplicationDbContext`, `DbSeeder` |
| `Repositories` | Интерфейсы и реализации репозиториев |
| `Services` | Бизнес-логика |
| `Mapping` | `MappingProfile` (AutoMapper) |
| `Middleware` | Глобальная обработка исключений |
| `Common` | `ReturnResult<T>`, `AppException` |

### Dependency Injection (lifetime)
`DbContext`, репозитории и сервисы — **Scoped**: DbContext живёт один HTTP-запрос, а репозитории/сервисы зависят от него, поэтому не могут быть Singleton; Transient не нужен, т.к. состояние должно быть общим внутри запроса. AutoMapper — Singleton (регистрируется библиотекой).

### Конфигурация модели EF Core
- **Data Annotations**: `[Key]`, `[Required]`, `[MaxLength]`, `[Column]`
- **Fluent API**: связи, внешние ключи, `DeleteBehavior`, уникальные индексы (Email; пара StudentId+CourseId)
- **Conventions**: имена таблиц/колонок, типы FK по соглашению

## Endpoints
| Метод | URL | Описание | Коды |
|---|---|---|---|
| GET | `/api/students` | Список студентов | 200 |
| GET | `/api/students/{id}` | Студент по ID | 200, 404 |
| GET | `/api/students/{id}/courses` | Студент с его курсами | 200, 404 |
| POST | `/api/students` | Создать студента | 201, 400, 409 |
| PUT | `/api/students/{id}` | Изменить студента | 200, 400, 404, 409 |
| DELETE | `/api/students/{id}` | Удалить студента | 200, 404 |
| GET/POST | `/api/teachers` | Список / создание преподавателя | 200 / 201, 400, 409 |
| GET/PUT/DELETE | `/api/teachers/{id}` | Получить / изменить / удалить | 200, 400, 404, 409 |
| GET | `/api/courses?name=&teacherId=&minCredits=&maxCredits=` | Список курсов с фильтрацией | 200, 400 |
| GET/PUT/DELETE | `/api/courses/{id}` | Получить / изменить / удалить | 200, 400, 404 |
| POST | `/api/courses` | Создать курс | 201, 400, 404 (нет преподавателя) |
| GET | `/api/enrollments` | Список записей | 200 |
| GET | `/api/enrollments/{id}` | Запись по ID | 200, 404 |
| POST | `/api/enrollments` | Записать студента на курс | 201, 400, 404, 409 |
| PUT | `/api/enrollments/{id}/grade` | Изменить оценку (0–100) | 200, 400, 404 |
| DELETE | `/api/enrollments/{id}` | Удалить студента с курса | 200, 404 |

## Формат ответа
Все endpoints возвращают `ReturnResult<T>`:
```json
{ "statusCode": 200, "isSuccess": true, "result": { }, "errorCode": null, "errorMessage": null, "errors": null, "traceId": "..." }
```
Ошибка валидации (400) дополнительно содержит `errors` — словарь «поле → сообщения».

## Бизнес-правила
1. Нельзя записать студента на несуществующий курс — 404 `COURSE_NOT_FOUND`
2. Нельзя записать несуществующего студента — 404 `STUDENT_NOT_FOUND`
3. Повторная запись на тот же курс — 409 `ALREADY_ENROLLED`
4. Курс с несуществующим преподавателем — 404 `TEACHER_NOT_FOUND`
5. Несуществующий объект никогда не возвращается как успех — 404
6. Дополнительно: уникальный Email у студентов и преподавателей (409); нельзя удалить преподавателя, у которого есть курсы (409 `TEACHER_HAS_COURSES`)

## Логирование
`ILogger<T>`: **Information** — создание/изменение/удаление; **Warning** — объект не найден, нарушение бизнес-правил; **Error** — непредвиденные исключения (со StackTrace только в логе, клиент получает `INTERNAL_SERVER_ERROR`).

## Демо-сценарии
Файл `requests.http` содержит запросы для всех 15 сценариев защиты (VS Code REST Client / Rider).


---

# Часть 2. Безопасность, пользователи, версии API, клиент

Часть 2 — продолжение части 1: CRUD, Repository, DTO, AutoMapper, `ReturnResult<T>`, middleware ошибок, логирование **сохранены**.
Старые URL (`/api/students`, `/api/courses` …) продолжают работать (= версия v1).

## Что добавлено
| Возможность | Реализация |
|---|---|
| CORS | именованная политика `ClientApps`, разрешены только адреса из `Cors:AllowedOrigins` (`http://localhost:5100`, `http://localhost:5200`) |
| Клиент | отдельный проект `UniversityClient` (ASP.NET Core MVC, `IHttpClientFactory`) |
| Identity | `ApplicationUser : IdentityUser` + `IdentityDbContext`, `UserManager`, `SignInManager`, блокировка после 3 неудачных входов (5 мин), пароль ≥ 8 символов, цифра, заглавная и строчная буквы |
| JWT | `POST /api/auth/login`, проверка подписи (HS256), Issuer, Audience, срока (`ClockSkew = 0`), срок жизни 30 мин (`Jwt:AccessTokenMinutes`) |
| Роли | `Admin`, `Teacher`, `Student`; политики `AdminOnly`, `ManageGrades`, `StudentOrAdmin` |
| Принадлежность ресурсов | `ICurrentUserService` (claims `studentId` / `teacherId`) + проверки в сервисах |
| Версии | `Asp.Versioning`: `/api/v1/courses`, `/api/v2/courses`; Swagger для v1 и v2 |

## Запуск (обязательные шаги)

### 1. Секреты (вне Git)
```bash
cd UniversityApi
dotnet user-secrets set "Jwt:Key" "ЗАМЕНИТЕ-НА-СЛУЧАЙНУЮ-СТРОКУ-МИНИМУМ-32-СИМВОЛА"
dotnet user-secrets set "Seed:Password" "Passw0rd123"     # пароль тестовых пользователей
```
(либо переменные окружения `Jwt__Key`, `Seed__Password`). Строка подключения к БД — в `appsettings.json`.

### 2. Миграции
БД из части 1 была создана через `EnsureCreated`, поэтому её нужно **удалить** и создать заново миграциями:
```bash
# удалить старую БД (pgAdmin → Delete, либо:)
psql -U postgres -c "DROP DATABASE IF EXISTS university_db;"

dotnet tool install --global dotnet-ef        # один раз
dotnet ef migrations add InitialCreate        # создаст папку Migrations (закоммитьте её)
```
При старте приложение само применяет миграции (`MigrateAsync`), заполняет тестовые данные и создаёт роли/пользователей.

### 3. Запуск
```bash
dotnet run            # API:    http://localhost:5000/swagger  (v1 и v2 — в выпадающем списке)
cd ../UniversityClient
dotnet run            # Клиент: http://localhost:5100
```

## Тестовые пользователи (пароль = `Seed:Password`)
| Пользователь | Роль | Привязка |
|---|---|---|
| `admin` | Admin | — |
| `teacher1` | Teacher | Aidar Nurlanov (курсы Id 1 и 4) |
| `teacher2` | Teacher | Madina Sarsenova (курс Id 3) |
| `student1` | Student | Zhassar Student (записи на курсы 1 и 2) |
| `student2` | Student | Alex Smith (запись на курс 1) |

Скрипт создания ролей и пользователей — `Data/IdentitySeeder.cs` (идемпотентный, запускается при старте).

## Схема прохождения запроса
```
Client ──HTTP + Authorization: Bearer <JWT>──▶ ExceptionHandlingMiddleware
   ▶ CORS ▶ UseAuthentication (JwtBearer: подпись, issuer, audience, срок → ClaimsPrincipal с claims sub/name/role/studentId/teacherId)
   ▶ UseAuthorization ([Authorize] / [AllowAnonymous], policy по ролям)
        ├─ нет/плохой токен → 401 (ReturnResult, errorCode UNAUTHORIZED / INVALID_TOKEN / TOKEN_EXPIRED)
        └─ роль не подходит → 403 FORBIDDEN
   ▶ Controller ▶ Service (проверка принадлежности ресурса: «свой студент / свой курс» → иначе 403)
   ▶ Repository ▶ EF Core ▶ PostgreSQL
```

## Права доступа
| Endpoint | Anonymous | Student | Teacher | Admin |
|---|---|---|---|---|
| `GET /api/v{1,2}/courses[/{id}]` | ✔ | ✔ | ✔ | ✔ |
| `POST/PUT/DELETE /api/v1/courses` | 401 | 403 | 403 | ✔ |
| `GET /api/v1/students` | 401 | 403 | 403 | ✔ |
| `GET /api/v1/students/{id}`, `/{id}/courses` | 401 | только свой | 403 | ✔ |
| `POST/PUT/DELETE` students, teachers | 401 | 403 | 403 | ✔ |
| `GET /api/v1/teachers[/{id}]` | 401 | ✔ | ✔ | ✔ |
| `GET /api/v1/enrollments` | 401 | только свои | только по своим курсам | все |
| `GET /api/v1/enrollments/{id}` | 401 | только своя | только по своему курсу | ✔ |
| `POST /api/v1/enrollments` | 401 | только себя | 403 | ✔ |
| `PUT /api/v1/enrollments/{id}/grade` | 401 | 403 | только по своему курсу | ✔ |
| `DELETE /api/v1/enrollments/{id}` | 401 | 403 | 403 | ✔ |
| `POST /api/auth/register`, `/login` | ✔ | ✔ | ✔ | ✔ |
| `GET /api/auth/me` | 401 | ✔ | ✔ | ✔ |
| `GET /api/users`, `PUT /api/users/{id}/role`, `/link` | 401 | 403 | 403 | ✔ |

Регистрация всегда даёт роль `Student` (поле роли в `RegisterDto` отсутствует). Роли и привязку к Student/Teacher меняет только Admin; Admin не может менять собственную роль.

## Проверка комплексных сценариев (раздел 7)
Все запросы — в `requests.http` (VS Code REST Client / Rider) или через Swagger (кнопка **Authorize**, вставить `accessToken` без слова `Bearer`).

| № | Сценарий | Как проверить | Результат |
|---|---|---|---|
| 1 | Аноним: список курсов | `GET /api/v1/courses` без токена | 200 |
| 2 | Аноним создаёт курс | `POST /api/v1/courses` без токена | 401 |
| 3 | Неверный пароль | `POST /api/auth/login` с неверным паролем | 401 `INVALID_CREDENTIALS`, токена нет |
| 4 | Верные данные | login `teacher1` | 200 + `accessToken` |
| 5 | Просроченный JWT | поставить `Jwt:AccessTokenMinutes` = 1, войти, подождать > 1 мин, вызвать `GET /api/auth/me` | 401 `TOKEN_EXPIRED` |
| 6 | Student удаляет курс | `DELETE /api/v1/courses/1` с токеном `student1` | 403 (*) |
| 7 | Teacher меняет оценку на своём курсе | `teacher1`: `PUT /api/v1/enrollments/1/grade` | 200 |
| 8 | Teacher меняет оценку на чужом курсе | `teacher2`: `PUT /api/v1/enrollments/1/grade` | 403 |
| 9 | Student смотрит чужие оценки | `student2`: `GET /api/v1/students/1/courses` или `GET /api/v1/enrollments/1` | 403 |
| 10 | Admin назначает роль | `admin`: `PUT /api/users/{id}/role` `{"role":"Teacher"}` | 200 |
| 11 | Student назначает себе Admin | `student1`: `PUT /api/users/{свой id}/role` `{"role":"Admin"}` | 403 |
| 12 | v1 и v2 | `GET /api/v1/courses` и `GET /api/v2/courses` | обе 200, разные контракты |

(*) В таблице задания указан 401, но по смыслу пользователь аутентифицирован и лишён прав, поэтому по п. 2.5 возвращается **403 Forbidden**. 401 — только когда личность не установлена.

> ⚠ Не проверяйте сценарий 3 на `student1`/`teacher1` больше 2 раз подряд: после 3 неудач аккаунт блокируется на 5 минут (`ACCOUNT_LOCKED`). Для демонстрации блокировки это как раз нужно.

## Версии API
| | v1 | v2 |
|---|---|---|
| `GET /api/v{n}/courses` | `id, name, description, credits, teacherId, teacherName, createdAt` | `id, name, description, credits, createdAt, teacher {id, fullName, department}, enrolledStudentsCount` |
Бизнес-логика общая (`ICourseService`), различаются контроллер и DTO (`CourseDto` / `CourseV2Dto`). Изменения v2 не затрагивают v1.

## Клиент (`UniversityClient`)
Список курсов и страница курса (переключатель v1/v2). Адрес API — `UniversityApi:BaseUrl` в `appsettings.json`. Обрабатываются: недоступность API (503), таймаут (504), 404/400/401/403/5xx. Страница «CORS демо» делает `fetch` из браузера с origin `http://localhost:5100` — запрос проходит, т.к. адрес в списке CORS.

## Безопасность и логи
- Секреты (`Jwt:Key`, `Seed:Password`) — только в user-secrets/переменных окружения.
- В логи не пишутся пароли и токены (логируются Id/имя пользователя); `EnableSensitiveDataLogging` не включён.
- DTO пользователей не содержат хеш пароля, security stamp и т.п.
