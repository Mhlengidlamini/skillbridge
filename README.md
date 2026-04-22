# SkillBridge

Clean, professional backend-first starter for the SkillBridge platform:

- `.NET 10` Minimal API
- Swagger/OpenAPI
- PostgreSQL with EF Core
- Ready for Angular frontend integration

## Solution Layout

- `SkillBridge.slnx` - solution file
- `SkillBridge.Api/` - backend API project
- `docker-compose.yml` - local PostgreSQL container

## 1) Start PostgreSQL

From the workspace root:

```bash
docker compose up -d
```

## 2) Run Backend API

```bash
dotnet restore
dotnet run --project SkillBridge.Api
```

Swagger UI:

- [http://localhost:5145/swagger](http://localhost:5145/swagger) (or the URL printed by `dotnet run`)

## 3) Available Minimal API Endpoints

- `GET /api/health`
- `GET /api/users`
- `POST /api/users`
- `GET /api/jobs?activeOnly=true`
- `POST /api/jobs`

## 4) Next Backend Steps

1. Add JWT auth and role-based access.
2. Add migrations (`dotnet ef migrations add InitialCreate`).
3. Expand entities (applications, skills, mentors, notifications).
4. Add OpenAI-powered endpoints as separate modules.
