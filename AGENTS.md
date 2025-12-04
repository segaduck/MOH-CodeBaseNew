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
