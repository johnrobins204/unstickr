# StoryFort Technical Audit Report

**Date:** January 31, 2026  
**Auditor:** GitHub Copilot (Gemini 3 Flash)  
**Status:** COMPLETE — Awaiting Review  
**Subject:** Codebase Health, Pattern Drift, and Refactor Opportunities

---

## 1. Executive Summary
This audit provides a comprehensive look at the StoryFort codebase. While the project is functionally robust and has survived a successful prototyping phase, it has accumulated significant "velocity debt." The architecture is leaning towards a **"God Service"** pattern in the backend (`StoryState`) and **Monolithic Razor** components in the frontend. Security and testing are currently the highest risk areas due to plaintext API key storage and near-zero automated test coverage.

---

## 2. Methodology
The audit was conducted via a workspace-wide scan of `.NET 10` source code, `Blazor` components, `SQLite` schemas, and `JavaScript` interop files. Each layer was evaluated against SOLID principles, DRY (Don't Repeat Yourself), and the project's own "No Ghostwriting" and "Sovereign Engine" mandates.

---

## 3. Audit Findings

### 3.1 Architectural & Pattern Drift (High Priority)
*   **Minimal API Sprawl in `Program.cs`:**
    *   **Evidence:** Lines 60-145 of `Program.cs` contain ~80 lines of inline controller logic for `/api/notebooks` and `/api/pins`. 
    *   **Risk:** Violates Separation of Concerns. Logic that touches `AppDbContext`, `ToastService`, and `AchievementService` is mixed with startup configuration.
*   **Tight Coupling in Orchestration:**
    *   **Evidence:** `TutorOrchestrator.cs` manually instantiates `SparkPromptStrategy` and `ReviewPromptStrategy` using `new` in the constructor.
    *   **Risk:** Prevents Dependency Injection (DI) based testing and mocks.
*   **Prompt Strategy Overshadowing:**
    *   **Evidence:** `SparkPromptStrategy.cs` contains a 20+ line hardcoded string for the System Prompt.
    *   **Risk:** Changing AI personality/rules requires a full re-compile. No versioning or A/B testing capability.

### 3.2 Monolithic Components (Razor)
*   **The "Big Three" Pages:**
    *   **Editor.razor** (312 lines), **Planner.razor** (347 lines), and **Admin.razor** (300 lines).
    *   **Evidence:** Each of these files manages its own data loading, child component state, JS interop initialization, and persistence logic.
    *   **Risk:** High cognitive load for developers; fragile `OnInitializedAsync` methods with deep `.Include()` chains; difficult to unit test UI logic.
*   **Reflective Admin Risk:**
    *   **Evidence:** `Admin.razor` uses deep reflection to CRUD any entity. 
    *   **Risk:** While "utilitarian," it bypasses any business logic/validation rules defined in service layers.

### 3.3 Service Layer & Data Layer (Medium Priority)
*   **`StoryState` God Object:**
    *   **Evidence:** `StoryState.cs` manages everything from session metadata and story content to theming timers and review flags.
    *   **Risk:** Violates Single Responsibility Principle. A failure in theme persistence can potentially crash the writing session state.
*   **Entity Fragmentation:**
    *   **Evidence:** `Story.cs` contains a `Pages` collection marked as deprecated, but `AppDbContext.cs` still contains a `DbSet<StoryPage>`.
    *   **Risk:** Schema bloat and confusion for new developers.
*   **Fragile JSON Parsing:**
    *   **Evidence:** `CohereTutorService.cs` uses `JsonDocument` to manually traverse response trees instead of using strongly-typed DTOs.
    *   **Risk:** Minor API response changes from Cohere will cause runtime crashes.

### 3.4 Security & Safety (Critical Risk)
*   **Plaintext API Keys:**
    *   **Evidence:** `Account.cs` stores `CohereApiKey` as a simple string.
    *   **Risk:** Keys are stored plaintext in `StoryFort.db`. If the database is compromised, the API budget is exposed.
*   **Hardcoded Safeguards:**
    *   **Evidence:** `TutorOrchestrator.ValidateSafeguards` uses hardcoded regex for PII and Prompt Injection.
    *   **Risk:** "Ignore previous instructions" is an evolving space; regex is a brittle defense.
*   **Serilog Privacy:** 
    *   **Evidence:** `Program.cs` lacks explicit `Story.Content` filtering in the logger setup.
    *   **Risk:** Accidental logging of full student stories could violate the Canadian data sovereignty/privacy mandate.

### 3.5 Client-Side Hygiene
*   **JS Module Fragmentation:**
    *   **Evidence:** `editor.js`, `inactivity.js`, `reader.js`, and `theme.js` appear to be global scripts.
    *   **Risk:** Global namespace pollution; risk of event listener leaks during Blazor component lifecycle changes.

---

## 4. Technical Debt Analysis
| Debt Category | Severity | Description |
|---------------|----------|-------------|
| **Testing** | Critical | `StoryFort.Tests.Unit/UnitTest1.cs` is empty. The project has 0% verified code coverage. |
| **Persistence** | Medium | Overuse of JSON columns (`Metadata`, `ThemePreferenceJson`) makes queries difficult and data types opaque. |
| **Accessibility** | Low | Some components (SVG Map) lack ARIA roles and keyboard navigation labels. |
| **Infrastructure** | Low | No CI/CD pipeline (GitHub Actions) to enforce build/test gates. |

---

## 5. Summary of Recommended Remediation

1.  **Safety First:** Move `ValidateSafeguards` to a standalone `ISafeguardValidator` and add unit tests for prompt injection patterns.
2.  **API Decoupling:** Extract Minimal API logic from `Program.cs` into typed `EndpointHandlers`.
3.  **Entity Cleanup:** Perform a final migration to remove `StoryPage` and `Pages` from the schema.
4.  **Component Thinning:** Refactor `Editor.razor` code into a partial class (`Editor.razor.cs`) or a dedicated `EditorViewService`.
5.  **Secure Config:** Implement encryption-at-rest for the `CohereApiKey` or move it to a more secure configuration provider.
6.  **Prompt Management:** Move LLM system prompts into `appsettings.json` or external template files.

---
*End of Audit Report.*
