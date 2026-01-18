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
    - **Stories**: Composed of `StoryPages`. Can "link" to Notebook Entities via `StoryEntityLink`.
    - **Themes**: Cosmetic overlays associated with Accounts/Stories.

## 🧩 Key Components
- **`Editor.razor`**: The core workspace.
    - **Left Pane**: Wrapper around `Blazored.TextEditor` (QuillJS).
    - **Right Pane**: Tabbed interface for **Notebooks** (Linkable entities) and **Tutor** (AI Chat).
    - **Saving Logic**: Manual save button triggers `SaveContent` -> updates `StoryPage` in DB.
- **`AppDbContext`**:
    - Pre-seeded with `NotebookType` (Classic categories like Spells, Recipes) and `Theme` (Visual styles).
- **`StoryState.cs`**:
    - Holds volatile UI state: `CurrentPageNumber`, `Content` (HTML buffer), `LinkedEntityIds`.

## 🤖 LLM Integration
- **Client**: Use the named HttpClient **"LLM"** (`IHttpClientFactory.CreateClient("LLM")`).
    - Base Address: `http://localhost:11434` (Ollama).
    - Timeout: 5 minutes (Long-running generation).
- **Pattern**:
    - UI must remain responsive. Use async/await and loading indicators (`IsLoadingAI`).
    - Responses usually populate a "Tutor" chat or notes area, not the main story text directly.

## 🛠 Developer Patterns & workflows
- **Rich Text Handling**:
    - **Library**: `Blazored.TextEditor`.
    - **Caveat**: **No two-way binding**. You must manually call `await QuillHtml.GetHTML()` to read and `LoadHTML()` to write.
- **JavaScript Interop**: 
    - Keep it simple. Use standard `window.confirm` and `window.prompt` via `IJSRuntime` for quick user inputs/validations.
    - Use `downloadFile` function in `_Layout` or `App` for exports.
- **Logging**:
    - Use **Serilog**. Logs are written to `logs/unstickd-YYYYMMDD.txt`.
- **Navigation**:
    - Project disables navigation exceptions: `<BlazorDisableThrowNavigationException>true</BlazorDisableThrowNavigationException>`.

## ⚠️ Important Implementation Details
1. **Scoped CSS**: Prefer scoped styles (`.razor.css`) over global CSS.
2. **Async Everywhere**: All DB operations and LLM calls must be async.
3. **Pagination**: Stories are chunked into `StoryPage` entities. The logic mimics a physical book (Page 1, Page 2...).
4. **Theme System**: Themes control fonts, colors, and background via CSS variables or inline styles injected dynamically.

## 🚀 Common Commands
- **Run**: `dotnet run --project Unstickd/Unstickd.csproj`
- **Watch**: `dotnet watch --project Unstickd/Unstickd.csproj`
- **Migrations**: `dotnet ef migrations add <Name> --project Unstickd/Unstickd.csproj`
