## Spec: StoryPersistence — Concurrency & Conflict Resolution
**File:** StoryFort.Tests.Integration/StoryPersistence_Concurrency_Specs.cs
**Type:** Integration
**Priority:** High
**Estimated Tests:** 4

### Context
`StoryPersistenceService` is the canonical service for saving story content. Multiple UI components or users may update a story concurrently (e.g., background autosave + manual save, or two collaborators). We must ensure no silent data loss, no corrupted HTML, and predictable conflict-resolution behavior.

### Behavior Requirements

#### Happy Path (Single Writer)
- GIVEN a single client updates story content and saves
- WHEN SaveContentAsync is called
- THEN the content persists and subsequent loads return the saved content

#### Concurrent Saves — Last-Write-Wins (Approved Strategy)
- GIVEN two concurrent updates A and B to the same story (A starts then B starts shortly after)
- WHEN both saves complete
- THEN Last-write-wins resolution applies: whichever save finishes last persists
- AND the final persisted content is complete HTML (either A or B, deterministic based on completion order)
- AND no exceptions are thrown to either caller

#### No Data Corruption
- GIVEN concurrent updates where one payload contains large HTML and another contains small edits
- WHEN saves occur concurrently
- THEN persisted content must be complete HTML (no truncation, no unbalanced tags), validated by basic parse check using HtmlAgilityPack (document node can be loaded without errors)

#### Resilience Under Transaction Failure
- GIVEN a simulated DB failure during SaveContentAsync (e.g., connection drop)
- WHEN save is attempted
- THEN the API surfaces a clear error and partial writes do not leave the story in a corrupted state (either previous content intact or fully rolled back)

### Non-Functional Requirements
- Consistency: Tests validate no partial writes and that saved content parses as valid HTML.
- Observability: When concurrent resolution occurs, save logic should log which write won (tests may assert that no unhandled exceptions occur).
- Performance: Two concurrent saves of small payloads complete under 500ms in the CI environment.

### Test Data Examples
1. Start with story content: `<p>Start</p>`
   - Save A: `<p>Start — A</p>` (delay 100ms before save completes)
   - Save B: `<p>Start — B</p>` (no delay)
   - Expect: depending on resolution strategy, verify persisted content matches documented behavior; prefer Last-write-wins verifying that final content is either A or B but is a complete HTML fragment

2. Large vs small update
   - Save A: large HTML payload (2KB)
   - Save B: small change like replace a word
   - Expect: no truncation; HtmlAgilityPack loads persisted content

3. Simulated DB failure
   - Configure test scope to throw during SaveChangesAsync
   - Expect: SaveContentAsync surfaces exception or returns an error indicator and DB state remains prior content

### Success Criteria
- [ ] Deterministic resolution behavior verified by tests (Last-write-wins or explicit 409 strategy)
- [ ] No persisted HTML corruption under concurrent saves
- [ ] Server recovers or reports errors cleanly when underlying DB fails

---

**Reviewer note:** Approve the concurrency resolution strategy you want enforced in tests ("last-write-wins" or "optimistic-concurrency-with-409"). Once you approve, I will generate the integration tests that simulate concurrent saves and DB failure, run them, and implement minimal production changes if needed to satisfy the spec.