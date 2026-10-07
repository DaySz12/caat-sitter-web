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

## 🚀 รันโปรเจ็คบนเครื่องใหม่ (คำสั่งเดียวด้วย Docker)

ใช้เมื่อ clone โปรเจ็คมาใหม่ หรือย้ายไปเครื่องอื่น — Docker จะสร้างทุกอย่างให้เอง
(ฐานข้อมูล, ตาราง, บัญชีทดสอบ, API, เว็บ และหน้า admin) ไม่ต้องลง .NET / Node / Postgres เอง

**สิ่งที่ต้องมี:** [Git](https://git-scm.com/) และ [Docker Desktop](https://www.docker.com/products/docker-desktop/) (เปิดโปรแกรมไว้ก่อนรันคำสั่ง)

```bash
git clone https://github.com/DaySz12/FF.git catsitter-web
cd catsitter-web
cp .env.example .env          # Windows PowerShell: copy .env.example .env
docker compose up -d --build  # ครั้งแรกใช้เวลาหลายนาที (โหลด image + build)
```

เสร็จแล้วเปิดได้เลย:

| อะไร | URL |
|---|---|
| เว็บเจ้าของแมว / พี่เลี้ยง | http://localhost:3001 |
| หน้า Admin | http://localhost:3000 |
| API | http://localhost:5254 (เช็ค `http://localhost:5254/health` → `{"status":"ok"}`) |
| Adminer (ดูฐานข้อมูล) | http://localhost:8082 — server: `db`, user/pass ตาม `.env` |

ล็อกอินด้วยบัญชีทดสอบ (รหัสผ่าน `password123`): `owner@test.com`, `sitter@test.com`, `admin@test.com`
— ระบบสร้างให้อัตโนมัติเมื่อฐานข้อมูลยังว่าง (ปิดได้ด้วย `SEED_DEMO_DATA=false` ใน `.env`)

**คำสั่งที่ใช้บ่อย**

```bash
docker compose ps                 # ดูสถานะ container
docker compose logs -f api        # ดู log ของ API
docker compose up -d --build      # หลังแก้โค้ดหรือ git pull — build ใหม่
docker compose down               # หยุดทั้งหมด (ข้อมูลยังอยู่)
docker compose down -v            # หยุด + ลบข้อมูลทั้งหมด (DB + รูปที่อัปโหลด) เริ่มใหม่จากศูนย์
```

**หมายเหตุ**
- ถ้า port ชนกับโปรแกรมอื่น แก้ `WEB_PORT`, `ADMIN_PORT`, `API_PORT`, `POSTGRES_PORT`, `ADMINER_PORT` ใน `.env`
- ถ้าเปลี่ยน port ของ API หรือเปิดผ่าน URL อื่น (เช่น Cloudflare tunnel) ให้แก้ `PUBLIC_API_URL`
  แล้วรัน `docker compose up -d --build` ใหม่ (URL นี้ถูกฝังตอน build เว็บ) และเพิ่ม URL ของเว็บใน `CORS_ORIGINS`
- ก่อนเปิดให้คนอื่นใช้จริง ให้เปลี่ยน `POSTGRES_PASSWORD` และ `JWT_SECRET` ใน `.env`

## Local development (hot reload, optional)

For working on the frontends with hot reload, keep `db` + `api` in Docker and run Vite locally:

```bash
docker compose up -d db api adminer

cd apps/web     # or apps/admin (port 3000)
cp .env.example .env   # VITE_API_URL defaults to http://localhost:5254
npm install
npm run dev            # http://localhost:3001
```

Stop the Docker `web` / `admin` containers first (`docker compose stop web admin`) if they hold ports 3001 / 3000.

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

Created automatically by `api/CatSitter.Api/Services/DemoSeeder.cs` on an empty database.

| Role | Email |
|---|---|
| Admin | `admin@test.com` |
| Owner | `owner@test.com` |
| Sitter | `sitter@test.com` (pre-approved & verified, so it shows up in owner search) |
| Owner | `owner2@test.com` (sample reviewer) |
| Sitter | `sitter2@test.com`, `sitter3@test.com`, `sitter4@test.com` (sample sitters with GPS + prices) |

The seeder also gives these accounts sample content — cats, bookings in every status
(including one in progress today with a care log), reviews, chats and notifications — so
every page has something to show. It runs once, while `owner@test.com` and `sitter@test.com`
have no cats or bookings, and only touches the accounts above.

## What's implemented vs. pending

**Working end-to-end** (Vue web app ↔ .NET API ↔ Postgres):
- Registration/login (owner + sitter) with JWT, sitters start `pending` until verified
- Owner: manage cats, search sitters (Smart Matching: rating 40% / GPS proximity 35% /
  review volume 15% / price 10%, ported from the old `SitterRepository`), sitter detail,
  booking wizard (server recomputes price + 10% platform fee), booking history
- Sitter: profile editing (bio, price, GPS location, service photos, socials, availability),
  incoming booking requests (accept/reject), job pipeline (confirmed → in progress → completed)
- Photo uploads (avatar, cat photo, sitter portfolio) via the API's local `/uploads` storage
- Booking detail page for both sides (cats' diet/health notes, price breakdown, actions)
- Daily care log (`CareController`): sitter check-in/check-out with optional GPS, photo and note,
  an auto-created daily checklist with per-task photos; the owner sees it live and gets notified
- Reviews (`ReviewsController`): owners rate completed bookings (with up to 4 photos); this keeps
  the sitter's `RatingAvg` / `ReviewCount` current, which Smart Matching ranks on
- Chat (`ChatController`): one room per owner/sitter pair, text + images
- Notifications page, pop-up toasts and sidebar badges (unread notifications/messages, pending
  requests), backed by `GET /api/users/me/summary`
- Live updates over Server-Sent Events (`GET /api/events`, `LiveEvents`): new notifications, chat
  messages and care-log changes are pushed to the browser as they happen. Pages keep a slow poll
  as a fallback. The event fan-out is in-memory, so it assumes a single API instance.

**Admin portal** (Vue 3, `apps/admin`, ↔ `AdminController`):
- Dashboard KPIs (users, revenue, pending sitters, active/total bookings, recent bookings)
- Sitter verification queue (approve/reject with reason, notifies the sitter)
- All-bookings table with filters, user management (suspend/reinstate), service-fee setting

**Pending:**
- Real payments — `PaymentStatus` is never updated by the API, and `WalletBalance` is never
  credited. Sitter earnings shown in the app are computed from completed bookings' base price.

## Repo layout

```
catsitter-web/
├── docker-compose.yml       # db + api + web + admin + adminer
├── api/CatSitter.Api/       # ASP.NET Core Web API + EF Core migrations
└── apps/
    ├── web/                 # owner/sitter Vue 3 app (vertical-slice features)
    └── admin/               # admin Vue 3 app (moved from catsitter-platform/admin_web)
```
