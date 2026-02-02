## Spec: TextTokenizer — Editor-to-Persistence Sanitization
**File:** StoryFort.Tests.Integration/Editor_Sanitization_Specs.cs
**Type:** Integration (Editor → Server → Persistence)
**Priority:** High
**Estimated Tests:** 3

### Context
Users edit story text in a rich-text editor that uses the `TextTokenizer` for in-place token replacement. The editor accepts user input that should be treated as literal text. Malformed or HTML-like user input (e.g., `UNIVERSE!</body>`) must not result in raw, unescaped HTML being persisted or rendered in a way that changes page structure or enables XSS.

This spec exercises the end-to-end flow: editor token replacement → server-side save API → persistence and retrieval.

### Behavior Requirements

#### Sanitization Behavior (Primary)
- GIVEN an existing story with HTML formatting (e.g., `<p>Hello <strong>world</strong>!</p>`)
- WHEN a user replaces a token with text that contains HTML-like fragments (e.g., `UNIVERSE!</body>`), and then saves
- THEN the persisted HTML must preserve original structure and treat user input as literal text (angle brackets escaped), e.g. `<p>Hello <strong>UNIVERSE!&lt;/body&gt;</strong>!</p>`
- AND the API returns a success response indicating save completed

#### Server-Side Defense-in-Depth
- GIVEN a malicious attempt to include script tags in replacement text (e.g., `<script>alert(1)</script>`)
- WHEN the change is submitted to the save endpoint
- THEN server-side sanitization or encoding prevents scripts from being persisted as executable markup (script tags must be removed or escaped)
- AND the client receives a sanitized copy of the persisted HTML

#### Failure Handling
- GIVEN an input that cannot be reasonably normalized or that trips server-side validators
- WHEN the user attempts to save
- THEN the API responds with a clear, user-facing error message (400) explaining the issue ("Your edit contains disallowed HTML; please remove tags or try plain text") and no unsafe content is persisted

### Non-Functional Requirements
- Sanitization must be deterministic and idempotent: multiple saves of the same input produce the same persisted value.
- Performance: round-trip save+retrieve for a single short edit must complete within 250ms in integration tests (CI environment tolerances considered).
- Security: no raw `<script>` or other executable markup can survive to the persisted HTML; inline event handlers (`onclick`) must be stripped or escaped.

### Test Data Examples
1. Input story: `<p>Hello <strong>world</strong>!</p>`
   - Replace `world` with `UNIVERSE!</body>` → Expected persisted: `<p>Hello <strong>UNIVERSE!&lt;/body&gt;</strong>!</p>`
2. Replace with: `<script>alert(1)</script>` → Expected persisted: `<p>Hello <strong>&lt;script&gt;alert(1)&lt;/script&gt;</strong>!</p>` or server rejects with 400 and a user-friendly error

### Success Criteria
- [ ] Integration test verifies persisted content contains escaped angle brackets (no `</body>` present) and original structure preserved
- [ ] Server rejects or sanitizes script tags so no executable markup persists
- [ ] API returns appropriate success or informative error responses

---

**Reviewer note:** Please `Approve` or provide edits. Once approved I will generate `StoryFort.Tests.Integration/Editor_Sanitization_Specs.cs` (failing-first), run the integration tests, and implement minimal server-side sanitization or adjust tokenizer persist pipeline as needed.