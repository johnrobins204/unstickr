# Test Specifications

This directory contains behavior specifications for StoryFort features and components. Each spec defines the **expected behavior** that tests must verify, written and approved by the developer **before** any test code is generated.

## Purpose

Specs prevent "Green Tick Drift"—where tests pass but don't actually validate correct behavior because they were written from the same flawed implementation they're supposed to test.

## Workflow

### Phase 1: Proposal & Approval (AI → Developer)
1. **AI Proposes**: "I want to test [Component]. Here's what it does and where it fits in the workflow."
2. **Developer Specs**: Developer writes the authoritative behavior requirements using the standardized format.

### Phase 2: Autonomous Execution (AI, No Developer Monitoring Required)
Once the developer approves a spec, the AI executes the following steps **autonomously**:

3. **AI Documents Spec**
   - Save spec to `specs/[component-name].md`
   - Update `specs/README.md` Current Specs section
   - Commit message: `docs: add spec for [component]`

4. **AI Generates Tests**
   - Create or replace test file: `StoryFort.Tests.Unit/[Component].Specs.cs`
   - Include spec reference in file header: `/// <summary>Spec: /specs/[component-name].md</summary>`
   - Use approved spec GIVEN-WHEN-THEN as test method names and logic
   - Tests should fail initially (Red)

5. **AI Runs Tests (Baseline)**
   - Run: `dotnet test StoryFort.Tests.Unit --filter "FullyQualifiedName~[Component].Specs" --verbosity minimal`
   - Document which tests fail and why
   - Expected: Most/all tests fail (proving we're testing behavior, not implementation)

6. **AI Implements/Fixes Code**
   - Modify production code to satisfy spec requirements
   - Make minimal changes to pass tests (Green)
   - Follow existing code style and patterns

7. **AI Runs Tests (Verification)**
   - Run full test suite: `dotnet test StoryFort.Tests.Unit --verbosity minimal`
   - Ensure new tests pass
   - Ensure existing tests still pass (no regressions)

8. **AI Generates Coverage Report**
   - Run: `dotnet test StoryFort.Tests.Unit --collect:"XPlat Code Coverage" --verbosity minimal`
   - Generate HTML: `reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:TestResults/CoverageReport -reporttypes:Html`
   - Document line/branch coverage improvements

9. **AI Updates Documentation**
   - Update `TEST_DEVELOPMENT_PLAN.md` with completed tests
   - Update `developer-diary.json` with session entry
   - Mark TODO items as completed

10. **AI Reports Completion**
    - Summary: "Completed [Component] spec-driven tests. X/Y tests pass. Coverage: +N% lines, +M% branches."
    - Report any implementation issues or spec ambiguities discovered
    - No further developer action required unless tests fail

## Spec Format

Each spec should follow this structure:

```markdown
## Spec: [Feature/Component Name]
**File:** [Path to test file]
**Type:** [Unit | Integration | E2E]
**Priority:** [Critical | High | Medium | Low]
**Estimated Tests:** [Number]

### Context
[What this component does and where it fits in the application workflow]

### Behavior Requirements

#### Happy Path
- **GIVEN** [Initial state]
- **WHEN** [Action]
- **THEN** [Expected outcome]

#### Edge Cases
- **GIVEN** [Edge condition]
- **WHEN** [Action]
- **THEN** [Expected handling]

#### Error Cases
- **GIVEN** [Error condition]
- **WHEN** [Action]
- **THEN** [Expected error handling]

### Non-Functional Requirements
- Performance: [Metrics]
- Security: [Requirements]
- Resilience: [Handling]

### Test Data Examples
[Concrete input/output examples]

### Success Criteria
- [ ] All behaviors covered
- [ ] Edge cases handled
- [ ] Errors are user-friendly
- [ ] No security vulnerabilities
```

## Current Specs

### Active
- [texttokenizer.md](texttokenizer.md) - TextTokenizer behavior spec (Approved)
- [archetype-service.md](archetype-service.md) - ArchetypeService null/edge cases (Awaiting developer approval)

### Completed
- [safeguard-service.md](safeguard-service.md) - Input Guardrails (Approved: Feb 2, 2026, Tests: 8/8 pass, Coverage: ✅)
- [cohere-tutor-service.md](cohere-tutor-service.md) - LLM API Integration (Approved: Feb 2, 2026, Tests: 7/7 pass, Coverage: ✅)
- [sessionstate-resilience.md](sessionstate-resilience.md) - Session Error Handling (Approved: Feb 2, 2026, Tests: 4/4 pass, Coverage: ✅)
- [texttokenizer.md](texttokenizer.md) - Token Replacement & HTML Escaping (Approved: Feb 2, 2026, Tests: 3/3 pass, Coverage: ✅)
- [texttokenizer-sanitization.md](texttokenizer-sanitization.md) - Editor → Persistence Sanitization (Approved: Feb 2, 2026, Tests: 2/2 pass, Coverage: ✅)
- [storypersistence-concurrency.md](storypersistence-concurrency.md) - Last-Write-Wins Concurrency (Approved: Feb 2, 2026, Tests: 4/4 pass, Coverage: ✅)

### Archive
_None yet._

## Audit Status

**Original Audit:** [AUDIT_GREEN_TICK_DRIFT.md](AUDIT_GREEN_TICK_DRIFT.md)

**Remediation Progress:**
- ✅ SafeguardService: Rewritten with realistic attack patterns (was 🔴 risk)
- ✅ CohereTutorService: Created from empty file (was 🔴 CRITICAL risk)
- ✅ SessionState: Added error resilience tests (was 🔴 missing edge cases)
- 🟡 TextTokenizer: Still has 🟡 moderate drift (implementation mirror tests)
- 🟡 SparkPromptStrategy: Still has 🟡 minor drift (mock-based validation)

**Next Priority From Audit:**
1. TextTokenizer behavior spec (Pattern 1: Implementation Mirror)
2. ArchetypeService edge cases (null handling, empty results)
3. StoryPersistenceService concurrency tests

## AI Execution Checklist

After receiving an **approved spec**, the AI must complete these steps autonomously:

- [ ] Save spec to `specs/[component-name].md`
- [ ] Update `specs/README.md` Current Specs section
- [ ] Create test file with spec reference header
- [ ] Run tests (expect failures - Red)
- [ ] Implement code to pass tests (Green)
- [ ] Run full test suite (verify no regressions)
- [ ] Generate coverage report and document improvements
- [ ] Update TEST_DEVELOPMENT_PLAN.md with ✅ checkmarks
- [ ] Update developer-diary.json with session entry
- [ ] Report completion with summary metrics

**Success Criteria:**
- All tests pass (green)
- No existing tests broken
- Coverage improved (line/branch %)
- Documentation updated
- Developer can review git diff without needing to monitor execution

## Spec Maintenance

- **Update specs** when requirements change (not when implementation changes)
- **Archive specs** for removed features to `specs/archive/`
- **Link specs** in test file headers: `/// <summary>Spec: /specs/component-name.md</summary>`
