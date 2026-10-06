# SoundRevival

A vertical marketplace for buying and selling used musical instruments 
between private users and small shops.

> 🚧 **Work in progress** — portfolio case study. Backend authentication 
> is complete; listings CRUD is under development. See 
> [docs/STATUS.md](docs/STATUS.md) for the current state.

## 🎯 What It Does

Generic marketplaces bury instrument listings among thousands of 
unrelated categories. SoundRevival focuses only on instruments, with 
categories and filters designed for this domain.

- User registration and JWT authentication ✅
- Creating, editing and deleting listings, marking them as sold 🚧
- Searching and filtering by category, keyword and price, with pagination ⬜
- Up to 5 photos per listing, stored on Cloudinary ⬜
- Admin moderation of listings ⬜

## 🛠️ Tech Stack

| Layer | Technologies |
|---|---|
| Backend | C# · ASP.NET Core Web API (.NET 10) · Entity Framework Core (Code First) |
| Database | PostgreSQL 16 (Docker) |
| Auth | JWT · BCrypt |
| Testing | xUnit · Moq |
| Frontend *(planned)* | React · Vite · TypeScript · React Query · Axios |
| Infra *(planned)* | Docker Compose · GitHub Actions · Cloudinary |

The backend is a multi-project solution with physically separated layers 
(`WebApi` → `Repository` → `Dto`), repository + service pattern and 
interface-based dependency injection. See 
[docs/06-Architecture.md](docs/06-Architecture.md).

## 🚀 Running Locally

**Prerequisites**: .NET 10 SDK, Docker Desktop, `dotnet-ef` 
(`dotnet tool install --global dotnet-ef`).

```bash
git clone https://github.com/nilde790/sound-revival.git
cd sound-revival

# Start PostgreSQL
docker compose up -d
```

Create `backend/SoundRevival.WebApi/appsettings.Development.json` 
(gitignored):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=soundrevival;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "<random secret, at least 32 characters>",
    "Issuer": "SoundRevival",
    "Audience": "SoundRevivalUsers",
    "ExpiryMinutes": 60
  }
}
```

```bash
cd backend

# Apply migrations
dotnet ef database update --project SoundRevival.Repository --startup-project SoundRevival.WebApi

# Run the API (http://localhost:5150)
dotnet run --project SoundRevival.WebApi

# Run tests
dotnet test SoundRevival.slnx
```

Sample requests are in `backend/SoundRevival.WebApi/SoundRevival.WebApi.http`.

## 📁 Documentation

The project was designed docs-first. Start from 
[docs/00-README.md](docs/00-README.md): vision, requirements, domain 
model, database, architecture, API contracts, frontend design, roadmap, 
testing strategy, and Architecture Decision Records in 
[docs/adr/](docs/adr/).

## 🎓 Project Goals

This project was built to practice the full lifecycle of a software 
project — from requirements and architecture to implementation, testing 
and deployment — with professional practices: ADRs, feature branches, 
pull requests, layered architecture, and automated tests.
