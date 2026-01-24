# Unstickd: Solution Architecture Document
**Status:** Draft | **Version:** 0.4 | **Date:** 2026-01-24

## 1. Executive Summary
Unstickd is a **Hybrid AI-augmented creative writing environment** designed for children. Unlike traditional generative AI tools which prioritize output generation ("Ghostwriting"), Unstickd prioritizes **process scaffolding**. It uses **Cohere's Intelligence API (Reasoning + Chat)** to provide passive, Socratic guidance, ensuring the user maintains the "Locus of Control" over their creative work.

The system is architected as a **.NET 10 Blazor Server** application running in a **Docker container hosted in Canada**. This ensures data sovereignty while providing the accessibility of a modern web application.

---

## 2. Architecture Pillars

### Pillar 1: The Sovereign Hybrid Engine
**High-Performance Reasoning Meets Canadian Data Sovereignty.**

*   **The Containerized Core**: The application core is a .NET 10 Blazor Server instance running in a Docker container hosted in Canada. This ensures that while the application is accessible via the web, the execution environment operates under strict data sovereignty laws, distinct from the typical "US-East" default of major tech competitors.
*   **The Intelligence Layer**: We utilize a "Hybrid" AI strategy. We bypass standard chatbots in favor of Cohere’s "Reasoning" models (Command R+), accessed via a secure API. This allows the system to perform complex logic—like the "Spark Protocol" brainstorming loop—without storing user data on the inference side, adhering to a strict "non-training" agreement.
*   **The Benefit**: Schools and parents get the ease of a web application with the legal assurance of Canadian data residency and enterprise-grade privacy boundaries.

### Pillar 2: Rights-First Data Governance
**Encryption, IP Respect, and Ethical Research.**

*   **Child-Centric Encryption**: Moving beyond simple local storage, we implement Server-Side Encryption at Rest. The full story content is encrypted to the child’s account and is architecturally gated: it must be shared with at least one Adult Supervisor, enforcing the "Supervisor Primacy" principle (GP-3).
*   **Intellectual Property & Privacy**: We explicitly engineer for the child's IP rights. Unlike platforms that claim ownership of user content, Unstickd treats the story as the child's property. For research initiatives, we collect only anonymized metadata or small snippets, and strictly on an Opt-In basis for parents/guardians (never opt-out), ensuring ethical compliance.
*   **The Benefit**: This architecture solves the "Privacy Paradox" in EdTech. It allows for the data collection necessary to improve the tool (via opt-in research) while mathematically guaranteeing that a child’s creative work remains their private, secure property.

### Pillar 3: "Psychology-as-Code" Scaffolding
**Encoding Self-Determination Theory into the Software Stack.**

*   **The "No Ghostwriting" Firewall**: We implement GP-2 (Guiding Principle 2) as an architectural constraint. The System Prompt is an invariant firewall ("You do not write the story"), and the UI physically lacks "Insert" buttons for AI text. This prevents "Skill Atrophy" by forcing the child to maintain the "Locus of Control".
*   **Context-Sensitive Pacing**: The system does not use a static inactivity timer. It differentiates between "Transcription Pauses" (typing breaks) and "Generative Pauses" (deep thinking). If a child stops after a punctuation mark, the system waits 30 seconds before intervening, respecting the cognitive load required to plan a narrative arc.
*   **The Spark Protocol**: When a child faces a blank page, the AI triggers a "Divergent -> Convergent" brainstorming state machine. It guides the user from a sensory question to a scenario prompt, then performs a "Handoff" that minimizes the AI and focuses the cursor, ensuring the child starts writing with their idea, not the machine's.

---

## 3. Guiding Principles (Risk Governance Lens)

These principles serve as the "Constitution" for architectural decision-making, specifically viewing features through a risk management lens.

### GP-1: Trusted AI Partnership (Data Privacy)
*   **Principle**: User data (stories, chats) is processed by a trusted 3rd party (Cohere) strictly for inference and must **not** be used for model training. Sensitive PII (Name, Email) remains local/sovereign.
*   **Risk Mitigation**: We utilize Cohere's enterprise-grade privacy commitments. No data persistence on the inference side.
*   **Architectural Implication**: Database is server-side encrypted (SQLite in Container). Inference requires HTTPS egress. API Keys must be managed securely by the Supervisor.

### GP-2: The "No Ghostwriting" Rule (Responsible AI)
*   **Principle**: The System shall never generate narrative prose for the user. It may only ask questions, suggest synonyms for review (not insertion), or provide structural templates.
*   **Risk Mitigation**: Mitigates "Skill Atrophy" in educational contexts and prevents the generation of harmful/hallucinated content directly into the user's manuscript.
*   **Architectural Implication**: System Prompts must be hard-coded to refuse generation requests. UI must lack "Insert" buttons for AI text.

### GP-3: Supervisor Primacy
*   **Principle**: Configuration of the AI (Model selection, API Keys), sensitive settings (Theme triggers), and content moderation is the domain of the Supervisor (Parent/Teacher), not the Child User.
*   **Risk Mitigation**: Prevents students/children from disabling safeguards or altering the pedagogical parameters of the session.
*   **Architectural Implication**: Settings requiring maturity are gated behind a "Teacher Gate" (PIN/Challenge). Encryption keys are managed by the Supervisor account.

---

## 4. Solution Overview

Unstickd uses a **Monolithic Blazor Server** architecture. The client (Browser) acts as a "Thin Terminal," rendering UI updates pushed over a persistent SignalR WebSocket connection from the containerized ASP.NET Core process.

### 4.1 High Level Diagram

```mermaid
graph TD
    User[Student User] -->|HTTPS / WebSocket| Browser[Web Browser]
    Browser -->|Input Events & DOM Updates| AppServer[Unstickd Blazor Server (Docker)]

    subgraph "Canadian Hosting Zone"
        AppServer
        DB[(SQLite Database + Encryption)]
    end
    
    subgraph "Cloud AI Provider"
        CohereAPI[Cohere Platform]
        R_Model[Command R+ (Reasoning)]
        L_Model[Command Light (Chat)]
    end

    AppServer -->|EF Core Access| DB
    AppServer -->|HTTPS JSON (Auth: Bearer Key)| CohereAPI
    CohereAPI --> R_Model
    CohereAPI --> L_Model
```

### 4.2 Application Components (C4 Container Level)

```mermaid
graph TD
    subgraph "Unstickd Application Service"
        Router[Blazor Router]
        
        subgraph "Presentation Layer"
            Editor[Editor.razor Component]
            Tutor[TutorPanel.razor Component]
            Dash[Dashboard / Notebooks]
        end
        
        subgraph "Application Logic"
            Store[StoryState (Scoped Service)]
            Orch[TutorOrchestrator]
            Monitor[Activity Monitor (JS Interop)]
        end
        
        subgraph "Infrastructure"
            DAL[AppDbContext (EF Core)]
            Net[HttpClient Factory]
            Log[Serilog Logger]
        end
    end
    
    %% Relationships
    Router --> Editor
    Router --> Tutor
    
    Editor -->|Updates| Store
    Editor -->|Triggers| Monitor
    Monitor -->|Inactivity Events| Orch
    
    Orch -->|Generates Prompts| Net
    Orch -->|Updates| Store
    
    Tutor -->|Observes| Store
    
    Store -->|Persists| DAL
```

### 4.3 Technology Stack
- **Framework**: .NET 10 (Preview)
- **Host Model**: Blazor Interactive Server (Linux Container)
- **Database**: SQLite (via `Microsoft.EntityFrameworkCore.Sqlite`) with Encryption
- **Rich Text**: QuillJS (via `Blazored.TextEditor`)
- **AI Integration**: Standard `System.Net.Http.HttpClient` communicating with **Cohere API**.
- **Testing**: xUnit, Moq, FluentAssertions, Playwright.

---

## 5. Data Architecture

The data model centers around the `Account` (User Context) and `Story` (Creative Context).

### 5.1 Entity Relationship Diagram (ERD)
*Note: Conceptual representation*

- **Account** (1) ---- (N) **Story**
- **Account** (1) ---- (N) **Notebook** (Global Resource Container)
- **Notebook** (1) ---- (N) **NotebookEntity** (Characters, Places)
- **NotebookEntity** (N) ---- (N) **Story** (via `StoryEntityLink`)
- **NotebookEntity** (1) ---- (N) **NotebookEntry** (Notes, Lore)

### 5.2 Key Data Attributes
- **Story.Content**: Stores the raw HTML of the story. *Note: We recently migrated from a Page-based model to a continuous scroll model.*
- **NotebookEntity.Metadata**: A flexible JSON blob allowing users to define custom attributes (e.g., "Strength", "Home Planet") without schema migrations. Suggests a future move to a Document-based approach for some features if complexity grows.

---

## 6. Gap Analysis (Current Code vs. Target Architecture)

This section highlights discrepancies between the documented architecture and the codebase as of Jan 24, 2026.

| ID | Feature | Target State | Current State | Risk Level |
|----|---------|--------------|---------------|------------|
| G-01 | **Teacher Gate** | Sensitive settings protected by PIN/Challenge. | Settings are openly accessible. | **High** |
| G-02 | **Input Guardrails** | "Bad Word" regex filter on AI inputs. | No filtering implemented. | **Med** |
| G-03 | **Age Gating** | Onboarding calculates age for themes. | Onboarding UI exists but logic checks are minimal. | **Low** |
| G-04 | **Assignment Mode** | "Curriculum Workflow" & Split-view Assignments. | Not started. | **Feature Gap** |
| G-05 | **Cohere Integration** | HttpClient targeting `api.cohere.com`. | Code is updated to target Cohere, but `Gap Analysis` row retained until E2E verified. | **InProgress** |
| G-06 | **Automated Tests** | Full pyramid (Unit/Int/E2E). | Scaffolding exists, coverage is 0%. | **High** |

---

## 7. Project Structure & Patterns
*Recommended addition for C# standard practices.*

This section maps the logical architecture to the physical project structure, clarifying separation of concerns within the monolithic project.

*   \Unstickd/Models/\: **Domain Entities**. POCOs representing the core data structures (\Account\, \Story\, \Notebook\).
*   \Unstickd/Data/\: **Persistence Layer**. \AppDbContext\ and EF configurations.
*   \Unstickd/Services/\: **Business Logic & State**.
    *   \StoryState.cs\: Session-scoped logic container.
    *   \TutorOrchestrator.cs\: AI interaction workflow manager.
    *   \CohereTutorService.cs\: AI infrastructure adapter.
*   \Unstickd/Components/\: **Presentation Layer** (Blazor).
    *   \/Pages\: Routable pages (\Editor.razor\).
    *   \/EditorPanels\: complex sub-components (\TutorPanel.razor\).
*   \Unstickd/wwwroot/js/\: **Browser Interop**. TypeScript/JS bridges for events Blazor cannot handle natively (e.g., rich text scroll events, user inactivity timers).

## 8. Cross-Cutting Concerns

### 8.1 Error Handling
- **Global Boundary**: A custom \CustomErrorBoundaryLogger\ wraps key routing points to catch unhandled Blazor circuit exceptions.
- **Fail-Safe**: If AI services fail, the application degrades gracefully to a "Offline/Manual" mode, preserving the writing capability.

### 8.2 Logging (Observability)
- **Serilog**: Structured logging is configured to write to local files (\logs/unstickd-*.txt\).
- **Privacy Filter**: Logs are explicitly configured to *exclude* \Story.Content\ to preventing leaking creative work into plaintext log files.

### 8.3 Authentication (Supervisor Model)
- **Current State**: Simplified "Single User" assumption for MVP.
- **Architecture Target**: Multi-Account support involves a Supervisor (Admin) managing multiple Child (Standard) accounts. Authentication is handled via standard ASP.NET Core Identity (future work, currently mocked).

## 9. Architectural Decision Records (ADR Summary)

*   **ADR-001: Blazor Server vs. WASM**: Chosen **Server** to simplify API key security (keys stay on server) and database access (no API layer required initially). *Trade-off*: Requires constant connection.
*   **ADR-002: SQLite vs. Postgres**: Chosen **SQLite** to allow the application to be self-contained and easily deployable in a single container without complex orchestration.
*   **ADR-003: Hybrid AI (Cohere)**: Chosen **Cohere** over OpenAI for stricter "Non-Training" data usage agreements and superior RAG/Reasoning capabilities in the Command R+ model series.

## 10. Deployment & Infrastructure
*Status: TBD*

*Detailed deployment pipeline and infrastructure specifications to be defined.*

## 11. State Management Strategy
**The "Anti-Fragile" Circuit Pattern**

To mitigate the inherent risks of Blazor Server (circuit disconnects), we adopt a hybrid state strategy:

1.  **Persistence is Truth**: The C# memory state is considered volatile. 
    *   **Story Content**: Auto-saved to SQLite every 2 seconds via \editor.js\ debounce.
    *   **Loss Tolerance**: A circuit reset may lose the last <2 seconds of typing, which is an acceptable trade-off for data sovereignty and simplicity.
2.  **Ephemeral Chat**: \TutorNotes\ (AI conversation) are strictly in-memory. 
    *   **Design Rationale**: If a session breaks, the chat clears. This is a feature, not a bug, reinforcing the "Notebook" metaphorthe AI is a transient thinking partner, not a permanent record.
3.  **Client-Side Continuity**: 
    *   **Preferences**: View settings (Theme, Font Size) are mirrored to \localStorage\ via JS Interop to survive refreshes.

## 12. Scalability Considerations
*Future Roadmap for 10k+ Users*

*   **Current Limit**: Blazor Server maintains a stateful circuit per user on the server RAM. 
*   **Bottleneck**: Memory usage per active user and WebSocket connection limits on the host.
*   **Scaling Strategy**: 
    1.  **Vertical**: Increase container RAM.
    2.  **Horizontal (Phase 2)**: Deploy behind a load balancer with Sticky Sessions (Session Affinity).
    3.  **Cloud-Native (Phase 3)**: Offload WebSocket management to a dedicated service (e.g., Azure SignalR) to handle connection multiplexing, allowing the app server to remain stateless regarding connection handling.

## 13. AI Orchestration & Prompt Architecture
**The Context-Aware Decision Engine**

To support future scalability beyond the single "Spark Protocol," the architecture defines the \TutorOrchestrator\ as a dynamic factory that selects prompt strategies based on user state. This ensures we can add new pedagogical modes (e.g., "Grammar Review", "Character Deep Dive") without rewriting the core loop.

### 13.1 Prompt Construction Data Flow

The following diagram illustrates how the system dynamically assembles a prompt based on the user's current context (Age, Genre) and state variables.

\\\mermaid
sequenceDiagram
    participant User as User / Editor UI
    participant State as StoryState (Memory)
    participant Orch as TutorOrchestrator
    participant Strategy as PromptStrategy (Logic)
    participant LLM as Cohere API

    Note over User, Orch: T0: User stops writing (Inactivity Event)
    
    User->>Orch: Trigger(Inactivity detected)
    
    Orch->>State: Fetch Context
    State-->>Orch: Returns { Age, Genre, Last 4 Sentences, Current Mode }
    
    Note over Orch: Selection Logic
    Orch->>Orch: Select Workflow based on Mode/State<br/>(e.g., 'Spark' vs 'Review')
    
    Orch->>Strategy: BuildPrompt(Context)
    Strategy->>Strategy: Inject {Genre} & {Age} into System Prompt
    Strategy->>Strategy: Append User Text to <User_Story> tag
    Strategy-->>Orch: Return Formatted Request
    
    Orch->>LLM: POST /v1/chat (with Preamble)
    LLM-->>Orch: JSON Response (Reasoning + Text)
    
    Orch->>State: Update TutorNotes
    State-->>User: Refresh UI Bubble
\\\

### 13.2 Key Architectural Patterns
1.  **Late Binding of Context**: We do not bake user details into the HttpClient. The TutorOrchestrator injects Age, Genre, and Archetype at the *moment of request*, ensuring the AI adapts immediately if settings change.
2.  **Strategy Pattern (Future)**: Currently functional, the roadmap calls for refactoring prompt generation into IPromptStrategy implementations (e.g., SparkStrategy, ReviewStrategy), allowing the Orchestrator to switch behavior polymorphically based on StoryState.CurrentMode.
3.  **The "Context Window" Management**: To maintain speed and lower costs, we only send the *relevant tail* of the story (last ~4-6 paragraphs) rather than the full manuscript, unless a specific "Whole Story Review" is requested.
