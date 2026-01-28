
# Copilot Instructions for StoryFort

**Summary:**
These instructions are the authoritative, up-to-date guide for AI coding agents working on StoryFort. They summarize the architecture, workflows, and unique conventions of the project. Read this before making any code changes or answering developer queries.

**Quick Start:**
- StoryFort is a .NET 10 Blazor Server app (containerized) for creative writing tutoring.
- AI agents must never generate story text—use the "Spark Protocol" (questioning) or review only.
- All critical data is encrypted and regionally hosted (Canada).
- Key files: requirements/requirements.md, developer-diary.md, architecture/ARCHITECTURE.md, governance/AI-System-Card.md, rationales.md, UXUI_Design/UI_UX_detail_reqs.md.

---

You are assisting with **StoryFort**, a .NET 10 Blazor Web App (Interactive Server) designed as a creative writing tutor for children.

## 🏗 Project Architecture
- **Framework**: .NET 10 Blazor Server (containerized).
- **Solution Format**: Uses `.slnx` (Visual Studio Solution XML).
- **Data**: SQLite with **WAL Mode** enabled for concurrency.
- **ORM**: Entity Framework Core.
- **State**: `StoryState` (Scoped Service) acts as the central session cache (Story, Notebooks, Tutor).
- **AI**: **Cohere Command R+** via `ICohereTutorService` / named HttpClient `"LLM"`.

## 🧩 Core Domain Concepts
- **Story Model**: 
  - `Story.Content` holds the full text (continuous scroll).
  - `Story.Pages` is **deprecated** but retained for migration history. Do not use for new features.
  - `Story.Genre` classifies stories (added via migration).
- **Supervisor Primacy**: Critical settings (`Account` model) are gated behind adult roles.
- **No Ghostwriting**: The AI **never** generates story text. It uses the "Spark Protocol" (questions) or reviews.
- **Data Sovereignty**: Critical data is encrypted and regionally hosted (Canada).

## 🤖 AI Integration Patterns
- **Orchestration**: `TutorOrchestrator` manages conversational flow and safeguards.
- **Prompt Strategies**: Implements `IPromptStrategy` for different modes:
  - `SparkPromptStrategy`: State-machine driven questioning (Sensory -> Attribute -> Conflict).
  - `ReviewPromptStrategy`: Feedback analysis.
- **Safeguards**: `ValidateSafeguards()` enforces "Defense in Depth" (PII, Prompt Injection) before API calls.

## 🛠 Developer Workflow
- **Run**: `dotnet run --project StoryFort/StoryFort.csproj`
- **Watch**: `dotnet watch --project StoryFort/StoryFort.csproj`
- **Migrations**: `dotnet ef migrations add <Name> --project StoryFort/StoryFort.csproj`
- **Testing**:
    - **Unit** (`StoryFort.Tests.Unit`): Logic/JSON parsing. Mock all I/O.
    - **Integration** (`StoryFort.Tests.Integration`): EF Core/SQLite. Use `EnsureDeleted()`/`EnsureCreated()`.
    - **E2E** (`StoryFort.Tests.E2E`): Playwright. Use `GetByRole` selectors (accessibility first).

## 📂 Key Code Patterns
- **Rich Text**: `Blazored.TextEditor` (QuillJS).
    - ⚠️ **Critical**: No two-way binding. Use `GetHTML()` and `LoadHTML()` explicitly.
    - **Styles**: Use scoped CSS (`.razor.css`).
- **Services**:
    - `StoryState.cs`: Central point for active session data.
    - `TutorOrchestrator.cs`: Conversational state manager.
- **Logging**: Serilog writes to `logs/`. **NEVER** log `Story.Content` (Privacy).

## ⚠️ Implementation Guidelines
1. **Async/Await**: Mandatory for all I/O (Database, AI API).
2. **Navigation**: `<BlazorDisableThrowNavigationException>` is `true`.
3. **Themes**: Injected via `StoryState.CurrentTheme` (CSS variables).
4. **Error Handling**: `CustomErrorBoundaryLogger` for circuit hardening.

## 📚 Read-In Plan for Agents
To fully understand the project's constraints and philosophy, read these files in order:

1. **The Core "Truth"**:
   - `requirements/requirements.md`: The functional bible. Defines the "No Ghostwriting" rule, Spark Protocol, and Inactivity triggers.
   - `developer-diary.md`: The living history. Check this to see what was *just* built (e.g., Spark Handoff) and what's next.

2. **Architecture & Decisions**:
   - `architecture/ARCHITECTURE.md`: Explains the "Hybrid Engine" (Blazor + Cohere) and Data Sovereignty pillars.
   - `governance/ADR-004-Data-Resilience.md`: Explains the Litestream integration for SQLite WAL replication (critical for understanding `entrypoint.sh`).

3. **Design & Psychology**:
   - `rationales.md`: **Crucial** for understanding *why* the AI waits 30s or why there is no "Insert" button. (Locus of Control, CASA paradigm).
   - `UXUI_Design/UI_UX_detail_reqs.md`: Read this before touching `Editor.razor` or `TutorPanel.razor`. Defines "Death of the Save Button" and accessibility specs.

4. **Safety & Governance**:
   - `governance/AI-System-Card.md`: Defines the "Defense in Depth" strategy for AI inputs/outputs.


