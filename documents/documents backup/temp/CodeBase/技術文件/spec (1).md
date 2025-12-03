
# Project Specification — .NET 8 Web API + Vue 3 (TS) Admin Boilerplate (No Pinia / No Test Suites)

> **Goal**: Provide a reusable, production-ready codebase and conventions for building internal/enterprise web apps with unified modules: authentication/authorization, role-based user management, codekind/codename dictionary, sidebar menu tree management (role-aware), session control, logging, and data tables with paging/filtering — **without Pinia** and **without unit test frameworks** to keep dependencies minimal.

---

## 1. Tech Stack & Versions

### Frontend (minimal runtime deps)
- **Vue.js**: 3.2.37+ (TypeScript) with **Vite 6+**
- **Tailwind CSS**: 4.0+
- **Axios**: 1.11.0+
- **Lucide Icons**: 0.540.0+
- **TanStack Table**: 8.21+
- **Vue Router**: 4.x
- Dev tooling: **ESLint** + **Prettier**
- **No Pinia**; **No Vitest/Jest**; **No Testing Library**

### Backend
- **ASP.NET Core**: 8.0 (C# 12) — Web API
- **Microsoft SQL Server**: 2022 Standard
- **Entity Framework Core**: 8.0+ (code-first, migrations)
- **Redis**: 7.2+ (session store, cache, token revocation list, rate limit buckets)
- **log4net**: 2.0.16+
- **xUnit** removed (no unit tests scaffolded)
- Hosting: **IIS**
- IDE: **Visual Studio Code**

---

## 2. High-Level Architecture

```
[Vue3 + Vite + TS]  ──HTTP/JSON──>  [ASP.NET Core 8 Web API]  ──EF Core──> [SQL Server 2022]
        │                                   │
  Lightweight auth/session                  ├── StackExchange.Redis → [Redis 7.2]
        │                                   ├── log4net → Files/Windows Event Log
  Axios (JWT + refresh cookie)              └── (optional) HostedService jobs
```

**Patterns**
- Clean Architecture / Modular Monolith
- Layers: **WebApi**, **Application**, **Domain**, **Infrastructure**

---

## 3. Repository & Solution Layout

```
/src
  /client/ (Vue app)
  /server/
    /WebApi/
    /Application/
    /Domain/
    /Infrastructure/
/tests  (omitted)
```

---

## 4. Database Schema (Core Modules)

*(unchanged from previous spec; includes Users/Auth, Roles/Permissions, Menus, Dictionary, Refresh Tokens, Login Audits, etc.)*

> Keep `created_at/updated_at/is_active` columns and seed data as previously defined.

---

## 5. Authentication, Session, and Authorization

### 5.1 Token & Session Model
- Login returns **Access Token (JWT, 15m)** and rotates a **Refresh Token (14d)** stored in an **HttpOnly, Secure, SameSite=Strict** cookie `rt` (browser-managed).
- Client stores **access token in memory only** (not localStorage) in a minimal **AuthSession** singleton.
- On page reload or startup, client calls `/auth/profile` using refresh flow:
  1) Try `/auth/refresh` (cookie sent automatically), then
  2) Fetch `/auth/profile` to hydrate user/claims/menus.
- Logout: call `/auth/logout`; client clears in-memory token and navigation data.

### 5.2 Lightweight Frontend Auth (no Pinia)
Create `src/session/auth.ts`:
```ts
import { reactive, computed } from "vue"
import axiosBase from "@/utils/axios"

type User = { id: string; username: string; displayName: string; roles: string[]; permissions: string[] }
type AuthState = { accessToken?: string; user?: User; hydrated: boolean }

const state = reactive<AuthState>({ hydrated: false })

export const auth = {
  isAuthenticated: computed(() => !!state.accessToken && !!state.user),
  user: computed(() => state.user),
  token: () => state.accessToken,
  setToken(token?: string) { state.accessToken = token },
  setUser(u?: User) { state.user = u },
  setHydrated(v: boolean) { state.hydrated = v },
  reset() { state.accessToken = undefined; state.user = undefined; state.hydrated = false }
}

// Bootstrapping after app.mount:
export async function bootstrapAuth() {
  try {
    // attempt refresh using HttpOnly cookie
    const { data } = await axiosBase.post("/auth/refresh")
    if (data?.accessToken) auth.setToken(data.accessToken)
    const me = await axiosBase.get<User>("/auth/profile")
    auth.setUser(me.data)
  } catch { /* ignore */ }
  finally { auth.setHydrated(true) }
}
```

### 5.3 Axios Instance with Retry-on-401
`src/utils/axios.ts`:
```ts
import axios from "axios"
import { auth } from "@/session/auth"

const api = axios.create({ baseURL: import.meta.env.VITE_API_BASE, withCredentials: true })

api.interceptors.request.use(cfg => {
  const token = auth.token()
  if (token) cfg.headers.Authorization = `Bearer ${token}`
  cfg.headers["X-Request-Id"] = crypto.randomUUID()
  return cfg
})

let refreshing: Promise<string | undefined> | null = null

async function refreshOnce() {
  try {
    const res = await api.post("/auth/refresh")
    const t = res.data?.accessToken as string | undefined
    if (t) auth.setToken(t)
    return t
  } catch {
    auth.reset()
    return undefined
  }
}

api.interceptors.response.use(
  r => r,
  async err => {
    if (err?.response?.status === 401) {
      if (!refreshing) refreshing = refreshOnce().finally(() => (refreshing = null))
      const t = await refreshing
      if (t) {
        const cfg = err.config
        cfg.headers.Authorization = `Bearer ${t}`
        return api(cfg)
      }
    }
    return Promise.reject(err)
  }
)

export default api
```

### 5.4 Router Guards (no Pinia)
`src/router/index.ts`:
```ts
import { createRouter, createWebHistory } from "vue-router"
import { auth } from "@/session/auth"
import { bootstrapAuth } from "@/session/auth"

const routes = [ /* ... */ ]

const router = createRouter({ history: createWebHistory(), routes })

let bootstrapped = false
router.beforeEach(async (to) => {
  if (!bootstrapped) { bootstrapped = true; await bootstrapAuth() }
  const needAuth = to.meta?.auth !== false
  if (needAuth && !auth.isAuthenticated.value) return { name: "login", query: { r: to.fullPath } }
  const needPerm = to.meta?.perm as string | undefined
  if (needPerm && !auth.user.value?.permissions.includes(needPerm)) return { name: "forbidden" }
})

export default router
```

---

## 6. API Design

- Base URL `/api/v1`, camelCase JSON, error envelope (same as previous version).
- **Paged request/response** contracts remain aligned to TanStack Table.
- Endpoints remain identical (Auth, Users, Roles/Permissions, Menus, Codes, System).

---

## 7. Backend Implementation Notes

### 7.1 ASP.NET Core Setup
- Same as before (Controllers, Swagger dev-only, EF Core, Redis, JWT bearer, dynamic policy provider, CORS, compression, log4net).
- Keep **Refresh rotation** and **access token blacklist** in Redis.
- Expose `/auth/profile` to return current user/roles/permissions.
- Add `/nav/tree` for role-filtered menus (cache per role).

### 7.2 EF Core, Redis, Logging, Security, IIS Deploy
- Same as previous spec (migrations, seeders, rolling logs, correlation ID middleware, HTTPS/HSTS, cookie flags, rate limit).

---

## 8. Frontend Structure (No Pinia)

```
/client
  /src
    /assets
    /components
      /data-table/
      /forms/
      /icons/
      /layout/            # AppShell, Sidebar, Topbar
    /pages
      /auth/Login.vue
      /dashboard/Index.vue
      /system/users/*.vue
      /system/roles/*.vue
      /system/menus/*.vue
      /system/codes/*.vue
    /router/
    /session/            # <- auth.ts lives here (no Pinia)
    /styles/
    /utils/              # axios.ts, formatters
    main.ts
  vite.config.ts
  tailwind.config.js
```

### 8.1 Sidebar Menu (role-aware)
- Fetch once after login: `GET /nav/tree` → store in a simple module `src/session/menu.ts`:
```ts
import { reactive } from "vue"
export const menu = reactive<{ tree: any[] }>({ tree: [] })
```
- Render from `menu.tree`. Clear on logout.

### 8.2 Data Table
- Generic table wrapper that posts **PagedQuery** to API endpoints and binds to TanStack Table.
- Keep table state in route query (no central store).

### 8.3 Login Flow (UI)
1) POST `/auth/login` with `{ username, password }`  
2) On success: set `auth.setToken(accessToken)`, fetch `/auth/profile`, `/nav/tree`, then `router.replace(redirect || "/")`.

---

## 9. Configuration

Unchanged; includes `appsettings.json` for server and `.env` for client with `VITE_API_BASE` and app title.

---

## 10. Migrations & Seed Data

- `dotnet ef migrations add Initial`
- `dotnet ef database update`
- Seed: `ADMIN` role, `admin/change_me!`, base permissions, base menus, dictionary kinds.

---

## 11. Non-Functional Requirements

- Stateless Web API; Redis-backed token revocation enables multi-instance.
- Performance: DB indexes + pagination.
- Observability: log4net, trace IDs, `/health`.
- i18n-ready (en/zh-TW). Accessibility basics.

---

## 12. Definition of Done (per module)

1. Login/Refresh/Logout working with rotation + revocation
2. Role-aware menu tree rendering
3. RBAC policy checks in backend
4. Dictionary API consumed by selects
5. Session list + force logout from admin
6. Audit & logs recorded for login/CRUD
7. IIS publish verified
8. Users/Roles/Menus/Codes CRUD screens with TanStack Table

---

## 13. Sample Vue Entrypoint (no Pinia)

`src/main.ts`:
```ts
import { createApp } from "vue"
import App from "./App.vue"
import router from "./router"
import "./styles/tailwind.css"

createApp(App).use(router).mount("#app")
```

---

## 14. Notes on Future Additions (optional, still Pinia-free)

- If a tiny store is later preferred, consider **nanostores** or **vue-use** patterns; both optional.
- SSO (OIDC), file storage, background jobs, WebSocket notify, TOTP 2FA can be added without changing the session model.
