# The Best of Chuck — Coding Exercise

## Overview
This solution fetches jokes from an external API and stores them in SQLite. It demonstrates clean architecture, SOLID principles, resilience (Polly), Azure Functions (timer-trigger), and unit/integration testing.

- Jokes longer than 200 characters are rejected at the domain level.
- Duplicate jokes (by text) are filtered in-app and prevented by a UNIQUE constraint in the database.
- Provider is swappable via the `IJokeApi` abstraction.

## Solution Structure
- `src/Antal.Core` — Domain (`Joke`) and Application (`IJokeFetcher`, `JokeFetcher`, contracts, DI)
- `src/Antal.Infrastructure` — SQLite repository (Dapper), HttpClient-based jokes API, DI, Polly policies
- `src/Antal.Functions` — Azure Functions (timer-trigger) host and configuration
- `src/Antal.ConsoleApp` — Simple console runner to fetch and print jokes
- `src/Antal.UnitTests` — Unit tests (domain and application)
- `src/Antal.IntegrationTests` — Integration tests (SQLite repo and upstream API)

## Prerequisites
- .NET SDK 8.0+
- Optional: Azure Functions Core Tools (for running Functions via `func start`)
- Optional: RapidAPI key (only for the external-API integration test)

## Configuration
- Azure Functions local settings: `src/Antal.Functions/local.settings.json`
  - `Values:JokeFetcherTimerSchedule` — CRON schedule for the timer trigger (default: every 15 minutes)
  - `Values:JokeFetcher:Count` — number of jokes to fetch per run
  - `Values:JokeFetcher:MaxParallelism` — parallel requests to the provider
  - `Values:SqliteConnectionString` or `ConnectionStrings:SqliteConnectionString` — SQLite connection string
  - `Values:JokeApi:BaseAddress` — upstream API base URL
  - `Values:JokeApi:ApiKey` — API key (set if using a provider that requires it)

Note: The console app has an in-memory configuration suitable for quick local runs.

## How to Run

### 1) Console App
From repository root (this folder):

```powershell
# Windows PowerShell
 dotnet run --project src/Antal.ConsoleApp
```

This will:
- Initialize SQLite (creates `jokes.db` in the working directory by default)
- Fetch jokes
- Print fetch metrics and list all jokes from the DB

### 2) Azure Functions (Timer Trigger)
Option A — .NET run:
```powershell
 dotnet run --project src/Antal.Functions
```

Option B — Functions Core Tools:
```powershell
# In directory: src/Antal.Functions
 func start
```

Adjust `JokeFetcherTimerSchedule` in `local.settings.json` for quicker local testing (e.g., every 30s: `"*/30 * * * * *"`).

## Tests
Run all tests:
```powershell
 dotnet test src/Antal.sln
```

- Upstream API integration test requires `RAPIDAPI_KEY`.
  - If not set, the test is automatically skipped.
  - To set for current session:
    - PowerShell: `$env:RAPIDAPI_KEY = "<your_key>"`
    - Bash: `export RAPIDAPI_KEY=<your_key>`

## Extensibility
- To replace the jokes provider, implement `Antal.Core.Application.Abstractions.IJokeApi` and register your implementation in DI.
- Resilience is configured in `Antal.Infrastructure.DependencyInjection` (retry/circuit breaker via Polly).

## Notes
- The database schema is initialized automatically on startup.
- No secrets are committed to the repository; `local.settings.json` contains a placeholder for the API key.
