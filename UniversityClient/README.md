# UniversityClient

Небольшое ASP.NET Core MVC-приложение — клиент для University Course Management API.
Получает данные **только по HTTP** (`IHttpClientFactory`), к базе данных не обращается.

- Адрес API: `appsettings.json` → `UniversityApi:BaseUrl` (по умолчанию `http://localhost:5000/`)
- Страницы: `/Courses` (список, переключатель v1/v2), `/Courses/Details/{id}`, `/Courses/CorsDemo`
- Ошибки: API недоступно (503), таймаут (504), 404/400/401/403/5xx показываются понятным сообщением

```bash
dotnet run        # http://localhost:5100   (сначала запустите API)
```
