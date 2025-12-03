
# Project Specification — .NET 8 Web API + Vue 3 (TS) Admin Boilerplate

> **Goal**: Provide a reusable, production‑ready codebase and conventions for building internal/enterprise web apps with unified modules: authentication/authorization, role‑based user management, codekind/codename dictionary, sidebar menu tree management (role‑aware), session control, logging, and data tables with paging/filtering.

---

## 1. Tech Stack & Versions

### Frontend
- **Vue.js**: 3.2.37+ (TypeScript) with **Vite 6+**
- **Tailwind CSS**: 4.0+ (JIT)
- **Axios**: 1.11.0+ (HTTP client, interceptors for JWT & refresh)
- **Lucide Icons**: 0.540.0+
- **TanStack Table**: 8.21+ (client table model, server‑side paging/filter/sort)
- **Pinia** for state management
- **Vue Router** for routing
- **Vitest** + **Testing Library** for unit tests
- **ESLint** + **Prettier** for formatting

### Backend
- **ASP.NET Core**: 8.0 (C# 12) — Web API
- **Microsoft SQL Server**: 2022 Standard
- **Entity Framework Core**: 8.0+ (code‑first, migrations)
- **Redis**: 7.2+ (session store, cache, token revocation list, rate limit buckets)
- **log4net**: 2.0.16+ (via Microsoft.Extensions.Logging.Log4Net.AspNetCore)
- **xUnit** + **FluentAssertions** for tests
- Hosting: **IIS** (Windows), behind HTTPS
- IDE: **Visual Studio Code**

---

## 2. High‑Level Architecture

```
[Vue3 + Vite + TS]  ──HTTP/JSON──>  [ASP.NET Core 8 Web API]  ──EF Core──> [SQL Server 2022]
        │                                   │
   Pinia/Router                             ├── StackExchange.Redis → [Redis 7.2]
        │                                   ├── log4net → Files/Windows Event Log
  Axios (JWT)                               └── Background jobs (HostedService, optional)
```

**Patterns**
- Clean Architecture / Modular Monolith
- Separation of layers: **WebApi** (presentation), **Application** (use cases), **Domain** (entities), **Infrastructure** (EF, Redis, log4net, external services)

---

## 3. Repository & Solution Layout

```
/src
  /client/ (Vue app)
  /server/
    /WebApi/                 # ASP.NET Core host (controllers, DI, filters, auth)
    /Application/            # CQRS handlers, validators, DTOs
    /Domain/                 # Entities, value objects, domain events
    /Infrastructure/         # EF Core, Redis, logging, email/SMS, file store
/tests
  /client/                   # Vitest
  /server/                   # xUnit
```

---

## 4. Database Schema (Core Modules)

> Naming convention: singular table names, snake_case columns, `created_at/created_by/updated_at/updated_by/is_active` on all business tables.

### 4.1 Users & Auth

```sql
CREATE TABLE users (
  id               UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
  username         NVARCHAR(64)  NOT NULL UNIQUE,
  email            NVARCHAR(256) NOT NULL UNIQUE,
  phone            NVARCHAR(32)  NULL,
  password_hash    VARBINARY(MAX) NOT NULL,
  password_salt    VARBINARY(64)  NOT NULL,
  display_name     NVARCHAR(128) NOT NULL,
  is_locked        BIT NOT NULL DEFAULT 0,
  last_login_at    DATETIME2 NULL,
  created_at       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  created_by       UNIQUEIDENTIFIER NULL,
  updated_at       DATETIME2 NULL,
  updated_by       UNIQUEIDENTIFIER NULL,
  is_active        BIT NOT NULL DEFAULT 1
);

CREATE TABLE roles (
  id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
  code         NVARCHAR(64) NOT NULL UNIQUE,
  name         NVARCHAR(128) NOT NULL,
  description  NVARCHAR(512) NULL,
  is_system    BIT NOT NULL DEFAULT 0,
  created_at   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  updated_at   DATETIME2 NULL,
  is_active    BIT NOT NULL DEFAULT 1
);

CREATE TABLE permissions (
  id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
  key          NVARCHAR(128) NOT NULL UNIQUE,  -- e.g. "user.create", "menu.read"
  name         NVARCHAR(128) NOT NULL,
  description  NVARCHAR(512) NULL
);

CREATE TABLE role_permissions (
  role_id        UNIQUEIDENTIFIER NOT NULL,
  permission_id  UNIQUEIDENTIFIER NOT NULL,
  PRIMARY KEY (role_id, permission_id),
  FOREIGN KEY (role_id) REFERENCES roles(id),
  FOREIGN KEY (permission_id) REFERENCES permissions(id)
);

CREATE TABLE user_roles (
  user_id  UNIQUEIDENTIFIER NOT NULL,
  role_id  UNIQUEIDENTIFIER NOT NULL,
  PRIMARY KEY (user_id, role_id),
  FOREIGN KEY (user_id) REFERENCES users(id),
  FOREIGN KEY (role_id) REFERENCES roles(id)
);

CREATE TABLE refresh_tokens (
  id            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
  user_id       UNIQUEIDENTIFIER NOT NULL REFERENCES users(id),
  token         CHAR(128) NOT NULL UNIQUE,
  jwt_id        UNIQUEIDENTIFIER NOT NULL,    -- jti of access token
  issued_at     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  expires_at    DATETIME2 NOT NULL,
  revoked_at    DATETIME2 NULL,
  replaced_by   CHAR(128) NULL
);

CREATE TABLE login_audits (
  id            BIGINT IDENTITY PRIMARY KEY,
  user_id       UNIQUEIDENTIFIER NULL,
  username      NVARCHAR(64) NULL,
  ip_address    NVARCHAR(45) NULL,
  user_agent    NVARCHAR(256) NULL,
  succeeded     BIT NOT NULL,
  message       NVARCHAR(256) NULL,
  occurred_at   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

### 4.2 Sidebar Menu & Navigation (Role‑aware)

```sql
CREATE TABLE menus (
  id            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
  parent_id     UNIQUEIDENTIFIER NULL REFERENCES menus(id),
  code          NVARCHAR(64) NOT NULL UNIQUE,     -- e.g. "sys.users"
  name          NVARCHAR(128) NOT NULL,
  route_path    NVARCHAR(256) NULL,               -- e.g. "/system/users"
  icon          NVARCHAR(64) NULL,                -- lucide icon name
  order_no      INT NOT NULL DEFAULT 0,
  is_show       BIT NOT NULL DEFAULT 1
);

CREATE TABLE role_menus (
  role_id  UNIQUEIDENTIFIER NOT NULL REFERENCES roles(id),
  menu_id  UNIQUEIDENTIFIER NOT NULL REFERENCES menus(id),
  PRIMARY KEY (role_id, menu_id)
);
```

### 4.3 Unified CodeKind/CodeName Dictionary

```sql
CREATE TABLE code_kinds (
  id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
  kind        NVARCHAR(64) NOT NULL UNIQUE,   -- e.g. "GENDER", "STATUS"
  name        NVARCHAR(128) NOT NULL,
  description NVARCHAR(256) NULL,
  is_system   BIT NOT NULL DEFAULT 0
);

CREATE TABLE codes (
  id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
  kind_id     UNIQUEIDENTIFIER NOT NULL REFERENCES code_kinds(id),
  code        NVARCHAR(64) NOT NULL,
  name        NVARCHAR(128) NOT NULL,
  sort_no     INT NOT NULL DEFAULT 0,
  extra_json  NVARCHAR(MAX) NULL,             -- optional extensibility
  is_active   BIT NOT NULL DEFAULT 1,
  UNIQUE (kind_id, code)
);
```

### 4.4 Session & Cache (Redis)

Logical keys:
- `sess:{sessionId}` → userId, claims, createdAt, expiresAt
- `jwt:blacklist:{jti}` → boolean (revoked)
- `rt:{token}` → state (revoked/replaced/active) *(duplicated from DB for quick checks)*
- `menu:tree:{roleId}` → cached menu DTO tree for role
- `codes:{kind}` → list of codes

---

## 5. Authentication & Authorization

### 5.1 Flow
- Username/Password login → JWT **access token** (short TTL, 15m) + **refresh token** (long TTL, 7–30d).
- Access token contains `sub`, `jti`, `exp`, and **role**/**permission** claims.
- Refresh token rotation: each refresh invalidates the previous token; store chain in DB & Redis.
- Logout: revoke current **access jti** (blacklist in Redis with remaining TTL) and mark refresh token revoked.

### 5.2 Token Transport
- Access token: `Authorization: Bearer <token>`
- Option A (default): store refresh token in **HttpOnly, Secure, SameSite=Strict** cookie `rt`.
- Option B: store refresh token in DB and send via header for non‑browser clients.

### 5.3 Password Policy & Hashing
- PBKDF2 (ASP.NET Core Identity implementation) or Argon2id (libsodium) — min 10k iterations if PBKDF2.
- Enforce complexity (min length 12, mixed chars) and lockout after N failures (configurable).

### 5.4 Authorization Model
- Role‑based + permission claims.
- Controllers marked with `[Authorize(Policy = "perm:user.read")]` etc.
- Policy provider maps `perm:*` to presence of claim; dynamic registration from DB at startup.

---

## 6. API Design

### 6.1 Common Conventions
- Base URL: `/api/v1`
- JSON case: `camelCase`
- Error envelope:
```json
{ "error": { "code": "ValidationError", "message": "...", "details": {...}, "traceId": "..." } }
```
- Paged list request (compatible with TanStack Table server mode):
```json
{
  "page": 1,
  "pageSize": 20,
  "sort": [{"id": "createdAt", "desc": true}],      // multiple sorts
  "filters": [{"id": "username", "value": "john"}]  // columnId + value
}
```
- Paged list response:
```json
{
  "items": [ ... ],
  "page": 1,
  "pageSize": 20,
  "total": 345
}
```

### 6.2 Endpoints

**Auth**
- `POST /auth/login` → { accessToken, expiresIn } (+ `rt` cookie set)  
  body: { username, password }
- `POST /auth/refresh` → rotate and return new { accessToken } (+ set new `rt`)
- `POST /auth/logout` → revoke current tokens
- `GET /auth/profile` → current user info + permissions
- `POST /auth/change-password`
- `POST /auth/forgot-password` (optional, email/SMS integration)

**Users**
- `GET /users` (paged/filter/sort)
- `GET /users/{id}`
- `POST /users` (create; default password policy)
- `PUT /users/{id}`
- `DELETE /users/{id}` (soft delete via `is_active` = 0)
- `PUT /users/{id}/lock` / `unlock`
- `GET /users/{id}/roles`
- `PUT /users/{id}/roles` (set roles)
- `GET /users/{id}/sessions` (active sessions from Redis)
- `DELETE /users/{id}/sessions/{sessionId}` (force logout)

**Roles & Permissions**
- `GET /roles` (paged)
- `POST /roles`
- `PUT /roles/{id}`
- `DELETE /roles/{id}`
- `GET /roles/{id}/permissions`
- `PUT /roles/{id}/permissions` (set permission keys)

**Menus**
- `GET /menus/tree` (for admin) — full CRUD view
- `POST /menus` / `PUT /menus/{id}` / `DELETE /menus/{id}`
- `GET /nav/tree` (current user) — role‑filtered, cached

**Code Dictionary**
- `GET /codes/kinds`
- `POST /codes/kinds`
- `PUT /codes/kinds/{id}`
- `DELETE /codes/kinds/{id}`
- `GET /codes/{kind}` (public cacheable list for dropdowns)
- `POST /codes/{kind}` (add code)
- `PUT /codes/{kind}/{codeId}`
- `DELETE /codes/{kind}/{codeId}`

**System**
- `GET /health` (liveness/readiness for IIS monitoring)
- `GET /version` (git sha, build time)
- `GET /audit/logins` (paged; requires perm)

---

## 7. Backend Implementation Notes

### 7.1 ASP.NET Core Setup
- `Program.cs`:
  - `AddControllers().AddJsonOptions(...)`
  - `AddEndpointsApiExplorer()` + `AddSwaggerGen()` (dev only)
  - `AddDbContext<AppDbContext>(SqlServer)` with resilient retry
  - `AddStackExchangeRedisCache` + `AddDistributedMemoryCache` (fallback)
  - `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` with RSA signing keys (rotate via `Microsoft.IdentityModel.Tokens`)
  - `AddAuthorization(options => RegisterPermissionPoliciesFromDb(options))`
  - `AddCors("default", ... allow specific origins)`
  - `AddResponseCompression()` (Brotli/Gzip)
  - `AddLogging(lb => lb.AddLog4Net("log4net.config"))`
- Filters: global exception filter → wrap error envelope; model validation filter.

### 7.2 EF Core
- Schema per above. Enable `UseQuerySplittingBehavior(SplitQuery)` on large graphs.
- Soft delete via `IsActive` and query filters.
- Add seeders for: admin user, base roles (`ADMIN`, `USER`), base permissions, sample menu tree, and dictionary kinds.

### 7.3 Redis
- Connection string via `appsettings.json` → `Redis:ConnectionString`.
- Implement `ISessionService` with Redis for active session tracking (sessionId ↔ userId, user agent, ip, lastSeen).
- Token blacklist and refresh token state kept with TTLs matching token expiry.

### 7.4 Logging & Audit
- `log4net.config` with rolling file appender (daily, retain 30), and EventLog appender (optional).
- Enrich logs with `CorrelationId` (use middleware to propagate `Trace-Id`).
- Audit tables: `login_audits` and `audit_events` (optional generic table).

### 7.5 Security
- HTTPS only, HSTS on IIS.
- Cookie flags: `Secure`, `HttpOnly`, `SameSite=Strict`.
- CSRF not required for pure API + Bearer; if using cookies for access tokens, enable anti‑forgery tokens.
- Input validation on all endpoints with **FluentValidation**; consistent 400 payloads.
- Rate limit: sliding window in Redis (e.g., 100 req/5m per IP + account).

### 7.6 Deployment to IIS
- Publish profile: `dotnet publish -c Release -o ./publish`
- IIS site: enable **ASP.NET Core Module V2**; set **Environment Variables** (`ASPNETCORE_ENVIRONMENT`, connection strings).
- URL Rewrite for SPA: serve `index.html` and let Vue Router handle client routes.
- Configure **Application Pool** identity for file access; grant SQL login least privileges.
- Log directory write permission for app pool identity.

---

## 8. Frontend Implementation Notes

### 8.1 Project Structure
```
/client
  /src
    /assets
    /components
      /data-table/               # TanStack abstractions
      /forms/
      /icons/                    # Lucide icon wrappers
      /layout/                   # AppShell, Sidebar, Topbar
    /composables                 # useAuth, useAxios, useTableQuery
    /router                      # auth guards
    /stores                      # Pinia (auth, menu, user)
    /pages
      /auth/Login.vue
      /dashboard/Index.vue
      /system/users/*.vue
      /system/roles/*.vue
      /system/menus/*.vue
      /system/codes/*.vue
    /styles                      # Tailwind entry, theme tokens
    /utils                       # formatters, validators
    main.ts
    env.d.ts
  index.html
  vite.config.ts
  tailwind.config.js
```

### 8.2 Auth Guard & Axios
- `useAxios()` creates a singleton with:
  - `Authorization: Bearer <accessToken>`
  - 401 handler → attempt `/auth/refresh` then retry once
  - Attach `X-Request-Id`, locale
- Router guards:
  - If no token → redirect to `/auth/login`
  - If token but lacks permission for route meta → 403 page

### 8.3 Sidebar Menu
- Fetch `/nav/tree` after login.
- Cache in Pinia; watch role change → refresh tree.
- Node structure:
```ts
type MenuNode = { id: string; name: string; routePath?: string; icon?: string; children?: MenuNode[] }
```

### 8.4 Data Table (Server Mode)
- Table wrapper component calls `/users` (etc.) with the **paged request** contract above.
- Preserve table state to query string for deep‑linking.
- Export CSV of current result set (optional).

### 8.5 UI/UX
- Tailwind utility classes; light/dark theme toggle.
- Lucide icons used via `<Lucide name="Users" />` wrapper.
- Form components: Input, Select (dictionary‑backed), DateRange, ConfirmDialog.

---

## 9. Configuration

`appsettings.json` (server):
```json
{
  "ConnectionStrings": {
    "Default": "Server=.;Database=AppDb;User Id=app;Password=***;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "YourCompany.App",
    "Audience": "YourCompany.App",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 14,
    "RsaPrivateKeyXmlPath": "keys/jwt.key.xml"
  },
  "Redis": {
    "ConnectionString": "localhost:6379,abortConnect=false"
  },
  "Cors": {
    "Origins": [ "https://your-frontend.example.com" ]
  },
  "Logging": { "LogLevel": { "Default": "Information" } }
}
```

`.env` (client):
```
VITE_API_BASE=/api/v1
VITE_APP_NAME=Admin Boilerplate
```

---

## 10. Migrations & Seed Data

- `dotnet ef migrations add Initial`
- `dotnet ef database update`
- Seeder creates:
  - **Roles**: `ADMIN`, `USER`
  - **Admin** account: `admin / change_me!` (force change on first login)
  - **Permissions**: user/role/menu/code CRUD
  - **Menus**: System → Users, Roles, Menus, Codes
  - **Code Kinds**: `GENDER`, `STATUS` with sample codes

---

## 11. CI/CD (Optional)

- GitHub Actions:
  - Backend: build, test, publish artifact; create IIS deploy package (zip)
  - Frontend: build with `vite build`; upload artifact
- Release pipeline steps:
  - Backup appsettings & keys
  - Stop site → deploy → start site
  - Run `dotnet ef database update`

---

## 12. Non‑Functional Requirements

- **Availability**: stateless API; session list in Redis enables multi‑node.
- **Performance**: DB indexes on foreign keys + `created_at`; use pagination for all lists.
- **Observability**: structured logs with `traceId`; expose `/health`.
- **Internationalization**: client i18n ready (en/zh-TW).
- **Accessibility**: color contrast & keyboard navigation in admin UI.

---

## 13. Definition of Done (per module)

1. API endpoints implemented with validators, unit tests for handlers.
2. EF migrations added; seed updated.
3. Swagger updated with request/response schemas.
4. Frontend pages with table, create/edit forms, and permission checks.
5. Audit logs recorded for create/update/delete + login events.
6. E2E happy path verified (login → menu → CRUD).

---

## 14. Future Extensions

- SSO (OIDC) integration (Azure AD / Keycloak) mapping to local roles.
- File storage service (S3‑compatible / Azure Blob) with signed URLs.
- Background jobs with Quartz.NET or Hangfire.
- WebSocket push for notifications.
- 2FA (TOTP) and device trust.

---

## 15. Sample Controller Signatures (C#)

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public sealed class UsersController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "perm:user.read")]
    public async Task<ActionResult<PagedResult<UserDto>>> List([FromQuery] PagedQuery query) => ...;

    [HttpPost]
    [Authorize(Policy = "perm:user.create")]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest req) => ...;

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "perm:user.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest req) => ...;

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "perm:user.delete")]
    public async Task<IActionResult> Delete(Guid id) => ...;
}
```

---

## 16. Acceptance Checklist

- [ ] Login/Refresh/Logout with rotation + revocation
- [ ] Role‑aware menu tree rendering
- [ ] RBAC policy checks on all endpoints
- [ ] Dictionary API consumed by selects
- [ ] Session list + force logout works
- [ ] Audit & logs visible on disk / Event Log
- [ ] IIS deployment scripts verified
- [ ] TanStack Table screens for Users/Roles/Menus/Codes

---

**Appendix A — Paging Query DTO**

```ts
// client → server
export type PagedQuery = {
  page: number
  pageSize: number
  sort?: { id: string; desc?: boolean }[]
  filters?: { id: string; value: string | number | boolean }[]
}
```

```csharp
// server
public sealed record PagedQuery(int Page = 1, int PageSize = 20,
    List<SortSpec>? Sort = null,
    List<FilterSpec>? Filters = null);

public sealed record SortSpec(string Id, bool Desc);
public sealed record FilterSpec(string Id, string? Value);
```

---

> This **spec.md** is intentionally implementation‑oriented so you can scaffold immediately and iterate. If you want, I can generate base projects for **/client** and **/server** with these conventions.
