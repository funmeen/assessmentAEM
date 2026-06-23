# PlatformWellSync — Technical Assessment Solution

## Overview
A .NET 8 Console application that:
1. Authenticates against the testDemo REST API
2. Fetches platform/well data using the bearer token
3. Upserts (insert or update) the data into a SQL Server LocalDB
4. Is resilient to missing or extra API response fields

---

## Project Structure

```
PlatformWellSync/
├── Models/
│   ├── Platform.cs              # Platform entity
│   └── Well.cs                  # Well entity
├── Data/
│   └── AppDbContext.cs          # EF Core DbContext (Code First)
├── DTOs/
│   └── ApiDtos.cs               # API request/response DTOs
├── Services/
│   ├── ApiService.cs            # Handles login + API calls
│   └── SyncService.cs           # Handles DB upsert logic
├── Migrations/
│   ├── 20240101000000_InitialCreate.cs
│   └── AppDbContextModelSnapshot.cs
├── Program.cs                   # Entry point + DI configuration
├── PlatformWellSync.csproj
└── Part2_LastUpdatedWellPerPlatform.sql   # PART 2 SQL query
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server LocalDB (included with Visual Studio, or install separately)
- Internet access to reach `http://test-demo.aemenersol.com`

---

## How to Run

### 1. Clone / open the project

```bash
git clone <your-repo-url>
cd PlatformWellSync
```

### 2. Restore packages

```bash
dotnet restore
```

### 3. Apply EF migrations (creates the LocalDB automatically)

```bash
dotnet ef database update
```

Or the app will auto-migrate on startup via `db.Database.MigrateAsync()`.

### 4. Run the application

```bash
dotnet run
```

The app will:
- Log in as `user@aemenersol.com`
- Fetch data from `/api/PlatformWell/GetPlatformWellActual`
- Upsert Platform and Well records into LocalDB
- Fetch data from `/api/PlatformWell/GetPlatformWellDummy` (resilience test)
- Upsert again — handles missing/extra keys without breaking

---

## Database Schema (Code First)

### Platform Table
| Column       | Type          | Notes                  |
|--------------|---------------|------------------------|
| Id           | int (PK)      | From API `platformId`  |
| PlatformName | nvarchar(200) | Nullable               |

### Well Table
| Column     | Type          | Notes                    |
|------------|---------------|--------------------------|
| Id         | int (PK)      | From API `id`            |
| PlatformId | int (FK)      | References Platform(Id)  |
| UniqueName | nvarchar(200) | Nullable                 |
| Latitude   | float         | Nullable                 |
| Longitude  | float         | Nullable                 |
| CreatedAt  | datetime2     | Nullable                 |
| UpdatedAt  | datetime2     | Nullable                 |

---

## Resilience to Missing / Extra Fields

- All DTO properties are **nullable** — missing JSON keys won't throw
- `[JsonExtensionData]` captures any **new/unknown keys** silently
- `MissingMemberHandling.Ignore` in `JsonSerializerSettings` prevents crashes
- SyncService uses null-coalescing (`??`) to keep existing values when keys are absent

---

## Part 2: SQL Query

See `Part2_LastUpdatedWellPerPlatform.sql`.

Two approaches provided:
1. **JOIN + subquery** — clean and readable
2. **ROW_NUMBER() OVER (PARTITION BY)** — handles ties, more robust

```sql
SELECT p.PlatformName, w.Id, w.PlatformId, w.UniqueName,
       w.Latitude, w.Longitude, w.CreatedAt, w.UpdatedAt
FROM Well w
INNER JOIN Platform p ON p.Id = w.PlatformId
INNER JOIN (
    SELECT PlatformId, MAX(UpdatedAt) AS LastUpdatedAt
    FROM Well
    GROUP BY PlatformId
) latest ON w.PlatformId = latest.PlatformId
        AND w.UpdatedAt = latest.LastUpdatedAt
ORDER BY p.PlatformName;
```

---

## Time Taken

**Estimated: ~3–4 hours**

Breakdown:
- ~30 min: Exploring the API via Swagger, understanding data shape
- ~45 min: Project setup, EF Code First models, migrations
- ~60 min: ApiService (login flow, bearer token, resilient deserialization)
- ~45 min: SyncService (upsert logic, null-safety, logging)
- ~30 min: Program.cs wiring, DI setup, testing flow
- ~20 min: Part 2 SQL query + README

---

## Notes

- Code First EF approach used — no manual SQL schema required
- `ValueGeneratedNever()` is used so EF doesn't try to auto-generate IDs that come from the API
- The app is a Console project for simplicity, but the same Services/Data layer works in Web API or Worker Service
