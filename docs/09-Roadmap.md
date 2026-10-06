# Roadmap

## Status legend
- ✅ Done
- 🚧 In progress
- ⬜ Not started

## v1 — MVP (current focus)

### Documentation
- ✅ Vision
- ✅ Requirements
- ✅ User Stories
- ✅ Domain Model
- ✅ Database Design
- ✅ Architecture + ADRs
- ✅ API Design
- ✅ Frontend Design
- ✅ Testing Strategy

### Backend
- ✅ Solution setup (WebApi, Repository, Dto, Tests projects)
- ✅ Database schema via EF Core Migrations (Code First)
- ✅ Authentication (register, login, JWT issuing/validation) + unit tests
- 🚧 Listings CRUD endpoints
- ⬜ Search/filter/pagination on listings
- ⬜ Authorization rules (owner-only edit/delete, admin override)
- ⬜ Input validation on request DTOs
- ⬜ Schema hardening migration (column lengths, price precision — see 
  05-Database.md)
- ⬜ User profile endpoints (`GET/PUT /api/users/me`)
- ⬜ Image upload integration with Cloudinary
- ⬜ Integration tests (real PostgreSQL in a disposable container)

### Frontend
- ⬜ Project setup (Vite + React + TypeScript)
- ⬜ Routing structure
- ⬜ Auth flow (register, login, protected routes)
- ⬜ Homepage with listings grid + pagination
- ⬜ Search/filter UI
- ⬜ Listing detail page
- ⬜ Create/edit listing form (with image upload)
- ⬜ User profile page

### Infrastructure
- 🚧 Docker Compose for local development (PostgreSQL ✅; backend + 
  frontend ⬜)
- ⬜ GitHub Actions CI pipeline (lint + build + test on push/PR)
- ⬜ Deployment (backend + DB, frontend)

## v2 — Planned future iterations

- ⬜ Real-time or asynchronous **in-app messaging** between buyers and 
  sellers (deferred from v1 — see 01-Vision.md)
- ⬜ Simulated/mocked payment flow (no real money movement)
- ⬜ Admin moderation dashboard (beyond simple delete — e.g. flagged 
  listings, user management)
- ⬜ Dynamic, admin-managed categories (instead of fixed enum — only if 
  the fixed list proves too limiting)
- ⬜ Token refresh (v1 uses a single access token with a fixed expiry — 
  see ADR-003)

## Explicitly out of scope (no plans to implement)

- Real payment processing
- Real shipping/logistics integration
- Multi-language support

## Notes

This roadmap reflects a case-study project with a 1–2 month timeframe. 
Priority is on shipping a complete, well-tested v1 rather than 
partially implementing v2 features.
