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
*   [x] **Minimal API Sprawl in `Program.cs` (REMEDIATED):**
    *   **Evidence:** Previously contained ~80 lines of inline controller logic.
    *   **Status:** Logic extracted to `StoryEndpoints.cs` using `app.MapStoryEndpoints()`.
*   [x] **Tight Coupling in Orchestration (REMEDIATED):**
    *   **Evidence:** `TutorOrchestrator.cs` manually instantiated strategies.
    *   **Status:** Refactored to use Dependency Injection for `SparkPromptStrategy` and `ReviewPromptStrategy`.
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

*   [/] **Persistence Encapsulation (PARTIALLY REMEDIATED):**
    * **Evidence:** A new `StoryPersistenceService` was introduced to centralize story load/save functionality; `Editor` and `Planner` now use this service for persistence operations.
    * **Status:** REMEDIATED — `StoryPersistenceService` now centralizes story and notebook persistence; components updated to use the service.

### 3.3 Service Layer & Data Layer (Medium Priority)
*   [x] **`StoryState` God Object (REMEDIATED):**
    *   **Evidence:** `StoryState.cs` managed everything from session metadata and story content to theming timers and review flags.
    *   **Status:** REMEDIATED — `StoryState` deleted. Replaced by decoupled `SessionState` (transient UI) and `StoryContext` (domain state) services.
*   [x] **Entity Fragmentation (REMEDIATED):**
    *   **Evidence:** Previously contained deprecated `Pages` collection and `StoryPage` entity.
    *   **Status:** Removed `StoryPage` model, updated `Story.cs`, and dropped the database table via migration.
*   [x] **Fragile JSON Parsing (REMEDIATED):**
    *   **Evidence:** `CohereTutorService.cs` used manual `JsonDocument` traversal.
    *   **Status:** Implemented strongly-typed DTOs for Cohere API responses.

### 3.4 Security & Safety (Critical Risk)
*   **Plaintext API Keys:**
    *   **Evidence:** `Account.cs` stores `CohereApiKey` as a simple string.
    *   **Status:** REMEDIATED — Plaintext property removed. `ProtectedCohereApiKey` used with `IApiKeyProtector` (AES encryption).
*   [/] **Hardcoded Safeguards (PARTIALLY REMEDIATED):**
    *   **Evidence:** `TutorOrchestrator.ValidateSafeguards` extracted to `ISafeguardService`.
    *   **Status:** Logic moved to dedicated service, but still primarily relies on regex patterns.
*   [x] **Encryption at Rest (REMEDIATED):**
    *   **Evidence:** `Story.Content`, `NotebookEntry.Content`, and `NotebookEntity.Description` were plaintext in SQLite.
    *   **Status:** REMEDIATED — Implemented `IStoryContentProtector` using Microsoft Data Protection and EF Core Value Converters to ensure all sensitive text is encrypted at rest automatically.
*   **Serilog Privacy:** 
    *   **Evidence:** `Program.cs` lacks explicit `Story.Content` filtering in the logger setup.
    *   **Status:** PARTIALLY REMEDIATED — added a `RedactEnricher` to redact known sensitive properties from logs; avoid logging full model objects.

### 3.5 Client-Side Hygiene
*   **JS Module Fragmentation:**
    *   **Evidence:** `editor.js`, `inactivity.js`, `reader.js`, and `theme.js` appear to be global scripts.
    *   **Risk:** Global namespace pollution; risk of event listener leaks during Blazor component lifecycle changes.

---

## 4. Technical Debt Analysis
| Debt Category | Severity | Description |
|---------------|----------|-------------|
| **Testing** | High | Baseline E2E and Unit tests established and passing, but functional coverage remains low (<10%). |
| **Persistence** | Medium | Overuse of JSON columns (`Metadata`, `ThemePreferenceJson`) makes queries difficult and data types opaque. |
| **Accessibility** | Low | Some components (SVG Map) lack ARIA roles and keyboard navigation labels. |
| **Infrastructure** | Low | CI workflow scaffolded (GitHub Actions) to run build/tests; consider adding gated E2E run and formal releases. |

---

## 5. Summary of Recommended Remediation

1.  [x] **Safety First:** Move `ValidateSafeguards` to a standalone `ISafeguardValidator`. (Done: `ISafeguardService`)
2.  [x] **API Decoupling:** Extract Minimal API logic from `Program.cs` into typed `EndpointHandlers`. (Done: `StoryEndpoints.cs`)
3.  [x] **Entity Cleanup:** Perform a final migration to remove `StoryPage` and `Pages` from the schema. (Done: `RemoveStoryPageEntity` migration)
4.  [x] **Component Thinning (REMEDIATED):** Refactor `Editor.razor` code into a partial class (`Editor.razor.cs`) or a dedicated `EditorViewService`.
    * **Evidence:** `Editor.razor` and `Planner.razor` logic extracted into `Editor.razor.cs` and `Planner.razor.cs` respectively; persistence logic centralized in `StoryPersistenceService`.
5.  [x] **Secure Config (REMEDIATED):** Implement encryption-at-rest for the `CohereApiKey` or move it to a more secure configuration provider. (Done: `IApiKeyProtector`, `AccountApiKeyHistory` table, and `ProtectedCohereApiKey` column.)
6.  [x] **Prompt Management (REMEDIATED):** Move LLM system prompts into `appsettings.json` or external template files. (Done: `PromptRepository`, `SparkPromptStrategy` uses `Prompts/spark_v1.txt`.)

---
## Progress Update (2026-01-31)

Recent remediations completed since the initial audit:

- **Security Hardening**: `Account.CohereApiKey` is now encrypted at rest. The `AccountApiKeyHistory` table tracks key rotation.
- **Tutor Refactor**: `TutorSession` logic extracted to `TutorSessionService`. `TutorOrchestrator` now uses the Strategy pattern (`SparkPromptStrategy`) to improve testability.
- **Prompt Externalization**: System prompts are loaded from `Prompts/spark_v1.txt` via a new `PromptRepository`.
- **Testing**: Unit, Integration, and E2E tests are passing. E2E deadlocks were resolved.
- **Entity Cleanup**: `StoryPage` was removed and the schema migrated.

Outstanding, recommended actions:

- **CI/CD**: Implement a GitHub Actions workflow to automate testing.
- **Admin UI**: Add a frontend interface for the API Key rotation endpoints.
- **StoryState**: Continue decomposing `StoryState` if it grows further (currently manageable).

---
*End of Audit Report.*

---
## Post-Audit Actions Taken (summary)

- Component thinning: `Editor.razor` and `Planner.razor` code moved to partial classes and helpers; `StoryPersistenceService` centralizes persistence.
- Tutor refactor: extracted `TutorSessionService`, delegated `StoryState.TutorSession` to it, and updated orchestrator and prompt strategies to use the service.
- Prompt externalization: added `Prompts/spark_v1.txt`, `PromptRepository`, and wired file-based prompt loading into `SparkPromptStrategy` with `appsettings.json` fallback.
- Key protection: added `IApiKeyProtector`/`ApiKeyProtector`, `ProtectedCohereApiKey` on `Account`, admin endpoint for rotation, and `AccountApiKeyHistory` migration.
- Tests & CI: added unit, integration, and E2E scaffolding; CI workflow scaffolding added; integration and unit tests executed locally and passed.

## Remaining Actions Before Merge

- Confirm and run E2E tests in CI (optional gating).
- Finalize `AUDIT_REPORT.md` and `developer-diary.json` (this file) with PR notes.
- Open PR with the changes, assign reviewers, and run CI on the PR.

