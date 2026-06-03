# Project Instructions: SafeWalk-VNJP-v3

## Build & Validation Mandate
- **Post-Update Build Check**: After making any modifications to the source code (including C# files and Razor views), you MUST execute `dotnet build` to verify that the project compiles without errors.
- **Process Management**: If a build fails due to a file lock (e.g., MSB3026/MSB3021), inform the user that the application process needs to be stopped, and attempt the build again once the process is released.
- **Namespace Verification**: Always ensure that necessary namespaces (e.g., `Microsoft.EntityFrameworkCore`) are added when using extension methods like `Include` or `ToListAsync`.

## Technology Stack
- **Backend**: ASP.NET Core 10.0 (net10.0)
- **Database**: PostgreSQL (Npgsql) with EF Core
- **Frontend**: Leaflet.js for Map functionality
- **Authentication**: ASP.NET Core Identity (Unique Email mandated)

## Development Workflow
1. **Research**: Map dependencies and existing logic.
2. **Strategy**: Propose surgical changes.
3. **Execution**: Apply changes and immediately run `dotnet build`.
4. **Validation**: Confirm build success and behavioral correctness.
