# ReworkSuite

A shared .NET 9 backend with two front ends:
- **Rework Desktop**: a .NET MAUI station app. A BlazorWebView hosts Razor components, and a `<rework-panel>`
  Angular Element runs inside the same page. The two talk through a small JavaScript bridge.
- **Rework Settings**: a standalone Angular web app for shared settings and local preferences.

## Layout

```
ReworkSuite/
├─ src/
│  ├─ Rework.Domain/          entities, rules, exceptions (no dependencies)
│  ├─ Rework.Application/     use cases, repository interfaces (depends on Domain)
│  ├─ Rework.Infrastructure/  EF Core DbContext, repositories, migrations (depends on Application)
│  ├─ Rework.WebApi/          REST controllers, DI, CORS, ProblemDetails (depends on all three)
│  ├─ Rework.Desktop/         MAUI + BlazorWebView + Razor components + JS bridge
│  ├─ rework-panel/           Angular Element <rework-panel>, bundled into the MAUI wwwroot
│  └─ rework-settings-web/    Angular standalone settings app
├─ scripts/                   bootstrap, desktop setup, backend run
├─ .gitignore  .gitattributes
└─ README.md
```

## Prerequisites

| Tool | Version | Used for |
|---|---|---|
| .NET SDK | 9.0 | backend, MAUI |
| .NET MAUI workload | `dotnet workload install maui` | desktop app (Windows) |
| EF Core CLI | `dotnet tool install --global dotnet-ef --version 9.0.0` | migrations |
| Node.js | 20 LTS or 22 LTS | Angular |
| Angular CLI | 19 (`npm i -g @angular/cli@19`) | optional, the npm scripts call it |
| SQL Server | 2022 (Docker or Azure SQL) | database |

## 1. Start SQL Server (local)

```bash
docker run -d --name rework-sql -p 1433:1433 \
  -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='Rework!Local2026' \
  mcr.microsoft.com/mssql/server:2022-latest
```

Set the connection string in your shell, not in a file:

```bash
export ConnectionStrings__ReworkDb="Server=localhost,1433;Database=ReworkSuite;User Id=sa;Password=Rework!Local2026;TrustServerCertificate=True;"
```

PowerShell equivalent: `$env:ConnectionStrings__ReworkDb = "..."`.

## 2. Build everything

```bash
chmod +x scripts/*.sh
./scripts/bootstrap.sh
```

This creates the solution file if it's missing, restores and builds the .NET projects, and installs and builds
both Angular apps.

## 3. Create the first migration (once)

```bash
dotnet ef migrations add InitialCreate --project src/Rework.Infrastructure --startup-project src/Rework.WebApi
```

Commit the generated `Migrations/` folder. Every environment applies it.

## 4. Run the backend

```bash
./scripts/run-backend.sh
```

This applies migrations, then serves the API at `https://localhost:7180`. Swagger is at
`https://localhost:7180/swagger` in Development.

Smoke test:
```bash
curl -sk https://localhost:7180/api/rework-items
curl -sk -X POST https://localhost:7180/api/rework-items \
  -H 'Content-Type: application/json' \
  -d '{"defectDescription":"Missing spot weld","stationId":"ST-01"}'
```

## 5. Run the Settings web app

```bash
cd src/rework-settings-web
npm start            # http://localhost:4200
```

It calls the API at `https://localhost:7180` (see `src/app/core/config.ts`). CORS already allows
`http://localhost:4200`. If you see a certificate error, trust the ASP.NET dev certificate:
`dotnet dev-certs https --trust`.

## 6. Build the panel and run the desktop app

```bash
cd src/rework-panel && npm run build && cd ../..   # done by bootstrap.sh, shown here for clarity
./scripts/setup-desktop.sh                          # copies main.js and polyfills.js into the MAUI wwwroot
```

The MAUI project files (`Platforms/`, `Resources/`) come from the MAUI template. If they are missing, generate
them once and keep our files:

```bash
dotnet new maui -n Rework.Desktop -o /tmp/maui-template
cp -r /tmp/maui-template/Platforms /tmp/maui-template/Resources src/Rework.Desktop/
```

Then run it:

```bash
cd src/Rework.Desktop
dotnet run -f net9.0-windows10.0.19041.0
```

Set the API address per machine with a preference (the code reads `api.baseUrl`, default `https://localhost:7180`).
Point it at another server with a PowerShell command such as
`[Microsoft.Maui.Storage.Preferences]::Default.Set("api.baseUrl", "https://rework.plant.internal")`, or edit the
default in `MauiProgram.cs`.

## How the desktop app connects its parts

```
Home.razor ──JS interop──► bridge.js ──► <rework-panel>.items = [...]     (C# → Angular)
Home.razor ◄──[JSInvokable] OnPanelAction ◄── bridge.js ◄── CustomEvent "actionRequested"   (Angular → C#)
Home.razor ──HttpClient──► ReworkApiClient ──► ReworkSuite API             (API calls stay in C#)
```

- The Angular panel never calls the API. It only renders what it receives and reports clicks.
- C# owns data and API calls, so the WebView doesn't need CORS access to the backend.
- The panel emits `actionRequested` without bubbling, so `bridge.js` listens in the capture phase.

## 7. Git: create a repository and push

```bash
cd ReworkSuite
git init
git add .
git status                       # confirm no bin/, obj/, node_modules/, dist/, or secrets are staged
git commit -m "Initial commit: ReworkSuite backend, desktop, and settings app"
git branch -M main
git remote add origin <your-repo-url>
git push -u origin main
```

Use your own remote URL (GitHub, Azure DevOps, or GitLab) and sign in with a personal access token or an SSH key on
your machine. Never commit connection strings. Keep them in environment variables or Key Vault.

## Known limits (read before production)

- **Not compiled in this environment.** Expect small fixes on the first build, especially in the MAUI and Razor
  projects, and in Angular template type checks under `strictTemplates`.
- **No authentication.** `Actor` and `Approver` values come from the client. Add Microsoft Entra ID (bearer tokens
  validated in the API) before production, so audit fields come from the token.
- **Operator identity** in the desktop app is a constant (`OP-118`). Wire it to the signed-in operator.
- **Angular version** is 19 across the Angular projects. Upgrade them together.
- **Tests** are not included yet. Add a unit test project for `Rework.Domain` first, because the state rules
  live there.
