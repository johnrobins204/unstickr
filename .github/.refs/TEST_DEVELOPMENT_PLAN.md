# Test Development Plan - Wave 2
**Date:** February 1, 2026  
**Updated:** February 2, 2026 (Spec-Driven Pivot)  
**Current Coverage:** 81.1% (11,158/13,744 lines)  
**Branch Coverage:** 16.4% (183/1,111 branches)  
**Method Coverage:** 36.9% (200/541 methods)

**Target Coverage:** 90%+ line coverage, 50%+ branch coverage  
**Estimated Effort:** 3-4 sprints

**Status Update (Phase 4):** Phases 1-4 complete (Feb 1, 2026). Implemented and committed unit tests for AI integration layer (CohereTutorService, PromptService, ReviewPromptStrategy), business logic (ArchetypeService, SessionState, TextTokenizer, ThemeService), helper services (ReaderHtmlHelper, AchievementService), and model/data layer (ModelValidationTests, ApiKeyProtectorTests, NotebookEntityTests). Integration tests added for ArchetypeService. All 39 tests pass locally (34 unit + 5 integration).

**Methodology Pivot (Feb 2, 2026):** Adopting spec-driven testing to prevent "Green Tick Drift" (tests that pass but prove nothing). All new tests will follow the Red-Agent-Green loop with developer-approved behavior specs.

---

## Spec-Driven Testing Methodology (2026 Standard)

### The Problem: Green Tick Drift
Tests written by the same agent that wrote the production code risk validating implementation details rather than actual behavior. This creates false confidence.

### The Solution: Human-Approved Specs
Before writing any test, the AI must:
1. **Explain** what is being tested and where it fits in the workflow
2. **Request** the authoritative spec from the developer
3. **Document** the spec in standardized format
4. **Generate** failing tests based on the spec
5. **Implement** code to pass the tests

### Standardized Spec Format

```markdown
## Spec: [Feature/Component Name]
**File:** [Path to test file]
**Type:** [Unit | Integration | E2E]
**Priority:** [Critical | High | Medium | Low]
**Estimated Tests:** [Number]

### Context
[Brief description of what this component does and where it fits in the application workflow]

### Behavior Requirements

#### Happy Path
- **GIVEN** [Initial state/precondition]
- **WHEN** [Action performed]
- **THEN** [Expected outcome]

#### Edge Cases
- **GIVEN** [Edge condition]
- **WHEN** [Action performed]
- **THEN** [Expected handling]

#### Error Cases
- **GIVEN** [Error condition]
- **WHEN** [Action performed]
- **THEN** [Expected error handling]

### Non-Functional Requirements
- Performance: [Response time, throughput]
- Security: [Auth, validation, sanitization]
- Resilience: [Retry logic, fallbacks]

### Test Data Examples
```
[Concrete examples of input/output]
```

### Success Criteria
- [ ] All behavior requirements covered
- [ ] Edge cases handled gracefully
- [ ] Error messages are user-friendly
- [ ] No security vulnerabilities
```

### Workflow for New Tests

1. **AI Proposes Test Target**
   - "I want to test [Component]. This component [brief description]. It fits into the workflow at [landmark location]. It interacts with [dependencies]."
   
2. **Developer Provides Authoritative Spec**
   - Developer writes or approves the behavior requirements using the standardized format above
   
3. **AI Documents Spec**
   - Spec is saved to `/specs/[component-name].md`
   
4. **AI Generates Failing Tests**
   - Tests are written to match the spec, not the current implementation
   - Tests run and fail (Red)
   
5. **AI Implements Code**
   - Code is written/fixed to pass the tests (Green)
   
6. **AI Refactors**
   - Code is cleaned up while tests remain green (Refactor)

### Test Type Guidelines

**Favor Integration Tests for:**
- Workflows involving multiple services
- Database interactions
- State management
- API contracts

**Use Unit Tests for:**
- Pure functions (no I/O, no state)
- Complex algorithms
- Validation logic
- Utility helpers

**Reserve E2E Tests for:**
- Critical user journeys (3-5 max)
- Cross-cutting concerns (auth, security)
- Accessibility requirements

---

## Executive Summary

While we have strong coverage (79%) on core security and data persistence services, significant gaps exist in:
1. **AI/LLM Integration Layer** (0% coverage)
2. **UI Components** (Blazor Pages: 0% coverage)
3. **Business Logic Services** (ArchetypeService, SessionState: 0-27%)
4. **Helper/Utility Services** (TextTokenizer, ReaderHtmlHelper: 0%)

This plan prioritizes **high-risk, high-value** areas first, following the testing pyramid principle.

---

## Coverage Gap Analysis

### 🔴 **Critical Gaps (0% Coverage)**

| Service/Component | Current | Priority | Risk Impact |
|-------------------|---------|----------|-------------|
| CohereTutorService | ✅ TESTED | **CRITICAL** | Core AI integration; failure breaks tutor |
| ArchetypeService | ✅ TESTED | **HIGH** | Planner feature; data integrity risk |
| SessionState | ✅ TESTED | **HIGH** | Session management; data loss risk |
| PromptService | ✅ TESTED | **HIGH** | AI prompt generation; quality risk |
| TextTokenizer | ✅ TESTED | **MEDIUM** | Token counting; cost overrun risk |
| ReviewPromptStrategy | ✅ TESTED | **MEDIUM** | Review mode functionality |
| ThemeService | ✅ TESTED | **LOW** | Theme persistence; cosmetic |
| AchievementService | ✅ TESTED | **LOW** | Badge system; non-critical |

### 🟡 **Partial Coverage (< 50%)**

| Service/Component | Current | Gap |
|-------------------|---------|-----|
| StoryContext | 60% | Need error path testing |
| StoryEntityLink | 75% | Need cascade delete tests |
| Account Model | 47.6% | Need validation tests |
| Story Model | 45% | Need metadata serialization tests |
| ThemePreference | 28.5% | Need JSON parsing tests |

### ⚪ **UI Components (0% Coverage)**

All Blazor pages have 0% coverage. This is expected but should be addressed with E2E tests.

---

## Wave 2 Test Plan (Prioritized)

### **Phase 1: Critical AI Integration (Sprint 1)**

#### 1.1 CohereTutorService Tests
**File:** `StoryFort.Tests.Unit/CohereTutorServiceTests.cs`  
**Target Coverage:** 80%+

**Test Scenarios:**
- ✅ `SendPromptAsync_WithValidApiKey_ReturnsResponse`
  - Mock HttpClient to return valid Cohere JSON
  - Assert response parsing works correctly
  
- ✅ `SendPromptAsync_WithInvalidApiKey_ThrowsUnauthorizedException`
  - Mock 401 response
  - Verify exception handling
  
- ✅ `SendPromptAsync_WithNetworkTimeout_RetriesAndFails`
  - Mock timeout scenario
  - Verify retry logic (if implemented)
  
- ✅ `SendPromptAsync_WithMalformedResponse_HandlesGracefully`
  - Mock invalid JSON response
  - Verify error handling
  
- ✅ `SendPromptAsync_IncludesNonTrainingHeader`
  - Verify `X-Client-Name: StoryFort` header present
  - Verify no training opt-out flag
  
- ✅ `ParseChatResponse_WithReasoningModel_ExtractsBothFields`
  - Test parsing of Command R+ response (thought + text)
  
- ✅ `ParseChatResponse_WithChatModel_ExtractsTextOnly`
  - Test parsing of Command Light response

**Mock Strategy:**
```csharp
var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
mockHttpMessageHandler.Protected()
    .Setup<Task<HttpResponseMessage>>("SendAsync", ...)
    .ReturnsAsync(new HttpResponseMessage { ... });
```

---

#### 1.2 PromptService Tests
**File:** `StoryFort.Tests.Unit/PromptServiceTests.cs`  
**Target Coverage:** 85%+

**Test Scenarios:**
- ✅ `BuildPrompt_InjectsContextVariables`
  - Verify {{Age}}, {{Genre}}, {{Archetype}} replacement
  
- ✅ `BuildPrompt_HandlesEmptyTemplate`
  - Edge case: missing template file
  
- ✅ `BuildPrompt_EscapesHtmlInContext`
  - Security: ensure user input is sanitized
  
- ✅ `LoadPromptTemplate_CachesAfterFirstLoad`
  - Performance: verify PromptRepository caching

---

#### 1.3 ReviewPromptStrategy Tests
**File:** `StoryFort.Tests.Unit/ReviewPromptStrategyTests.cs`  
**Target Coverage:** 80%+

**Test Scenarios:**
- ✅ `BuildPromptAsync_ForGrammarReview_IncludesGrammarContext`
- ✅ `BuildPromptAsync_ForStyleReview_IncludesStyleContext`
- ✅ `BuildPromptAsync_ExcludesFullStoryContent`
  - Security: verify only relevant excerpt sent

---

### **Phase 2: Business Logic Services (Sprint 1-2)**

#### 2.1 ArchetypeService Tests
**File:** `StoryFort.Tests.Unit/ArchetypeServiceTests.cs`  
**Target Coverage:** 75%+

**Test Scenarios:**
- ✅ `GetArchetypes_ReturnsAllArchetypes`
  - Verify singleton caching works
  
- ✅ `GetArchetypeById_WithValidId_ReturnsArchetype`
  - Test "hero", "quest", "transformation" lookups
  
- ✅ `GetArchetypeById_WithInvalidId_ReturnsNull`
  - Edge case handling
  
- ✅ `GetArchetypePoints_IncludesExamples`
  - Verify EF Include() navigation properties
  
- ✅ `GetArchetypePoints_OrderedByStepId`
  - Data integrity: verify points returned in sequence

**Integration Test:**
**File:** `StoryFort.Tests.Integration/ArchetypeServiceIntegrationTests.cs`

- ✅ `GetArchetypes_LoadsFromSeededDatabase`
  - Verify 3 archetypes exist after seed
  
- ✅ `GetArchetypePoints_LoadsAllExamples`
  - Verify 16 plot points with examples

---

#### 2.2 SessionState Tests
**File:** `StoryFort.Tests.Unit/SessionStateTests.cs`  
**Target Coverage:** 80%+

**Test Scenarios:**
- ✅ `NotifyStateChanged_TriggersOnChangeEvent`
  - Event firing verification
  
- ✅ `UpdateThemePreference_PersistsToDatabase`
  - Mock ThemeService interaction
  
- ✅ `LoadNotebooks_CachesInMemory`
  - Verify session caching behavior
  
- ✅ `NotifyStateChanged_WithNoSubscribers_DoesNotThrow`
  - Null reference protection

---

#### 2.3 TextTokenizer Tests
**File:** `StoryFort.Tests.Unit/TextTokenizerTests.cs`  
**Target Coverage:** 90%+

**Test Scenarios:**
- ✅ `EstimateTokens_ForSimpleText_ReturnsApproximateCount`
  - "Hello world" → ~2 tokens
  
- ✅ `EstimateTokens_ForLongStory_UsesChunkingLogic`
  - 10,000 word story → ~13,000 tokens
  
- ✅ `EstimateTokens_HandlesEmptyString`
  - Edge case: "" → 0
  
- ✅ `EstimateTokens_HandlesUnicodeCharacters`
  - Test emoji, accents: "café ☕" → proper count

---

### **Phase 3: Helper & Utility Services (Sprint 2)**

#### 3.1 ThemeService Tests
**File:** `StoryFort.Tests.Unit/ThemeServiceTests.cs`  
**Target Coverage:** 75%+

**Test Scenarios:**
- ✅ `SaveThemePreference_SerializesToJson`
- ✅ `LoadThemePreference_DeserializesFromAccount`
- ✅ `SaveThemePreference_HandlesNullAccount_ThrowsException`

---

#### 3.2 ReaderHtmlHelper Tests
**File:** `StoryFort.Tests.Unit/ReaderHtmlHelperTests.cs`  
**Target Coverage:** 85%+

**Test Scenarios:**
- ✅ `SplitIntoPages_ByParagraphs_ReturnsArray`
  - Input: "<p>One</p><p>Two</p>" → ["One", "Two"]
  
- ✅ `SplitIntoPages_HandlesQuillDeltaFormat`
  - Test actual QuillJS HTML output
  
- ✅ `SplitIntoPages_PreservesFormatting`
  - Verify bold, italic, links intact

---

#### 3.3 AchievementService Tests
**File:** `StoryFort.Tests.Unit/AchievementServiceTests.cs`  
**Target Coverage:** 70%+

**Test Scenarios:**
- ✅ `CheckAchievement_FirstDraft_UnlocksWhenStoryReaches100Words`
- ✅ `CheckAchievement_DailyWriter_UnlocksAfter7ConsecutiveDays`
- ✅ `GetUnlockedAchievements_ReturnsListForAccount`

---

### **Phase 4: Model & Data Layer (Sprint 2-3)**

#### 4.1 Model Validation Tests
**File:** `StoryFort.Tests.Unit/ModelValidationTests.cs`  
**Target Coverage:** 90%+

**Test Scenarios:**
- ✅ `Story_MetadataMap_SerializesAndDeserializes`
  - Test JSON round-trip
  
- ✅ `Account_ProtectedCohereApiKey_EncryptsAndDecrypts`
  - Test encryption at rest
  
- ✅ `NotebookEntity_Metadata_HandlesInvalidJson`
  - Resilience testing
  
- ✅ `ThemePreference_DefaultValues_AreValid`
  - Constructor testing

---

#### 4.2 Enhanced SafeguardService Tests
**File:** `StoryFort.Tests.Unit/SafeguardServiceTests.cs` (expand existing)  
**Target Coverage:** 90%+

**New Scenarios:**
- ✅ `ValidateSafeguards_WithBannedWord_ReturnsFalse`
  - Test G-02 implementation
  
- ✅ `ValidateSafeguards_WithMultipleBannedWords_ReturnsFirstMatch`
  - Test multiple regex patterns
  
- ✅ `ValidateSafeguards_CaseInsensitiveMatching`
  - "BADWORD" and "badword" both blocked
  
- ✅ `ValidateSafeguards_WordBoundaries_AvoidFalsePositives`
  - "assassin" should not trigger "ass" filter

---

### **Phase 5: Integration Tests (Sprint 3)**

#### 5.1 End-to-End Tutor Flow
**File:** `StoryFort.Tests.Integration/TutorFlowIntegrationTests.cs`

**Test Scenarios:**
- ✅ `SparkProtocol_WithEmptyStory_ReturnsOpeningQuestion`
  - Full orchestrator → service → mock API → response
  
- ✅ `SparkProtocol_WithPausedWriting_ReturnsContextualPrompt`
  - Verify context window logic
  
- ✅ `ReviewMode_WithGrammarRequest_ReturnsGrammarFeedback`
  - Test review strategy integration

---

#### 5.2 Archetype Planner Integration
**File:** `StoryFort.Tests.Integration/PlannerIntegrationTests.cs`

**Test Scenarios:**
- ✅ `SelectArchetype_LoadsAllPlotPoints`
- ✅ `SavePlotPointNotes_PersistsToStoryMetadata`
- ✅ `ChangeArchetype_RetainsExistingNotes`

---

#### 5.3 Theme & Notebook Persistence
**File:** `StoryFort.Tests.Integration/ThemeAndNotebookTests.cs`

**Test Scenarios:**
- ✅ `UpdateThemePreference_SavesJsonToAccount`
- ✅ `CreateNotebookEntity_WithMetadata_RoundTrips`
- ✅ `LinkEntityToStory_CreatesStoryEntityLink`

---

### **Phase 6: E2E Playwright Tests (Sprint 3-4)**

#### 6.1 Expand E2E Coverage
**File:** `StoryFort.Tests.E2E/E2ETests.cs` (expand existing)

**Current:** 1 smoke test  
**Target:** 10+ user journey tests

**New Scenarios:**
- ✅ `UserJourney_CreateStory_WriteParagraph_SavesAutomatically`
  - Test auto-save debounce
  
- ✅ `UserJourney_OpenPlanner_SelectArchetype_AddNotes`
  - Test planner workflow
  
- ✅ `UserJourney_OpenSettings_RequiresTeacherPin`
  - Test G-01 (Teacher Gate)
  
- ✅ `UserJourney_UseTutor_GetsSocraticQuestion`
  - Test AI interaction (mock Cohere endpoint)
  
- ✅ `UserJourney_CreateCharacter_LinkToStory`
  - Test notebook system
  
- ✅ `UserJourney_ExportStory_GeneratesHtml`
  - Test reader/export
  
- ✅ `UserJourney_WriteInappropriateWord_GetsSafeguardMessage`
  - Test G-02 (Input Guardrails)

**Accessibility Tests:**
- ✅ `Accessibility_EditorPage_HasProperAriaLabels`
- ✅ `Accessibility_NavigationMenu_KeyboardNavigable`

---

## Test Infrastructure Improvements

### 1. **Shared Test Fixtures**
**File:** `StoryFort.Tests.Shared/TestFixtures.cs`

Create reusable fixtures:
```csharp
public class MockCohereApiFixture
{
    public Mock<ICohereTutorService> MockService { get; }
    public void SetupSuccessResponse(string response) { ... }
    public void SetupErrorResponse(int statusCode) { ... }
}

public class InMemoryDbFixture : IDisposable
{
    public AppDbContext CreateContext() { ... }
}
```

### 2. **Test Data Builders**
**File:** `StoryFort.Tests.Shared/Builders/`

```csharp
public class StoryBuilder
{
    public Story WithTitle(string title) { ... }
    public Story WithContent(string content) { ... }
    public Story Build() { ... }
}

public class AccountBuilder { ... }
public class NotebookBuilder { ... }
```

### 3. **Custom Assertions**
**File:** `StoryFort.Tests.Shared/Assertions/`

```csharp
public static class StoryAssertions
{
    public static void ShouldHaveValidMetadata(this Story story) { ... }
    public static void ShouldBeEncrypted(this string apiKey) { ... }
}
```

---

## Test Coverage Goals by Phase

| Phase | Target Line Coverage | Target Branch Coverage | Estimated Tests |
|-------|---------------------|----------------------|-----------------|
| **Phase 1** | 82% (+3%) | 20% (+10%) | +15 tests |
| **Phase 2** | 85% (+3%) | 30% (+10%) | +20 tests |
| **Phase 3** | 87% (+2%) | 40% (+10%) | +15 tests |
| **Phase 4** | 89% (+2%) | 45% (+5%) | +10 tests |
| **Phase 5** | 90% (+1%) | 50% (+5%) | +8 tests |
| **Phase 6** | 91% (+1%) | 55% (+5%) | +10 tests |
| **TOTAL** | **91%** | **55%** | **+78 tests** |

---

## Implementation Guidelines

### Spec-First Process (REQUIRED)

**Before writing any test:**
1. AI explains component context and workflow position
2. Developer provides authoritative spec
3. Spec is documented in `/specs/[component-name].md`
4. Tests are written from spec (not from implementation)

### Unit Test Template
```csharp
namespace StoryFort.Tests.Unit;

/// <summary>
/// Tests for [ComponentName]
/// Spec: /specs/[component-name].md
/// </summary>
public class ServiceNameTests
{
    [Fact]
    public void MethodName_WithCondition_ExpectedBehavior()
    {
        // Arrange
        var mockDependency = new Mock<IDependency>();
        var sut = new ServiceName(mockDependency.Object);
        
        // Act
        var result = sut.MethodName(input);
        
        // Assert
        result.Should().Be(expected);
        mockDependency.Verify(x => x.Method(), Times.Once);
    }
}
```

### Integration Test Template (PREFERRED for Prototypes)
```csharp
namespace StoryFort.Tests.Integration;

public class FeatureIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    
    public FeatureIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }
    
    [Fact]
    public async Task Feature_WorksEndToEnd()
    {
        // Arrange
        await SeedTestData();
        
        // Act
        var result = await _service.ExecuteFeature();
        
        // Assert
        var persisted = await _context.Entities.FindAsync(result.Id);
        persisted.Should().NotBeNull();
    }
    
    public void Dispose() => _context.Dispose();
}
```

---

## Continuous Improvement

### Coverage Monitoring
- Run `dotnet test --collect:"XPlat Code Coverage"` on every PR
- Fail CI if coverage drops below 85%
- Generate HTML report: `reportgenerator -reports:**/coverage.cobertura.xml`

### Mutation Testing (Optional - Phase 7)
Consider adding **Stryker.NET** for mutation testing:
```bash
dotnet tool install -g dotnet-stryker
dotnet stryker
```

This will verify tests are actually catching bugs, not just hitting lines.

---

## Risk Mitigation

### High-Risk Areas Requiring Immediate Coverage

1. **CohereTutorService** (0%)
   - **Risk:** API key leakage, prompt injection bypass
   - **Mitigation:** Add security-focused tests in Phase 1

2. **ArchetypeService** (0%)
   - **Risk:** Corrupt archetype data breaks planner
   - **Mitigation:** Add data integrity tests in Phase 2

3. **SessionState** (27.5%)
   - **Risk:** State loss causes frustration, data corruption
   - **Mitigation:** Add event and caching tests in Phase 2

### Low-Priority Areas (Defer)

- UI Components (Blazor pages) → Covered by E2E
- Migration classes (97%+ coverage already)
- DTO classes (no logic to test)

---

## Success Metrics

### Quantitative
- ✅ Line coverage: 79% → **91%**
- ✅ Branch coverage: 9.9% → **55%**
- ✅ Method coverage: 36.9% → **65%**
- ✅ Test count: 18 → **96+**

### Qualitative
- ✅ All critical paths have happy + error cases
- ✅ Security features (G-01, G-02) have dedicated tests
- ✅ Tests run in <60 seconds (maintain fast feedback)
- ✅ No flaky tests (deterministic, isolated)

---

## Timeline

| Sprint | Weeks | Focus | Deliverable |
|--------|-------|-------|-------------|
| **Sprint 1** | Weeks 1-2 | Phase 1-2 (AI + Business Logic) | +35 tests, 85% coverage |
| **Sprint 2** | Weeks 3-4 | Phase 3-4 (Helpers + Models) | +25 tests, 89% coverage |
| **Sprint 3** | Weeks 5-6 | Phase 5 (Integration) | +8 tests, 90% coverage |
| **Sprint 4** | Weeks 7-8 | Phase 6 (E2E) | +10 tests, 91% coverage |

**Total Duration:** 8 weeks (2 months)

---

## Next Steps

1. **Review & Approve Plan** (This document) ✅
2. **Adopt Spec-Driven Workflow** (Feb 2, 2026) ✅
3. **Create `/specs/` Directory** for behavior specifications
4. **Audit Existing Tests** for Green Tick Drift (3-5 files)
5. **Start Next Feature Spec-First** 
   - AI explains context and workflow position
   - Developer provides authoritative spec
   - Document spec in standardized format
   - Generate failing tests from spec
   - Implement to pass tests

---

**Prepared by:** AI Assistant (GitHub Copilot)  
**Review Status:** Pending Developer Approval  
**Last Updated:** February 1, 2026
