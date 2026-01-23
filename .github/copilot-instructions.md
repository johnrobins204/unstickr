# Copilot Instructions for Unstickd

This project is a modern **.NET 10 Blazor Web App** using **Interactive Server** render mode and the new **.slnx** solution format. It is a creative writing tool designed for local prototyping.

## 🏗 Architecture & State Management
- **Hosting**: Server-Side Rendering (Interactive Server, WebSocket-based).
- **Persistence Strategy**:
    - **Database**: SQLite accessed via **Entity Framework Core**. This is the definitive source of truth.
    - **Session State**: `StoryState` (Scoped Service) acts as a transient data buffer for the active user session. Do **NOT** rely on it for long-term storage; always commit critical changes to `AppDbContext`.
- **Data Model**:
    - **Hierarchy**: `Account` owns `Notebooks` and `Stories`.
    - **Notebooks**: Global resource containing `Entities` (Characters, Places) and `Entries` (Notes).
    - **Stories**: Contain `Content` (HTML string). Can "link" to Notebook Entities via `StoryEntityLink`.
        - *Legacy Note*: `StoryPage` entity exists but is deprecated; logic migrates old pages to `Story.Content` on load.
    - **Themes**: Cosmetic overlays associated with Accounts.

## 🧩 Key Components
- **`Editor.razor`**: The core workspace.
    - **Left Pane**: Wrapper around `Blazored.TextEditor` (QuillJS) using **Continuous Scrolling** (visual "book" viewport).
    - **Right Pane**: Tabbed interface for **Notebooks** (Linkable entities) and **Tutor** (AI Chat).
    - **Saving Logic**: 
        - **Auto-Save**: Triggered via JS debounce (2s).
        - **Manual Save**: Icon indicator updates `Story.Content` in DB.
- **`TutorPanel.razor`**:
    - AI assistant integration.
    - **Inactivity Logic**: Responds to JS events (`OnInactivityStage1` @ 15s, `Stage2` @ 30s) to change avatar state.
    - **Design Target**: primarily **Passive Observation**. The AI should not "ghostwrite" content. Direct chat is for specific "Review" or "Spark" modes (see `requirements.md`).
    - **Rationale**: See `rationales.md` for the psychological basis of the "Unstickr" intervention (Locus of Control).

## 🤖 LLM Integration
- **Client**: Use the named HttpClient **"LLM"** (`IHttpClientFactory.CreateClient("LLM")`).
    - **Configuration**: Dynamic per-user via `Account.OllamaUrl` (default: `http://localhost:11434`) and `Account.OllamaModel`.
    - **Endpoint**: Uses `/api/generate` (single completion), not chat history.
- **Pattern**:
    - UI must remain responsive. Use async/await and loading indicators (`IsLoadingAI`).
    - Responses populate `StoryState.TutorNotes`.
- **Planned Features** (See `requirements.md`):
    - **Spark Protocol**: A specific Q&A loop for blank pages.
    - **Review Protocol**: Style/Grammar coaching on demand.
    - **Assignments**: Structured prompts in a split view.

## 🛠 Developer Patterns & workflows
- **Rich Text Handling**:
    - **Library**: `Blazored.TextEditor`.
    - **Caveat**: **No two-way binding**. You must manually call `await QuillHtml.GetHTML()` to read and `LoadHTML()` to write.
- **Logging**:
    - Use **Serilog**. Logs are written to `logs/unstickd-YYYYMMDD.txt`.
- **JavaScript Interop**: 
    - Used for Editor auto-save debounce (`editor.js`) and Inactivity detection (`inactivity.js`).
    - Keep JS interop calls async.

## ⚠️ Important Implementation Details
1. **Scoped CSS**: Prefer scoped styles (`.razor.css`) over global CSS.
2. **Navigation**: Project disables navigation exceptions: `<BlazorDisableThrowNavigationException>true</BlazorDisableThrowNavigationException>`.
3. **Theme System**: Themes control fonts, colors, and background via CSS variables injected dynamically based on `StoryState.CurrentTheme`.

## 🚀 Common Commands
- **Run**: `dotnet run --project Unstickd/Unstickd.csproj`
- **Watch**: `dotnet watch --project Unstickd/Unstickd.csproj`
- **Migrations**: `dotnet ef migrations add <Name> --project Unstickd/Unstickd.csproj`
