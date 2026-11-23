# Frontend Design

## Folder structure — feature-based organization

Instead of grouping files by type (`components/`, `hooks/`, `services/`), 
the frontend is organized by **feature**: everything related to a 
specific domain concept lives together. This scales better as the 
project grows, since working on "listings" doesn't require jumping 
between multiple top-level folders.

```
frontend/src/
│
├── features/
│   ├── auth/
│   │   ├── components/       # LoginForm, RegisterForm
│   │   ├── hooks/            # useAuth, useLogin
│   │   ├── api/              # auth-related Axios calls
│   │   └── types.ts
│   │
│   ├── listings/
│   │   ├── components/       # ListingCard, ListingForm, ListingDetail
│   │   ├── hooks/            # useListings, useListing, useCreateListing
│   │   ├── api/               # listings-related Axios calls
│   │   └── types.ts
│   │
│   └── users/
│       ├── components/       # ProfileForm
│       ├── hooks/            # useProfile
│       ├── api/
│       └── types.ts
│
├── shared/
│   ├── components/           # Button, Input, Navbar, Pagination —
│   │                          # generic, reused across features
│   ├── hooks/                # generic hooks not tied to one feature
│   └── lib/                  # axios instance setup, React Query config
│
├── pages/                    # top-level route components, composing
│   ├── HomePage.tsx           # features together (thin, mostly layout)
│   ├── ListingDetailPage.tsx
│   ├── CreateListingPage.tsx
│   ├── EditListingPage.tsx
│   ├── LoginPage.tsx
│   ├── RegisterPage.tsx
│   └── ProfilePage.tsx
│
├── routes/
│   └── AppRoutes.tsx          # React Router route definitions
│
└── App.tsx
```

## Routing

| Path                  | Page                  | Auth required |
|------------------------|------------------------|----------------|
| `/`                    | HomePage               | No             |
| `/listings/:id`        | ListingDetailPage      | No             |
| `/listings/new`        | CreateListingPage      | Yes            |
| `/listings/:id/edit`   | EditListingPage        | Yes (owner or admin) |
| `/login`               | LoginPage              | No             |
| `/register`            | RegisterPage           | No             |
| `/profile`             | ProfilePage            | Yes            |

Protected routes (requiring auth) are wrapped in a `ProtectedRoute` 
component that checks authentication state and redirects to `/login` 
if the user isn't logged in.

## State management

- **Server state** (data fetched from the API: listings, user profile): 
  managed via **React Query**, handling caching, loading/error states, 
  and automatic refetching after mutations (e.g. after creating a listing)
- **Auth state** (current user, token): managed via **React Context** 
  (`AuthContext`), providing `useAuth()` across the app — kept separate 
  from React Query since it's app-wide session state, not server data 
  to be cached/refetched
- **Local UI state** (form inputs, filters before submission, modals): 
  handled with plain `useState` inside individual components

## API communication

- **Axios** instance configured once in `shared/lib/axios.ts`, with an 
  interceptor that automatically attaches the JWT token (from 
  `AuthContext`) to every outgoing request's `Authorization` header
- Each feature's `api/` folder contains typed functions wrapping Axios 
  calls (e.g. `getListings()`, `createListing()`), consumed by that 
  feature's React Query hooks

## Key components (high level)

- **ListingCard**: compact preview shown in listing grids (homepage, 
  search results)
- **ListingForm**: shared between create and edit listing flows
- **ListingDetail**: shows the seller's email only when the API returns 
  it (authenticated requests, FR-15); otherwise shows an invitation to 
  log in to contact the seller
- **ListingFilters**: keyword, category, condition, brand, price range 
  and sort (`newest` / `price_asc` / `price_desc`), kept in the URL query 
  string so a filtered search can be shared and survives a page reload
- **Pagination**: generic component driving `page`/`pageSize` params 
  against `GET /api/listings`
- **ProtectedRoute**: wraps pages requiring authentication

## Related documents

- `06-Architecture.md` — overall stack decisions (React, Vite, Axios, 
  React Query)
- `07-API-Design.md` — endpoint contracts consumed by the `api/` folders