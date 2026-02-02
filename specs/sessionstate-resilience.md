## Spec: SessionState - Resilience & Error Paths
**File:** StoryFort.Tests.Integration/SessionState_Resilience_Specs.cs
**Type:** Integration (in-process DB + scoped services) + Unit (pure function behaviors)
**Priority:** High
**Estimated Tests:** 6-8

### Context
`SessionState.UpdateThemePreference` applies a UI change immediately and persists the change asynchronously via `ThemeService.SaveThemePreferenceAsync` using an `IServiceScopeFactory`. The method is fire-and-forget and must not throw to the UI thread even if persistence fails. Concurrent updates may occur from multiple UI components.

### Behavior Requirements

#### Happy Path
- **GIVEN** a valid `IServiceScopeFactory` and `Account` exists in DB
- **WHEN** `UpdateThemePreference` is called to change `FontSize`
- **THEN** `ThemePreference` is updated in-memory immediately and eventually persisted to DB

#### Database Failure Handling
- **GIVEN** `ThemeService.SaveThemePreferenceAsync` throws (DB unavailable)
- **WHEN** `UpdateThemePreference` is called
- **THEN** the UI update still completes, no exception propagates to caller, and the error is logged

#### Missing ScopeFactory (No Persistence)
- **GIVEN** `SessionState` constructed without `IServiceScopeFactory`
- **WHEN** `UpdateThemePreference` is called with an `Account`
- **THEN** in-memory `ThemePreference` updates and method returns without attempting persistence

#### Concurrent Updates (Last-Write Wins)
- **GIVEN** two concurrent calls to `UpdateThemePreference` that change overlapping fields
- **WHEN** both complete
- **THEN** the `ThemePreference` reflects the last applied update (deterministic ordering for test: apply A then B)

#### Background Persistence Timing
- **GIVEN** a persistence call is scheduled
- **WHEN** the persistence completes successfully
- **THEN** the DB record for the `Account` contains the serialized `ThemePreference` matching the in-memory state

### Non-Functional Requirements
- Resilience: No unhandled exceptions from `UpdateThemePreference` into UI
- Observability: Errors should be logged via `Serilog` (test verifies logger was called)
- Performance: Persistence is asynchronous; UI update must not block (test uses controlled synchronization)

### Test Data Examples
- Account seeded with `Id=100`, initial `ThemePreference.FontSize=12`
- Update A: set FontSize=16
- Update B: set FontSize=18

### Success Criteria
- [ ] In-memory updates occur immediately
- [ ] No exceptions bubble to caller on persistence failures
- [ ] Concurrent updates are deterministic (last-write wins)
- [ ] Persistence writes correct JSON to DB when scope factory present

### Notes for AI Execution
- Use `UseSqlite("DataSource=:memory:")` and `EnsureCreated()` for integration tests
- Mock `ThemeService` failure using a scoped service override or a test double
- Capture logs with a test Serilog sink or `ILogger<T>` mock
