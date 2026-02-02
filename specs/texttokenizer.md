## Spec: TextTokenizer
**File:** StoryFort.Tests.Unit/TextTokenizer_Specs.cs
**Type:** Unit + Integration
**Priority:** High
**Estimated Tests:** 5

### Context
`TextTokenizer` converts HTML-rich content into an editable token stream used by review and replace operations. Consumers rely on the tokenizer to allow single-word replacements without corrupting HTML formatting (e.g., bold, italics, links).

### Behavior Requirements

#### Happy Path
- GIVEN HTML content with nested inline formatting (e.g., `<p>Hello <strong>world</strong>!</p>`)
- WHEN the caller replaces the token for the second word with `UNIVERSE`
- THEN the resulting HTML is `<p>Hello <strong>UNIVERSE</strong>!</p>` (formatting preserved)

#### Edge Cases
- GIVEN punctuation attached to a word (e.g., `world!`)
- WHEN replacing the word portion only
- THEN punctuation remains in place (e.g., `UNIVERSE!`) and formatting is preserved

- GIVEN an HTML fragment with links surrounding a word
- WHEN the token is replaced
- THEN the link remains intact and the replacement is inside the link

- GIVEN user replacement contains HTML-like input (e.g., `UNIVERSE!</body>`)
- WHEN the token is replaced
- THEN the insertion is treated as literal text and any angle-brackets are escaped (e.g., `&lt;/body&gt;`), preserving the overall HTML structure

#### Error Cases
- GIVEN malformed HTML
- WHEN tokenization is attempted
- THEN tokenizer either normalizes to a best-effort structure or returns a clear parse error object (no exceptions bubbled to caller)



### Non-Functional Requirements
- Performance: Tokenize and replace operations should complete under 10ms for a 2KB input in unit tests
- Security: Tokenizer must not execute scripts or alter attributes beyond text replacement

### Test Data Examples
- Input: `<p>Hello <strong>world</strong>!</p>` → Replace index 1 with `UNIVERSE` → Expected: `<p>Hello <strong>UNIVERSE</strong>!</p>`
- Input: `<p>Click <a href="/x">here</a> now</p>` → Replace `here` → Expected link preserved with new inner text

### Success Criteria
- [ ] Replacement tests pass for formatted text
- [ ] Punctuation handling verified
- [ ] Malformed HTML handled gracefully (no unhandled exceptions)

---

**Notes for reviewer (developer):**
- Please review and either `Approve` or provide edits to the GIVEN/WHEN/THEN cases before I generate tests.
- If approved, I will create `StoryFort.Tests.Unit/TextTokenizer_Specs.cs` with failing-first tests and proceed through the 10-step checklist.
