# Copilot Instructions for Unstickd

You are assisting with **Unstickd**, a .NET 10 Blazor Web App (Interactive Server) designed as a creative writing tutor for children. 

## 🏗 Project Architecture
- **Framework**: .NET 10 Blazor Server (containerized).
- **Solution Format**: Uses `.slnx` (Visual Studio Solution XML).
- **Data**: SQLite with **WAL Mode** enabled (Write-Ahead Logging) for concurrency.
- **ORM**: Entity Framework Core.
- **AI**: **Cohere Command R+** (Reasoning) via named HttpClient `"LLM"`.
- **State**: `StoryState` (Scoped Service) manages active session data (Story, Notebooks, Tutor).

## 🧩 Core Domain Concepts
- **Supervisor Primacy**: Critical settings/data are gated. Child vs. Adult roles.
- **No Ghostwriting**: The AI **never** writes story content for the user. It only asks questions ("Spark Protocol") or reviews text.
- **Data Sovereignty**: Critical story data is encrypted (Server-Side Encryption) and hosted in Canada.

## 🛠 Developer Workflow
- **Run**: `dotnet run --project Unstickd/Unstickd.csproj`
- **Watch**: `dotnet watch --project Unstickd/Unstickd.csproj`
- **Migrations**: `dotnet ef migrations add <Name> --project Unstickd/Unstickd.csproj`
- **Testing**:
    - Unit: `dotnet test Unstickd.Tests.Unit`
    - Integration: `dotnet test Unstickd.Tests.Integration`
    - E2E: `dotnet test Unstickd.Tests.E2E` (Playwright)

## 📂 Key Code Patterns & Directories
- **Rich Text**: Uses `Blazored.TextEditor` (QuillJS).
    - ⚠️ **Critical**: No two-way binding. Use `GetHTML()` and `LoadHTML()` explicitly.
    - **Styles**: Scoped CSS (`.razor.css`) preferred.
- **Services (`Unstickd/Services/`)**:
    - `StoryState.cs`: Central session state/cache.
    - `TutorOrchestrator.cs`: Manages AI conversational state/pacing.
    - `CohereTutorService.cs`: AI API integration.
- **JS Interop (`Unstickd/wwwroot/js/`)**:
    - `editor.js`: Auto-save debounce logic (2s), scroll events.
    - `inactivity.js`: "Passive Observation" timers (Stage 1 @ 15s, Stage 2 @ 30s).
- **Logging**: Serilog writes to `logs/unstickd-*.txt`.
    - **Rule**: Never log `Story.Content` (Privacy).

## ⚠️ Implementation Guidelines
1. **Async/Await**: Mandatory for all I/O, especially AI calls (can be slow).
2. **Navigation**: `<BlazorDisableThrowNavigationException>` is `true`.
3. **Themes**: Controlled via `StoryState.CurrentTheme` injecting CSS variables.
4. **Error Handling**: `CustomErrorBoundaryLogger` catches circuit errors.

## 🔎 Service Boundaries
- **Tutor Panel**: Isolated component for AI chat.
- **Editor**: Continuous scroll viewport.
- **Notebooks**: Entity management (Characters/Places) linked to Stories.

## 🧪 Testing Strategy
- **Unit**: Business logic in Services/Models.
- **Integration**: EF Core operations and Service interactions.
- **E2E**: Critical flows (Onboarding, Editor saving, Tutor interaction).
