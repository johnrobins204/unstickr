# Unstickd Developer Diary

## Session 1: Project Initialization & Architecture
- [x] **Project Scaffolding**: Created .NET 10 Blazor Web App (Interactive Server) with .slnx solution.
- [x] **Core Libraries**: Added Entity Framework Core (SQLite), Blazored.TextEditor (QuillJS).
- [x] **Requirements Specification**: Established "passive" AI constraints (no ghostwriting) and "notebook" metaphor interactions.

## Session 2: Data Layer & Onboarding
- [x] **Data Models**: Implemented `Theme`, `Story`, and `StoryPage` entities.
- [x] **Database Setup**: Configured SQLite context and successfully ran `InitialCreate` migration.
- [x] **Seeding**: Populated database with 8 classic literature themes (Wonderland, Oz, etc.).
- [x] **Onboarding UI**:
    - Implemented Date of Birth "Age Gate" on Home page.
    - Created `ThemeChooser` page to display database-driven theme tiles.
    - Created `EmptyLayout` for distraction-free onboarding.

## Session 3: DevOps & Debugging
- [x] **Build Recovery**: Resolved "phantom" compilation errors via hard clean of `bin`/`obj`.
- [x] **Logging Strategy**: Implemented Serilog with daily file rotation (`logs/unstickd-*.txt`).
- [x] **Error Handling**: 
    - Added Global `<ErrorBoundary>` in `Routes.razor`.
    - Created `CustomErrorBoundaryLogger` to capture UI crashes.
- [x] **Monitoring**: Added `UserCircuitHandler` to trace WebSocket connection life-cycles.

## Session 4: Troubleshooting & Stabilization
- [x] **Static Assets**: Resolved 404 errors for JS libraries by adding `app.UseStaticFiles()` to middleware pipeline.
- [x] **Dependency Resolution**: Fixed QuillJS crash by aligning CDN version (1.3.6) with `Blazored.TextEditor` requirements.
- [x] **Ghost UI Fix**: Resolved persistent "An unhandled error has occurred" message in `EmptyLayout`.
    - **Diagnosis**: The standard Blazor error UI `<div id="blazor-error-ui">` was lacking the `display: none` style. In the new `EmptyLayout`, the default project styles weren't targeting this element correctly, causing the raw HTML to render visibly as static black text even when no error existed.
    - **Fix**: Added explicit `style="display: none"` (or corresponding CSS) to the error container in `EmptyLayout.razor`.

## Current State
App is stable. Onboarding flow (Age Gate -> Theme Chooser) works visually without errors. Theme cards are rendered from SQLite.

## Session 5: Core Features (The "Meat")
- [x] **Theme Logic**: Implemented `SelectTheme` in `ThemeChooser`.
    - Creates a new `Story` entity with an initial `StoryPage`.
    - Persists to SQLite.
    - Injects into `StoryState`.
- [x] **Editor UI**: Rebuilt `Editor.razor` from scratch.
    - **Routing**: Added route parameter `/{StoryId}`.
    - **Split-Pane Layout**: 8-col Editor / 4-col AI Tutor.
    - **Persistence**: Implemented `OnInitializedAsync` (Load) and `SaveContent` (Update) DB logic.
    - **Text Editor**: Configured `BlazoredTextEditor` with custom formatting toolbar.
    - **AI Integration**: Added UI for chatting with Ollama (with context injection).
- [x] **Critical Bug Fix**: Resolved `TypeError: removeChild` crash on Save in Editor.
    - **Cause**: Blazor fighting with QuillJS for DOM control when rebinding `Content` after save.
    - **Fix**: Decoupled initialization (`InitialContent`) from updates (`StoryState.Content`). Blazor initializes the editor once; Quill manages the DOM thereafter.

## Current State
The "Happy Path" is fully coded and verified:
1. User enters Age.
2. User selects Theme -> Story is created in DB.
3. User lands on Editor -> Loads Story.
4. User writes -> Content Saves successfully without errors.
5. User asks AI -> Sends prompt to local Ollama.

## Next Steps
1. Polish the AI prompt engineering (better personas, system prompts).
2. Handle edge cases (Navigation away without saving?).
3. Add "Load Existing Story" functionality (Dashboard/Home).

## Session 6: Global Notebooks & Account Architecture
- [x] **Conceptual Pivot**: Shifted from discrete, isolated stories to an "Account/World" model (Reusable Entities).
- [x] **Data Refactor**:
    - Introduced `Account` entity as the root.
    - Reparented `Notebook` from `Story` to `Account`.
    - Added `AccountId` to `Story`.
- [x] **Database Migration**:
    - Reset SQLite database to cleanly implement the structural changes.
    - Applied `RefactorToAccountModel` migration.
- [x] **Logic Updates**:
    - `ThemeChooser.razor`: Seeds/Checks global notebooks on account.
    - `Editor.razor`: Fetches `Account.Notebooks`.

## Current State
Global Notebooks implemented. Architecture supports multi-story entity sharing.

## Next Steps
1. Runtime verification of Entity sharing.
2. Implement UI for linking specific Entities to a Story context.
3. Dashboard: List existing stories.

