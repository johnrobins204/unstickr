# Copilot Instructions for StoryFort

**Summary:**
These instructions are the authoritative, up-to-date guide for AI coding agents working on StoryFort. They summarize the architecture, workflows, and unique conventions of the project. Read this before making any code changes or answering developer queries.

**Quick Start:**
- **Project**: .NET 10 Blazor Web App (Interactive Server), container-ready.
- **Mission**: Creative writing tutor for children. **AI never writes the story** (Spark Protocol).
- **Data**: SQLite (WAL enabled), EF Core, Regionally hosted (Canada) for data sovereignty.
- **Key Files**: 
  - [requirements/requirements.md](requirements/requirements.md) (Functional Truth)
  - [developer-diary.json](developer-diary.json) (History & Current State) 
  - [architecture/ARCHITECTURE.md](architecture/ARCHITECTURE.md) (Deep Dive)
  - [governance/AI-System-Card.md](governance/AI-System-Card.md) (Safety & Ethics)

---

##  Project Architecture

### Framework & Infrastructure
- **Stack**: .NET 10, Blazor Server (Interactive Server).
- **Solution**: \.slnx\ (Visual Studio Solution XML).
- **Database**: SQLite with **WAL Mode** enabled (critical for concurrency).
- **Logging**: Serilog to \logs/\, **NEVER** log \Story.Content\.

### Core Services (Scope & Lifecycle)
- **\StoryState\ (Scoped)**: The central session manager. Caches the active Story, Notebooks, and Tutor context for the user's session.
- **\TutorOrchestrator\ (Scoped)**: Manages appropriate conversational flow, state transitions, and usage of the LLM. 
- **\ArchetypeService\ (Singleton)**: Read-only provider for character archetypes.
- **\ICohereTutorService\ (Scoped)**: Typed client for the Cohere API (named client "LLM").

##  Core Domain Concepts

### The Story Model
- **Content**: \Story.Content\ (string) holds the full HTML text (QuillJS output).
- **Genre**: \Story.Genre\ classifies the story (Default: "General").
- **Pages**: \Story.Pages\ is **DEPRECATED**. Retained only for migration history; do not use for active features.

### AI Philosophy & Constraints
- **No Ghostwriting**: The AI must **never** generate story text for the user. It may only ask questions (Spark Protocol) or review text.
- **Spark Protocol**: A state-machine approach to questioning (Sensory -> Attribute -> Conflict) to unblock writers without doing the work for them.
- **Supervisor Primacy**: \Account\ model settings are gated behind adult/supervisor roles.

##  Developer Workflow

### CLI Commands
- **Run**: \dotnet run --project StoryFort/StoryFort.csproj\
- **Watch**: \dotnet watch --project StoryFort/StoryFort.csproj\
- **Migrations**: \dotnet ef migrations add <Name> --project StoryFort/StoryFort.csproj\
- **Database Update**: \dotnet ef database update --project StoryFort/StoryFort.csproj\

### Testing Strategy
- **Unit (\StoryFort.Tests.Unit\)**: Pure logic, regex, JSON parsing. Mock all I/O.
- **Integration (\StoryFort.Tests.Integration\)**: EF Core/SQLite interactions. Use \EnsureDeleted()\/\EnsureCreated()\.
- **E2E (\StoryFort.Tests.E2E\)**: Playwright. Prioritize accessibility selectors (\GetByRole\).

##  Critical Implementation Details

1. **Rich Text (QuillJS)**:
   - component: \Blazored.TextEditor\.
   - **Warning**: No automatic two-way binding. You must explicitly call \GetHTML()\ to save and \LoadHTML()\ to load.
   - Styling: Use scoped CSS (\.razor.css\).

2. **Async/Await**: 
   - Mandatory for all I/O, especially Database and AI API calls.

3. **Navigation**: 
   - \BlazorDisableThrowNavigationException\ is set to \	rue\ in \.csproj\.

4. **Safety & Governance**:
   - **Defense in Depth**: \ValidateSafeguards()\ must be called before sending any prompt to the LLM.
   - **Data Sovereignty**: Critical data is encrypted at rest.

##  Read-In Priority
1. [requirements/requirements.md](requirements/requirements.md) - The rules (No Ghostwriting, etc.).
2. [developer-diary.json](developer-diary.json) - What has just been built (e.g., Archetypes).
3. [architecture/ARCHITECTURE.md](architecture/ARCHITECTURE.md) - The "Hybrid Engine" design.
4. [rationales.md](rationales.md) - Why we do things this way (Psychology).
