# Cat Sitter — Web Platform

Web port of the original Flutter mobile app (`catsitter-platform`). Supabase has been
replaced entirely with a self-hosted stack: **PostgreSQL in Docker** + a custom
**ASP.NET Core (.NET 10) API** for auth, data, and file storage.

## Stack

| Layer | Tech |
|---|---|
| Database | PostgreSQL 16, running in Docker |
| Backend API | ASP.NET Core 10 Web API + EF Core (Npgsql), JWT auth, BCrypt password hashing |
| Owner/Sitter web app | Vue 3 + TypeScript + Vite + Tailwind v4 + Pinia (`apps/web`) |
| Admin portal | Vue 3 + TypeScript + Vite + Tailwind v4 (`apps/admin`, moved from the old repo) |
| File storage | Local disk volume served by the API at `/uploads/**` (replaces Supabase Storage) |

Folder layout is **vertical-slice**: `apps/web/src/features/<domain>` bundles each
feature's views, Pinia store and API calls together (`auth`, `cats`, `sitters`, `booking`,
`dashboard`, `profile`), instead of splitting by technical layer.

## 1. Start the database + API (Docker)

```bash
cp .env.example .env   # adjust POSTGRES_PASSWORD / JWT_SECRET before using this anywhere but localhost
docker compose up -d --build
```

This starts three containers:
- **db** — Postgres 16 on `localhost:5432`
- **api** — the .NET API on `localhost:5254` (applies EF Core migrations automatically on boot)
- **adminer** — a DB browser UI on `localhost:8082` (server: `db`, user/pass from `.env`)

Check it's alive: `curl http://localhost:5254/health` → `{"status":"ok"}`

## 2. Run the owner/sitter web app

```bash
cd apps/web
cp .env.example .env   # VITE_API_URL defaults to http://localhost:5254
npm install
npm run dev             # http://localhost:3001
```

## 3. Run the admin portal

```bash
cd apps/admin
cp .env.example .env    # VITE_API_URL defaults to http://localhost:5254
npm install
npm run dev              # http://localhost:3000
```

Admin portal is now rewired to the new API (`api/CatSitter.Api/Controllers/AdminController.cs`)
instead of Supabase — dashboard stats, sitter verification (approve/reject), booking list,
user management (suspend/reinstate), and service-fee settings all work against Postgres.

## Creating an admin account

There's no self-serve admin signup (by design). Register a normal account through the web
app or the API, then promote it via Adminer (`localhost:8082`) or `psql`:

```sql
UPDATE users SET role = 'admin', status = 'approved' WHERE email = 'you@example.com';
```

### Seeded test accounts (password: `password123` for all)

| Role | Email |
|---|---|
| Admin | `admin@test.com` |
| Owner | `owner@test.com` |
| Sitter | `sitter@test.com` (pre-approved & verified, so it shows up in owner search) |

## What's implemented vs. pending

**Working end-to-end** (Vue web app ↔ .NET API ↔ Postgres):
- Registration/login (owner + sitter) with JWT, sitters start `pending` until verified
- Owner: manage cats, search sitters (Smart Matching: rating 40% / GPS proximity 35% /
  review volume 15% / price 10%, ported from the old `SitterRepository`), sitter detail,
  booking wizard (server recomputes price + 10% platform fee), booking history
- Sitter: profile editing (bio, price, GPS location, service photos, socials, availability),
  incoming booking requests (accept/reject), job pipeline (confirmed → in progress → completed)
- Photo uploads (avatar, cat photo, sitter portfolio) via the API's local `/uploads` storage

**Admin portal** (Vue 3, `apps/admin`, ↔ `AdminController`):
- Dashboard KPIs (users, revenue, pending sitters, active/total bookings, recent bookings)
- Sitter verification queue (approve/reject with reason, notifies the sitter)
- All-bookings table with filters, user management (suspend/reinstate), service-fee setting

**Schema exists, API/UI pending** (tables + EF Core entities are already migrated):
- Checklists, attendance check-in/out, reviews, chat, notifications — these tables are in
  the EF Core model (`api/CatSitter.Api/Entities`) and migration, but have no controllers
  or UI yet.

## Repo layout

```
catsitter-web/
├── docker-compose.yml       # db + api + adminer
├── api/CatSitter.Api/       # ASP.NET Core Web API + EF Core migrations
└── apps/
    ├── web/                 # owner/sitter Vue 3 app (vertical-slice features)
    └── admin/               # admin Vue 3 app (moved from catsitter-platform/admin_web)
```
