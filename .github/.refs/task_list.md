# Requirements & Task List: StoryFort Developer Diary

## Design Principles
*   **Separation of Powers**:
    *   *Drafting*: The child writes "The big dog." (Flow State).
    *   *Reviewing*: The AI points out "Doyle called it an 'immense hound'." (Learning Mode).
    *   *Goal*: Vocabulary Nudges must never interrupt the initial creative burst (Passive Observation).
*   **No Ghostwriting**: The system never rewrites the user's sentences. It provides hints, rules, or inspiration.

## 1. AI & Prompt Engineering

### [ ] Create `SystemPrompt_Spark.md` (The Electric Starter)
*   **Context**: Used when Story.Length == 0 to solve "Blank Page Fear" via the "Henry Protocol."
*   **Objective**: Execute a "Divergent -> Convergent" brainstorming loop (max 4 turns).
*   **The Funnel**:
    1.  The Wish: Sensory/Wish-fulfillment question.
    2.  The Vibe: Narrowing to attributes/personality.
    3.  The Scenario: "What if" prompt.
*   **Output Requirement**: The LLM must output valid JSON. When a specific story idea is detected, it must set the flag `{"status": "READY_TO_WRITE"}`.

### [ ] Create `SystemPrompt_ReviewTutor.md` (The Learning Loop)
*   **Context**: Defines the logic for the "Review Mode" trigger.
*   **Orthographic Coaching**:
    1.  Analyze a specific misspelled word provided by the system.
    2.  Identify the orthographic rule broken (e.g., "Bossy-E", "I before E").
    3.  Generate a phonetic or rule-based hint (e.g., "It sounds like 'sh', but we use 'tion'").
*   **Style Coaching (New)**:
    *   **Logic**: "If the user is stuck on a repetitive word (e.g., 'big', 'said'), do not just give a synonym. Quote a famous author from the `{SelectedGenre}` using a better word, then ask the child if they like that sound."
*   **Constraint**: Strict adherence to "No Ghostwriting".

### [ ] Create `Corpus_Definition.json` (The Bridge)
*   **Context**: Static lists to simulate RAG for the MVP.
*   **Data**: Mapping of common weak words (Big, Said, Went) to Genre-specific strong words.
    *   *Example*: "Big" -> Fantasy: "Immense", "Colossal" | Sci-Fi: "Massive", "Planetary".
*   **Extension**: Ensure JSON includes Full Text (or path to text) for `Reader.razor` to access.

## 2. Frontend & UX Logic

### [x] Update [inactivity.js](../StoryFort/wwwroot/js/inactivity.js) - Frustration Detection
*   **Context**: "Hyper-activity" is a signal for help.
*   **Requirement**: Listen for:
    *   **Rage Clicks**: >3 clicks on the same coordinates within 1s.
    *   **Button Mashing**: High-velocity random keystrokes followed by immediate deletion (Implemented as >5 deletions/sec).
*   **Outcome**: Trigger `TutorState.OfferingHelp` immediately, bypassing standard timers.

### [ ] Update `inactivity.js` - Repetition Detection
*   **Context**: Expanding the "Silent Spellcheck".
*   **Logic**: Count instances of non-stop-words within the current session/paragraph.
*   **Trigger**: If frequency > threshold, flag as `ReviewIntent.Vocabulary`.

### [ ] Implement "Spark Handoff" State (The Catch)
*   **Trigger**: Receiving `READY_TO_WRITE` flag from the Spark LLM.
*   **Action 1 (UI)**: Call `TutorService.Dismiss()` to minimize the bubble.
*   **Action 2 (Data)**: Create a temporary `NotebookEntry` (Type: Idea) with the spark_summary content and pin it to the side panel.
*   **Action 3 (Focus)**: Call `await JSRuntime.InvokeVoidAsync("focusQuillEditor")` to force cursor into document.

### [ ] Implement Client-Side "Silent" Spellcheck
*   **Context**: To trigger "Review Mode" without "Red Pen Anxiety".
*   **Requirement**: Lightweight JS solution (e.g., typo.js or Bloom Filter) running in background.
*   **Logic**: If `ErrorCount > 3` AND `Inactivity > 15s`, trigger the "Review Mode" bubble.

### [ ] Implement "The Blender" (Story Mixer UI)
*   **Context**: Interface for combining story elements. Enables mixing characters, settings, and themes.

### [ ] Create `Reader.razor` (The Library View)
*   **Context**: A distraction-free reading interface, separate from `Editor.razor`.
*   **UI Requirements**:
    *   Large, readable serif font (e.g., Merriweather).
    *   "Pinterest-style" highlighting menu.
*   **Data Source**: Displays "Public Domain Corpus" texts from `Corpus_Definition.json`.

### [ ] Implement "Text Pinning" Logic
*   **Interaction**: User selects text -> Clicks "Pin" -> Selects Target Notebook.
*   **Backend**: Creates a `NotebookEntry` with:
    *   **Type**: `NotebookType.Quote` (New Enum Member).
    *   **Content**: The selected text.
    *   **Metadata**: `{"SourceBook": "Sherlock Holmes", "Author": "Doyle"}`.
*   **Outcome**: Quotes appear in the global "Toy Box" sidebar.

### [ ] Implement "Book Unlock" System
*   **Trigger**: `Story.Status` changes to `Completed`.
*   **Logic**: Check `Story.Genre` and `Story.Archetype`. Unlock corresponding book in `Account.Library`.
*   **Visual**: "New Book Unlocked!" celebration modal.

## 3. Horizon Opportunities (Post-MVP)

### [ ] The RAG "Mentor" Engine
*   **Context**: Replacing static JSON (`Corpus_Definition.json`) with dynamic vector lookups.
*   **Architecture**:
    *   Ingest provided library into local vector store (e.g., ChromaDB).
    *   Classify chunks by "Descriptive Type" (e.g., Description of Size, Speed).
*   **Interaction**: Hover over repetitive word -> "How did Doyle say it?" tooltip.

### [ ] The "Publisher" Engine (Keepsake Reward)
*   **Context**: Physical output justifying B2B2C model.
*   **Task**: Logic for mapping screen themes (Dark Mode/Space) to Print-on-Demand PDF specs (CMYK/Margins).

### [ ] Blueprint Data Seeding
*   **Context**: "Structural Scaffold" for mid-story support.
*   **Task**: JSON schema for extracting archetypes (Hero, Mentor, MacGuffin) from Project Gutenberg texts.

## 4. Audit Remediation (Hardening)

### [ ] Disaster Recovery: Litestream Implementation
*   **Context**: Auditor identified "Docker Volume" as a single point of failure (RPO 24h).
*   **Action**:
    1.  Add `litestream` binary to Dockerfile.
    2.  Configure `litestream.yml` to replicate `StoryFort.db` to an S3-compatible bucket (e.g., OVH Object Storage).
    3.  Update `entrypoint.sh` to restore DB on startup if missing.

### [ ] Network Resilience: "Disconnected Operation" Verification
*   **Context**: Auditor flagged Blazor Server latency risk.
*   **Action**:
    1.  Verify `editor.js` handles all typing/cursor events locally (no SignalR round-trips for `oninput`).
    2.  Stress test with "Network Throttling" (3G) to ensure typing remains fluid.
    3.  Implement "Offline/Reconnecting" UI indicator if WebSocket drops.

### [ ] Security: Basic Threat Modeling
*   **Context**: WebSocket spoofing risk.
*   **Action**:
    1.  Ensure `StoryFort` hub methods validate `Context.User.Identity`. *(Note: Requires Auth implementation first)*.
    2.  Review `TutorOrchestrator` for rate-limiting (prevent token exhaustion).

### [ ] Governance: Operational Sovereignty Policy
*   **Context**: Documenting the "Human Process" control for Canadian Sovereignty.
*   **Action**: Create `GOVERNANCE.md` stating explicit restriction: Production access is limited to Canadian-resident personnel only.

### [ ] Safety: Prompt Injection Defense (Input Filtering)
*   **Context**: Auditor flagged "Ignore previous instructions" as OWASP LLM Top 10 risk.
*   **Action**:
    1.  Add regex filter in `TutorOrchestrator` to detect injection patterns (e.g., "ignore", "disregard", "new instructions").
    2.  If detected, return a canned "I can only help with your story!" response without calling Cohere.

### [ ] Safety: Output Moderation Check
*   **Context**: Defense-in-depth against harmful AI responses.
*   **Action**:
    1.  Before displaying `TutorNotes`, check response against a blocklist (violence, explicit content).
    2.  Optionally integrate Cohere's moderation endpoint for production.

### [ ] Safety: Red Teaming Session (Pre-Pilot)
*   **Context**: Adversarial testing before school deployment.
*   **Action**:
    1.  Create a test plan with 10-15 "jailbreak" prompts (e.g., "Write my story for me", "Pretend you're evil").
    2.  Document results and mitigations in `AI-System-Card.md`.

### [ ] Governance: Formalize AI System Card
*   **Context**: Auditor recommends transparency document for school admins.
*   **Action**:
    1.  Review existing draft at `.github/.refs/governance/AI-System-Card.md`.
    2.  Add "Red Teaming Results" section.
    3.  Link from main `ARCHITECTURE.md`.

### [ ] Accessibility: ARIA Live Regions for Tutor
*   **Context**: Screen reader support (WCAG 2.2 / Accessible Canada Act).
*   **Action**:
    1.  Add `aria-live="polite"` to `TutorPanel` message container.
    2.  Test with NVDA or VoiceOver.
    3.  Ensure keyboard navigation works in Quill editor (no "traps").

### [ ] Documentation: Context Window Management Strategy
*   **Context**: Auditor noted "last 4 sentences" is ad-hoc; Quizlet uses summarization.
*   **Action**:
    1.  Document the current "sliding window" approach in `ARCHITECTURE.md` Section 9.
    2.  For long stories (>2000 words), add a "Story So Far" summarization step before Spark/Review calls.

### [ ] Documentation: PII Stripping Data Flow Diagram
*   **Context**: Prove that PII never leaves Canadian enclave.
*   **Action**:
    1.  Create a Mermaid diagram showing: `Editor -> StoryState -> [PII Filter] -> Cohere API`.
    2.  Add to `ARCHITECTURE.md` Section 3 (Data Governance).

### [ ] Documentation: Spark Protocol State Diagram
*   **Context**: Auditor recommends formal visualization of pedagogical logic.
*   **Action**:
    1.  Create UML State Diagram: `Idle -> Monitoring -> Evaluating -> Prompting -> Scaffolding -> Handoff`.
    2.  Add to `ARCHITECTURE.md` Section 9 or `rationales.md`.

### [ ] DevOps: Prompt Version Control
*   **Context**: Khan Academy treats prompts as versioned R&D artifacts.
*   **Action**:
    1.  Move System Prompts from inline strings to `.md` files in `StoryFort/Prompts/`.
    2.  Add prompt version header (e.g., `# v1.2 - 2026-01-25`).
    3.  Consider a "Golden Dataset" of ideal AI responses for regression testing.

## 3. Infrastructure & DevOps (Paused)

### [ ] Re-implement Litestream for Disaster Recovery
*   **Context**: Blocked by local Windows virtualization issues (WSL2 unavailable).
*   **Impact**: Pilot will launch without real-time S3 replication (Daily backups only via manual export recommended).
*   **Action**: Solve virtualization issue or deploy to Linux VM.

### [ ] Containerize Application (Docker)
*   **Context**: Docker Desktop requires WSL2.
*   **Action**: Re-introduce `Dockerfile` and `.devcontainer` once hardware constraints are resolved.


