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

## Session 7: Notebooks & Entities MVP
- [x] **Hierarchy Refinement**:
    - Implemented `NotebookType` (Characters, Places, Recipes, etc.).
    - Seeded generic types (Icon + Name).
    - Updated `NotebookEntry` to include `StoryId` (Provenance/Context).
- [x] **UI Enhancements**:
    - **Notebook Creation**: Added shelf management to create new notebooks by Type or Custom Name.
    - **Entity Creation**: Implemented named entity creation with **Auto-Link** logic (automatically links new entity to current story).
    - **Display**: Added "Link" toggle (Bookmark icon) to manage which entities appear in the current story context.
    - **Context Awareness**: Notebook entries now display the Story Title if they were created within a specific story context.
- [x] **Bug Fixes**:
    - Resolved UI list duplication issue (EF tracking vs Manual list add).
    - Fixed build errors related to property renaming (`StoryEntityLinks`).

## Current State
MVP for "World Building" is active. Users can create global entities, link them to specific stories, and add context-aware notes.

## Next Steps
1. Dashboard: List existing stories (revisiting this).
2. AI Tutor: Update prompt context to include "Linked Entities" (e.g., "Here is my cast...").
3. Polish: "Load Existing Story" navigation.

## Session 8: The Pagination Update
- [x] **Core Requirement**: Implemented `FR-2.1` (Page-Based Architecture).
- [x] **Backend**:
    - Updated `StoryPage` to include `LastModified`.
    - Refactored `StoryState` to track `CurrentPageNumber` and `TotalPages`.
- [x] **UI Implementation**:
    - Added navigation controls (Prev, Next, New Page).
    - Resized Editor to accommodate footer controls.
- [x] **Stability Fix**:
    - Resolved `TypeError: removeChild` crash on page navigation by decoupling `InitialContent` from `LoadHTMLContent`.

## Current State
User can write multi-page stories. Navigation works preserving state.

## Session 9: Research & Rationales ("The Passive AI")
- [x] **Literature Review**: Paused coding to conduct UX research for the child-specific AI interaction.
- [x] **Documentation**: Created `rationales.md` detailing the psychological basis for:
    - **Locus of Control**: Why the AI must be passive.
    - **Generative Pauses**: Why inactivity is necessary for creativity.
    - **The 30-Second Rule**: Why the timer delay is set high to avoid interruption.
- [x] **Algorithm Design**: Defined the "Context-Sensitive Inactivity Trigger".
    - Base Logic: 15s (Look Up) -> 30s (Offer Help).
    - **Punctuation Multiplier**: If the last character was a sentence terminator (. ? !), double the wait time (30s/60s) to respect "closing thought" cognitive load.

## Session 10: Implementing Passive Interventions
- [x] **Client-Side Logic**: Created `wwwroot/js/inactivity.js`.
    - Handles keypress monitoring.
    - Implements the "Punctuation Multiplier" logic locally to reduce server chatter.
    - Optimized Interop: Only notifies C# of activity if a timeout was previously triggered.
- [x] **Blazor Integration**: Updated `Editor.razor`.
    - Implemented `TutorState` (Idle, LookingUp, OfferingHelp).
    - Wired `[JSInvokable]` methods to receive timer events.
    - Updated UI to display visual cues (Eyeglasses, Lightbulb) based on state.
- [x] **Proactive AI**: Updated the "Offer Help" state to include a "Get a hint" button that pre-populates a context-aware prompt ("I need a small idea to continue...").

## Current State
The "Passive AI" system is fully implemented. The editor now "watches" the user and offers escalating levels of support based on inactivity, respecting the cognitive rhythm of writing.

## Session 11: Passive AI Refinement
- [x] **Visual Assets**: Integrated custom graphics for Alice (Reading, Alert, Help) into `wwwroot/images`.
- [x] **Timer Logic Refinement**: Tighted `inactivity.js` to ONLY respond to keystrokes in the `.ql-editor`. Mouse clicks and sidebar interactions no longer reset the timer, satisfying the strict "writing time" requirement.
- [x] **Transient Interactions**: Implemented "Brief Glance" logic (`IsGlancingUp`). Clicking Alice for help triggers a 500ms visual acknowledgment (She looks up) but does NOT change the underlying Activity State, preserving the "Passive" contract.
- [x] **UI Polish**: Persistent Avatar display stacked above interaction notes.

## Session 12: Dashboard & Resume Improvements
- [x] **Smart Resume**: Updated `Editor.razor` to automatically open the story on the **last modified page** (calculated from `Pages.OrderByDescending(LastModified)`). This fixes the user experience of "losing place" in a multi-page story.
- [x] **Deletion Safety**: Added JavaScript confirmation to the "Delete Story" button in `Home.razor` to prevent accidental data loss.
- [x] **Dashboard Verified**: Confirmed `Home.razor` already implements the full "My Bookcase" card view with theme styling and sorting.

## Session 13: Feature Completeness (Notebooks, Settings, Themes)
- [x] **Notebook Management**: 
    - Implemented **Delete** and **Rename** functionality for Notebooks and Entities.
    - Added JavaScript-based confirmation dialogs to prevent accidental data loss.
- [x] **Export System**: 
    - Implemented `ExportStory(string format)` in `Editor.razor`.
    - Added JavaScript `downloadFile` helper to support client-side file generation.
    - Supported Formats:
        - **HTML**: Formatted download with simple CSS.
        - **TXT**: Plain text download (HTML stripped via Regex).
- [x] **Configuration Module**:
    - Created `Account` table columns (`OllamaUrl`, `OllamaModel`) to support flexible AI backends.
    - Implemented global `/settings` page for user configuration.
    - Fixed `appsettings.json` vs Database configuration priority references.
- [x] **Dynamic Theming System (Refactor)**:
    - **Architecture**: Decoupled Theme from specific Stories. Moved `ActiveThemeId` to `Account` level.
    - **Logic**: User preference now persists globally; opening a Pirate story doesn't force Pirate UI if the user prefers Wonderland.
    - **Implementation**: `MainLayout.razor` now dynamically injects a `<style>` block based on `StoryState.CurrentTheme`, reskinning the entire application (Backgrounds, Buttons, Fonts) in real-time.

## Current State
The application is feature-complete for the Prototype phase. Users can Create, Write, Organize (Notebooks), Customize (Themes/Settings), and Export their work.

## Next Steps
1. **QA & Search**: Verify full search functionality across notebooks.
2. **Notebook Types**: Verify "Create Shelf" functionality works with all defined types.
3. **Deployment**: Package for local installation.
