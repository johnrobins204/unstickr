# StoryFort Codebase Audit Plan

**Purpose:** Identify pattern drift, monolithic tendencies, overly complex code, and early-stage refactor opportunities across the entire codebase. This plan must be approved before execution.

---

## Audit Scope

| Layer | Files / Folders | Notes |
|-------|-----------------|-------|
| **Startup & DI** | `Program.cs` | Service registrations, middleware ordering, inline Minimal API endpoints |
| **Data Layer** | `Data/AppDbContext.cs`, `Models/*.cs`, `Migrations/` | Seed data sprawl, deprecated entities (`StoryPage`), JSON columns |
| **Services** | `Services/*.cs` | Strategy pattern usage, orchestration complexity, DI lifetime issues |
| **Components (Razor)** | `Components/**/*.razor` | Monolithic pages, code-behind separation, accessibility, CSS scope |
| **Client JS** | `wwwroot/js/*.js` | Interop surface, event-listener leakage, scope isolation |
| **Tests** | `StoryFort.Tests.*` | Coverage gaps, placeholder tests, Playwright stability |
| **Docs & Governance** | `.github/.refs/**` | Stale links, missing architecture docs |

---

## Audit Categories & Checklists

### 1. Architectural & Pattern Drift
| Check | Risk | Files |
|-------|------|-------|
| **Inline Minimal API in Program.cs** | Violates layer separation; hard to test | `Program.cs` L60-145 |
| **Duplicate DB registration** | Commented line may confuse devs | `Program.cs` L52 |
| **TutorOrchestrator creates strategies inline** | Tight coupling; prevents DI-based testing | `TutorOrchestrator.cs` constructor |
| **SparkPromptStrategy builds hardcoded mega-prompt** | Prompt logic mixed with orchestration | `SparkPromptStrategy.cs` L56-71 |
| **StoryState has too many responsibilities** | Session, persistence, theming, review tokens | `StoryState.cs` |

### 2. Monolithic Components (Razor)
| Check | Risk | Files |
|-------|------|-------|
| **Editor.razor > 300 LOC** | Hard to unit-test; mixes UI + data loading + JS interop | `Editor.razor` |
| **OnInitializedAsync mega-query** | Multiple `.Include()` chains; N+1 risk | `Editor.razor` L172-214 |
| **No code-behind pattern** | Business logic embedded in markup | Most `.razor` files |
| **Hardcoded CSS utility classes** | Style drift from design tokens | Various |

### 3. Data Layer Smells
| Check | Risk | Files |
|-------|------|-------|
| **StoryPage still in model & DB** | DEPRECATED but seeded/migrated; confusing | `Story.cs`, `AppDbContext.cs` |
| **Massive seed data in OnModelCreating** | 100+ LOC of seed; consider external JSON | `AppDbContext.cs` |
| **JSON blobs (Metadata, ThemePreferenceJson)** | No schema validation; hard to query | `Story.cs`, `Account.cs` |
| **Composite keys (StoryEntityLink)** | OK, but consider surrogate if scale | `AppDbContext.cs` |

### 4. Service Layer Smells
| Check | Risk | Files |
|-------|------|-------|
| **ICohereTutorService has only one impl** | Fine for now; watch for mock gaps in tests | `ICohereTutorService.cs` |
| **CohereTutorService JSON parsing in-line** | Fragile; no DTO classes | `ICohereTutorService.cs` L28-60 |
| **ValidateSafeguards is private** | Can't unit-test independently | `TutorOrchestrator.cs` |
| **Hard-coded regex in ValidateSafeguards** | Move patterns to config or constants | `TutorOrchestrator.cs` |

### 5. Client JS & Interop
| Check | Risk | Files |
|-------|------|-------|
| **Multiple JS files, unclear module boundaries** | `editor.js`, `inactivity.js`, `reader.js`, `theme.js` | `wwwroot/js/` |
| **Global namespace pollution** | Functions attached to `window` | Various |
| **No TypeScript / no type safety** | Increased bug surface | All `.js` |
| **DotNetObjectReference lifecycle** | Leak risk if not disposed | `Editor.razor` |

### 6. Testing & CI
| Check | Risk | Files |
|-------|------|-------|
| **Placeholder unit test** | Zero coverage | `UnitTest1.cs` |
| **E2E flakiness noted in diary** | Playwright hover retries | `StoryFort.Tests.E2E` |
| **No integration tests for services** | No DB or API mocking | `StoryFort.Tests.Integration` |
| **No CI pipeline defined** | No GitHub Actions / DevOps YAML | `.github/` |

### 7. Security & Governance
| Check | Risk | Files |
|-------|------|-------|
| **API key stored in Account plaintext** | Should be encrypted or user-secret | `Account.cs` |
| **Supervisor PIN / Teacher Gate not implemented** | Documented but missing code | Docs only |
| **Serilog filter for Story.Content** | Need to verify exclusion | `Program.cs` or Serilog config |
| **SQLite encryption at rest** | Documented; need to verify in code | `Program.cs` |

---

## Proposed Refactors (Priority Order)

| # | Refactor | Effort | Impact |
|---|----------|--------|--------|
| 1 | Extract Minimal API endpoints to a `/Api` folder or Controllers | S | Separation, testability |
| 2 | Move seed data to JSON file + `DbContext` extension | S | Readability, maintainability |
| 3 | Split `StoryState` into `SessionState`, `ThemeState`, `ReviewState` | M | SRP, testability |
| 4 | Inject `IPromptStrategy` via DI instead of `new` | S | Testability |
| 5 | Extract `Editor.razor` data-loading to a `StoryLoader` service | M | Thin component |
| 6 | Make `ValidateSafeguards` public or extract to `ISafeguardValidator` | S | Testability |
| 7 | Remove `StoryPage` entity and migration if truly deprecated | S | Reduce confusion |
| 8 | Create DTO classes for Cohere responses | S | Type safety |
| 9 | Modularize JS with ES modules or bundler | M | Maintainability |
| 10 | Scaffold real unit/integration tests for `TutorOrchestrator`, `StoryState` | M | Confidence |
| 11 | Add GitHub Actions CI for build + test | S | Automation |
| 12 | Encrypt or externalize API key storage | M | Security |

---

## Execution Plan (If Approved)

1. **Phase 1 — Quick Wins (Effort S):** Items 1, 2, 4, 6, 7, 8, 11
2. **Phase 2 — Medium Refactors (Effort M):** Items 3, 5, 9, 10, 12
3. **Phase 3 — Full Test Harness & CI:** Expand tests, add Playwright stability fixes, add coverage gate

Each phase concludes with a build verification and updated `developer-diary.json` entry.

---

**Awaiting your approval to proceed with Phase 1.**
