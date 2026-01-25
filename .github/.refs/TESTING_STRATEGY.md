# Testing Architecture & Strategy

## 🎯 Goal
Implement a holistic testing suite for the Unstickd (.NET 10 Blazor Server) application that validates business logic, data integrity, and end-user workflows without manual regression.

## 🏗 Structural Overview (Pyramid)

| Layer | Project Name | Scope | Tech Stack |
| :--- | :--- | :--- | :--- |
| **User Journey (E2E)** | `Unstickd.Tests.E2E` | Full browser interaction. Validates JS Interop, WebSocket connection, and User Flows. | Playwright, FluentAssertions |
| **Integration** | `Unstickd.Tests.Integration` | Database & Service boundaries. Validates EF Core queries, Schema fidelity, and Service coupling. | xUnit, SQLite (In-Memory File), FluentAssertions |
| **Unit** | `Unstickd.Tests.Unit` | Isolated Logic. Validates State Management, Algorithms, and Protocol logic. | xUnit, Moq, FluentAssertions |

---

## 1. Unit Tests (`Unstickd.Tests.Unit`)
**Focus**: Pure C# logic, State Machines, and Utilities. **Mock connections to DB and LLM.**

### Critical Scenarios
*   **TutorOrchestrator**:
    *   Verify Spark Protocol state transitions (Sensory -> Attribute -> Scenario).
    *   Verify JSON parsing logic for LLM responses.
*   **StoryState**:
    *   Verify in-memory property updates trigger `NotifyStateChanged`.
    *   Test debouncing logic (if implemented in C#).
*   **Models**:
    *   Validate default JSON metadata serialization (e.g. `Metadata = "{}"`).

### Guidelines for Agents
*   **Mocks**: Use `Moq` for `IHttpClientFactory`, `AppDbContext` (if not using Integration), and `IJSRuntime`.
*   **Speed**: These tests must run in milliseconds. No thread sleeps or I/O.

---

## 2. Integration Tests (`Unstickd.Tests.Integration`)
**Focus**: Data Persistence and EF Core relationships. **Use a real SQLite instance.**

### Critical Scenarios
*   **Notebooks & Entities**:
    *   Create `Account` -> Create `Notebook` -> Add `NotebookEntity`.
    *   Verify `Include()` chains work for deep queries.
    *   Verify cascading deletes (e.g., Deleting an Account deletes Stories).
*   **Story Persistence**:
    *   Save `Story` with Metadata. Reload from new Context. Verify JSON content matches.
*   **Migrations**:
    *   Ensure database schema creates successfully from `InitialCreate` to latest migration.

### Guidelines for Agents
*   **Database**: Use `Microsoft.Data.Sqlite` in a "Shared Cache" mode (`DataSource=file:memdb1?mode=memory&cache=shared`) to simulate real SQLite behavior without disk I/O penalties.
*   **Cleanup**: Enforce `EnsureDeleted()` / `EnsureCreated()` in the test constructor/dispose pattern to ensure hydration isolation.

---

## 3. End-to-End Tests (`Unstickd.Tests.E2E`)
**Focus**: The "Child Experience". Validates the UI assembly.

### Critical Scenarios
*   **Onboarding**:
    *   Enter DoB -> Select Theme -> Verify Editor loads with correct CSS variables.
*   **Writing & Saving**:
    *   Type in QuillJS editor -> Wait for Debounce -> Verify "Saved" badge appears.
*   **Entity Editor (Sidebar)**:
    *   Open Notebook Panel -> Click Entity -> Edit Metadata (Inline) -> Save -> Verify UI text updates.
*   **Inactivity**:
    *   Wait 15s -> Verify `TutorPanel` changes state (Visual regression or class check).

### Guidelines for Agents
*   **Playwright**: Use `Page` object model.
*   **Selectors**: Prefer `GetByRole` or `GetByText` over CSS selectors to match accessible user behavior.
*   **Waits**: Never use fixed sleeps. Use `Expect(locator).ToBeVisibleAsync()`.
*   **Host**: Assume `dotnet run` is executed by the test harness or a pre-configured `WebApplicationFactory` host.

---

## 📦 Standard Tooling
*   **Mocking**: `Moq`
*   **Assertions**: `FluentAssertions` (Use `Should().Be()`, `Should().Contain()`)
*   **Snapshots**: `Snapshooter.Xunit` (For validating complex HTML/JSON outputs)

## 🚀 Execution Strategy
*   **Local**: `dotnet test` runs Unit + Integration.
*   **E2E**: Requires `playwright install`. Run via `dotnet test --filter Category=E2E`.
