# Database Design

## Engine

**PostgreSQL 16** — chosen for its strong ecosystem support and wide 
adoption in the industry (the most used database in the Stack Overflow 
Developer Survey 2025 and 2026). Runs locally via `docker-compose.yml`.

Hosting for the deployed database is not chosen yet: it will be an ADR 
when deployment starts. Free tiers change often (as of late 2026, 
Render's free PostgreSQL expires after 30 days and Railway only offers 
monthly credits; Neon and Supabase offer non-expiring free tiers with 
storage limits), so check current terms at that time.

## Image storage strategy

Images are **not stored in the database or on the application server**. 
They are uploaded to an **external storage service**, which returns a 
URL. Only this URL is persisted in the `Images` table. This avoids 
relying on ephemeral filesystem storage, which many hosting platforms 
wipe on redeploy.

The provider (e.g. Cloudinary or the deployment cloud's blob storage) is 
**not chosen yet**: it is an ADR at the start of phase 3 (see 
09-Roadmap.md). Deleting a listing removes its `Images` rows by cascade, 
but not the stored files: the image feature must delete them explicitly.

## Schema

The schema is generated **Code First** by EF Core from the entity classes 
in `SoundRevival.Repository/Entities/` and the fluent configuration in 
`AppDbContext.OnModelCreating`. EF Core's default naming is kept: table 
and column names are **PascalCase** (e.g. `"Listings"."UserId"`), so 
they must be double-quoted in raw SQL.

The tables below reflect the schema as currently migrated 
(`InitialCreate`). Defaults are set **by the application** (C# property 
initializers), not by the database.

### `Users`

| Column       | Type                     | Constraints / notes              |
|--------------|--------------------------|----------------------------------|
| Id           | uuid                     | PRIMARY KEY                      |
| Email        | text                     | NOT NULL, UNIQUE index           |
| PasswordHash | text                     | NOT NULL (BCrypt hash)           |
| DisplayName  | text                     | NOT NULL                         |
| Role         | text                     | NOT NULL, app default `user`     |
| CreatedAt    | timestamp with time zone | NOT NULL, app default UTC now    |

### `Listings`

| Column      | Type                     | Constraints / notes                          |
|-------------|--------------------------|----------------------------------------------|
| Id          | uuid                     | PRIMARY KEY                                  |
| UserId      | uuid                     | NOT NULL, FK → `Users.Id` (cascade), indexed |
| Title       | text                     | NOT NULL                                     |
| Description | text                     | NOT NULL                                     |
| Price       | numeric                  | NOT NULL                                     |
| Category    | text                     | NOT NULL, indexed (validated at app level)   |
| Condition   | text                     | NOT NULL (validated at app level)            |
| Status      | text                     | NOT NULL, indexed, app default `available`   |
| CreatedAt   | timestamp with time zone | NOT NULL, app default UTC now                |
| UpdatedAt   | timestamp with time zone | NOT NULL, app default UTC now                |

### `Images`

| Column       | Type     | Constraints / notes                             |
|--------------|----------|-------------------------------------------------|
| Id           | uuid     | PRIMARY KEY                                     |
| ListingId    | uuid     | NOT NULL, FK → `Listings.Id` (cascade), indexed |
| Url          | text     | NOT NULL (URL from the image storage)           |
| DisplayOrder | smallint | NOT NULL                                        |

## Relationships (foreign keys)

- `Listings.UserId` → `Users.Id` (one user, many listings)
- `Images.ListingId` → `Listings.Id` (one listing, up to 5 images)

Both foreign keys use `ON DELETE CASCADE`: if a user is deleted, their 
listings are deleted too; if a listing is deleted, its images are 
deleted too. This keeps the database consistent without orphaned rows.

## Indexes

- `Users.Email` — unique (login lookups + uniqueness constraint)
- `Listings.Category` — category-filtered searches
- `Listings.Status` — default "available only" queries
- `Listings.UserId`, `Images.ListingId` — created automatically by EF 
  Core for foreign keys

## Planned schema changes (not yet migrated)

The original design specified tighter column constraints than the 
current migration enforces, and the domain gained a `Brand` attribute. 
Both are the target, to be added via fluent API (`HasMaxLength`, 
`HasPrecision`) and entity changes, ideally in one dedicated migration 
(technical debt S1):

| Column                 | Target                          |
|------------------------|---------------------------------|
| `Users.Email`          | varchar(255), stored normalized (lowercase) so the unique index is case-insensitive in practice |
| `Users.DisplayName`    | varchar(100)                    |
| `Users.Role`           | varchar(20)                     |
| `Listings.Title`       | varchar(150)                    |
| `Listings.Description` | varchar(5000)                   |
| `Listings.Price`       | numeric(10,2)                   |
| `Listings.Category`    | varchar(30)                     |
| `Listings.Condition`   | varchar(20)                     |
| `Listings.Status`      | varchar(20)                     |
| `Listings.Brand`       | **new column**, varchar(60), NULL allowed |
| `Images.Url`           | varchar(500)                    |

Until then, length and precision limits must be enforced by DTO 
validation (see the field constraints in 07-API-Design.md).

Indexes to reconsider with the sort options of 07-API-Design.md: 
`Listings.CreatedAt` (default sort, newest first). No index is planned 
for `Brand` or text search: partial matches (`ILIKE '%x%'`) can't use a 
B-tree index, and at the target volume (NFR-4: 1,000 listings) a scan is 
fast enough. Measure first (rule DATA-6).

## Design notes

- **UUID as primary key type** for all tables, instead of 
  auto-incrementing integers (SERIAL). This avoids exposing sequential 
  information through IDs (e.g. a URL like `/listings/47` would reveal 
  the approximate number of listings created) and is consistent with 
  patterns used in modern, potentially distributed systems. IDs are 
  generated in the service layer with `Guid.NewGuid()` (see ADR-002).

- **Category, condition, status and role are plain text fields**, 
  validated at the application layer against the fixed lists defined in 
  04-DomainModel.md, rather than separate lookup tables — consistent 
  with the domain model decision to keep them as enums, not entities.
  
- **Enforcing max 5 images per listing** is a business rule, not easily 
  expressed as a pure database constraint — it is validated in the 
  service layer (see 07-API-Design.md).
