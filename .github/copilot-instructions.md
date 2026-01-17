# Copilot Instructions for Unstickd

This project is a modern **.NET 10 Blazor Web App** using **Interactive Server** render mode and the new **.slnx** solution format. It is a creative writing tool designed for local prototyping.

## 🏗 Big Picture Architecture
- **Framework**: .NET 10.0 (Preview/Latest).
- **Hosting Model**: Server-Side Rendering (Interactive Server).
- **Deployment**: Localhost only. No Authentication implementation required.
- **Project Structure**:
    - **`Unstickd/`**: Main Blazor Web App project.
    - **`Components/`**: Contains UI components.
        - **`Pages/`**: Routable pages (e.g., `Editor.razor` `Home.razor`, `Counter.razor`).
        - **`Layout/`**: Shared layouts (e.g., `MainLayout.razor`, `NavMenu.razor`).
        - **`Routes.razor`**: Main routing configuration.
        - **`App.razor`**: Root component.
- **Core Services**:
    - **LLM**: Connects to local Ollama instance (`http://localhost:11434`) via Named HttpClient "LLM".
    - **Database**: SQLite with Entity Framework Core (`Microsoft.EntityFrameworkCore.Sqlite`).
    - **State**: In-memory Scoped service (`StoryState`) to persist data during session navigation.

## 💻 Tech Stack & Libraries
- **Language**: C# 13+ / .NET 10.
- **Rich Text**: `Blazored.TextEditor` (QuillJS wrapper).
    - *Note*: Requires QuillJS CDN links in `App.razor`.
- **Database**: Entity Framework Core + SQLite.
- **Styling**: Bootstrap 5 + Scoped CSS (`.razor.css`).

## 🧩 Key Components & Patterns
- **`StoryState.cs`**: 
    - Scoped service that acts as the "Source of Truth" for the active session.
    - Holds `Title`, `Content` (HTML), and `TutorNotes`.
    - Components subscribe to `OnChange` for updates.
- **`Editor.razor`**:
    - **Split View**: Left side Rich Text Editor, Right side AI Tutor.
    - **Logic**: Saves HTML content back to `StoryState`.
    - **AI**: Sends prompts to Ollama and updates `TutorNotes`.
- **`App.razor`**:
    - Must contain `<link>` and `<script>` tags for QuillJS and Blazored.TextEditor.

## 🛠 Developer Workflow
- **Run**: `dotnet run --project Unstickd/Unstickd.csproj`
- **Ports**: `https://localhost:7180`, `http://localhost:5112`.
- **LLM Setup**: Ensure Ollama is running (`ollama run llama3` or similar) on port 11434 before testing AI features.

## ⚠️ Critical Implementation Details
- **Interactive Server State**: 
    - `StoryState` is **Scoped**, meaning it resets if the user refreshes the browser (WebSocket disconnect).
    - Data persistence to SQLite is logical next step (currently in-memory).
- **LLM Integration**:
    - Uses named HttpClient "LLM".
    - Asynchronous UI: When calling the LLM, ensure the UI remains responsive (loading states).
- **Text Editor**:
    - `BlazoredTextEditor` relies on JS Interop. Access content via `await QuillHtml.GetHTML()`. 
    - Do not try to bind `@bind-Value` directly; use manual save methods.
- **Navigation Lock**: Project uses `BlazorDisableThrowNavigationException` in `.csproj`.
