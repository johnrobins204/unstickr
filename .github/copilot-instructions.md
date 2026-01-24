# Copilot Instructions for Unstickd

This project is a modern **.NET 10 Blazor Web App** using **Interactive Server** render mode and the new **.slnx** solution format. It is a creative writing tool designed for local prototyping.

## 🏗 Architecture & State Management
- **Hosting**: .NET 10 Blazor Server running in a **Docker container hosted in Canada** (Data Sovereignty).
- **Persistence Strategy**:
    - **Database**: SQLite accessed via **Entity Framework Core**.
    - **Security**: **Server-Side Encryption at Rest**. Critical story data is encrypted to the child's account and requires an Adult Supervisor for access (Supervisor Primacy).
    - **Session State**: `StoryState` (Scoped Service) acts as a transient data buffer.
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
    - **Inactivity Logic**: Responds to JS events (`OnInactivityStage1` @ 15s, `Stage2` @ 30s) to change avatar state. Context-sensitive pacing differentiates between "Transcription Pauses" and "Generative Pauses".
    - **Design Target**: **Passive Observation**. The AI should not "ghostwrite" content ("No Ghostwriting" Firewall).
    - **Rationale**: See `rationales.md` for the psychological basis (Locus of Control).

## 🤖 LLM Integration
- **Client**: Use the named HttpClient **"LLM"** (`IHttpClientFactory.CreateClient("LLM")`).
    - **Configuration**: Dynamic per-user via `Account.CohereApiKey` (managed by Supervisor).
    - **Endpoint**: Uses **Cohere's API** (`api.cohere.com`) specifically **Command R+ (Reasoning)** models.
- **Privacy**: "Hybrid" AI strategy. No user data stored on inference side (Non-training agreement).
- **Pattern**:
    - UI must remain responsive. Use async/await and loading indicators (`IsLoadingAI`).
    - Responses populate `StoryState.TutorNotes`.
- **Planned Features** (See `requirements.md`):
    - **Spark Protocol**: A specific Q&A loop for blank pages (Divergent -> Convergent brainstorming).
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
