# AGENTS.md - EECOnline Codebase Guidelines

## Build Commands
- **Build**: `msbuild trunk/EECOnline.csproj /p:Configuration=Release` or open `e-service/ES.sln` in Visual Studio
- **Restore NuGet**: `nuget restore e-service/ES.sln`
- **No unit tests** configured in this legacy codebase

## Tech Stack
- ASP.NET MVC 5.2.3 / .NET Framework 4.5.2 / SQL Server / iBATIS.NET 1.6.4 / Dapper

## Code Style
- **Naming**: PascalCase for classes/methods, UPPER_SNAKE_CASE for DB columns/entities, camelCase for JS
- **Entities**: Implement `IDBRow`, use `[IdentityDBField]` for PKs, table prefix `Tbl` (e.g., `TblEEC_Hospital`)
- **DAO Pattern**: Inherit `BaseDAO`, use iBATIS SqlMaps in `SqlMaps/*.xml` with `#param#` (never `$param$`)
- **Controllers**: Inherit `BaseController`, use `[LoginRequired]` attribute, standard actions: Index/Query/New/Modify/Save/Delete
- **Areas**: A1-A8 for features, Login for auth, SHARE for common components

## Security Requirements
- Always use parameterized queries (`#param#` in iBATIS, `@param` in Dapper)
- Hash passwords with SHA512 via `CommonsServices.HashSHA512()`
- Use `[ValidateAntiForgeryToken]` on POST actions
- Mask sensitive data: `CommonsServices.MaskIDN()`, `MaskName()`, `MaskPhone()`

## Error Handling
- Wrap DAO calls in try-catch with `BeginTransaction()`/`CommitTransaction()`/`RollBackTransaction()`
- Log errors via `LogUtils.Error(message, exception)`
- Return `Json(new { success = false, message = "..." })` for AJAX errors

## File Encoding Requirements (CRITICAL)
- **All `.cshtml` and `.cs` files MUST be saved as UTF-8 with BOM** (Byte Order Mark: `EF BB BF`)
- Without BOM, Chinese characters will display as garbled text (e.g., `??亥岷` instead of `送出查詢`)
- **Verify BOM exists**: `Get-Content -Path "file.cshtml" -Encoding Byte -TotalCount 3` should show `EF BB BF`
- **Add BOM via PowerShell**:
  ```powershell
  $content = Get-Content -Path "file.cshtml" -Raw -Encoding UTF8
  $Utf8Bom = New-Object System.Text.UTF8Encoding $True
  [System.IO.File]::WriteAllText("file.cshtml", $content, $Utf8Bom)
  ```
- See `documents/bug-fix/Razor視圖檔案中文編碼問題修復說明.md` for detailed explanation

## SQL DateTime Format Requirements
- **DateTime columns stored as NVARCHAR** must use format `yyyy/MM/dd HH:mm:ss`
- **Always use explicit FORMAT()** in SQL scripts: `FORMAT(GETDATE(), 'yyyy/MM/dd HH:mm:ss')`
- **Never insert DATETIME directly** into NVARCHAR columns (causes implicit conversion with wrong format)
- Example: `INSERT INTO EEC_Apply (createdatetime) VALUES (FORMAT(@now, 'yyyy/MM/dd HH:mm:ss'))` ✓
- Bad: `INSERT INTO EEC_Apply (createdatetime) VALUES (@now)` ✗ (produces `Nov 27 2025 10:04AM`)
## MSBuild Configuration
- **MSBuild Path**: `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"`
- **Always use Visual Studio 2019** MSBuild (VS 2022 BuildTools lacks WebApplication targets)
- **Build command**: `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe" trunk/EECOnline.csproj /p:Configuration=Debug /v:minimal`
- **Use Debug configuration** to avoid sgen.exe errors in Release builds

## Test Credentials
- **Backend Admin (for Mock Login verification)**:
  - Username: `testadmin`
  - Password: `Test@1234`
- **Database**: `EEC_PD_DB` on Docker container `moh-sqlserver`
  - SA Password: `YourStrong!Passw0rd`
- **IIS Express**: Port 8080, path `F:\AITest\MOH-CodeBaseNew\trunk`

## Deployment Files Location (CRITICAL)
- **ALL deployment files/resources/documents MUST be saved under `deploy/`** at project root
- **NEVER create deployment folders inside `e-service/` or `trunk/`** (e.g., `e-service/deploy-xxx/` is WRONG)
- **Folder structure**:
  ```
  deploy/
  ├── e-service/              ← e-service (民眾端) deployment
  │   ├── README-部署說明.md  ← Deployment instructions
  │   ├── docs/               ← Documentation
  │   ├── sql/                ← SQL scripts and source data
  │   └── source/             ← Code files (bin/, Views/, etc.)
  ├── trunk/                  ← trunk (管理後台) deployment (if needed)
  ├── build-release.ps1       ← Build scripts
  └── deploy-*.ps1            ← Deployment scripts
  ```
- **Scripts location**: Common scripts (`build-release.ps1`, `deploy-*.ps1`) stay at `deploy/` root level 
