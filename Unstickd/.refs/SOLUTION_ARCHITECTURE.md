# Unstickd: Solution Architecture Document
**Status:** Draft | **Version:** 0.2 | **Date:** 2026-01-24

## 1. Executive Summary
Unstickd is a **Hybrid AI-augmented creative writing environment** designed for children. Unlike traditional generative AI tools which prioritize output generation ("Ghostwriting"), Unstickd prioritizes **process scaffolding**. It uses **Cohere's Intelligence API (Reasoning + Chat)** to provide passive, Socratic guidance, ensuring the user maintains the "Locus of Control" over their creative work.

The system is architected as a **.NET 10 Blazor Server** application running locally, interfacing with a local SQLite database for persistence and a managed, enterprise-grade cloud inference provider (Cohere). This hybrid approach balances data privacy with state-of-the-art reasoning capabilities.

---

## 2. Guiding Principles (Risk Governance Lens)

These principles serve as the "Constitution" for architectural decision-making, specifically viewing features through a risk management lens.

### GP-1: Trusted AI Partnership (Data Privacy)
*   **Principle**: User data (stories, chats) is processed by a trusted 3rd party (Cohere) strictly for inference and must **not** be used for model training. Sensitive PII (Name, Email) remains local.
*   **Risk Mitigation**: We utilize Cohere's enterprise-grade privacy commitments. No data persistence on the inference side.
*   **Architectural Implication**: Database is embedded (SQLite). Inference requires HTTPS egress. API Keys must be managed securely by the Supervisor.

### GP-2: The "No Ghostwriting" Rule (Responsible AI)
*   **Principle**: The System shall never generate narrative prose for the user. It may only ask questions, suggest synonyms for review (not insertion), or provide structural templates.
*   **Risk Mitigation**: Mitigates "Skill Atrophy" in educational contexts and prevents the generation of harmful/hallucinated content directly into the user's manuscript.
*   **Architectural Implication**: System Prompts must be hard-coded to refuse generation requests. UI must lack "Insert" buttons for AI text.

### GP-3: Supervisor Primacy
*   **Principle**: Configuration of the AI (Model selection, API Keys), sensitive settings (Theme triggers), and content moderation is the domain of the Supervisor (Parent/Teacher), not the Child User.
*   **Risk Mitigation**: Prevents students/children from disabling safeguards or altering the pedagogical parameters of the session.
*   **Architectural Implication**: Settings requiring maturity are gated behind a "Teacher Gate" (PIN/Challenge).

---

## 3. Safety, Privacy & Ethics by Design

### 3.1 Privacy by Design
- **Data Minimization**: We do not store Date of Birth (DoB); it is processed in-memory during onboarding to determine age-appropriate settings and discarded.
- **Trusted Cloud**: We rely on Cohere's non-training agreement for API usage.
- **Local Logs**: Application logs (Serilog) are stored locally in text files (`logs/`) and rotated. Review required to ensure no Story Content or PII is written to logs during exceptions.

### 3.2 Security by Design
- **Environment Isolation**: As a strictly local web app, standard web vulnerabilities (XSS, CSRF) are mitigated significantly.
- **Supervisor Gating**: Critical destructive actions (Delete Story) and Configuration (API Keys) are protected interfaces.
- **Content Filtering**: We implement a regex-based **"Bad Word Filter"** on the *Inputs* to the LLM to prevent users from jailbreaking the tutor. Cohere's own safety filters act as a second layer of defense.

### 3.3 Responsible AI by Design
- **Socratic System Prompt**: The core system prompt acts as a firewall against generative behavior.
    - *Invariant*: "You are a helpful tutor. You do not write the story. You ask questions."
- **Reasoning vs. Chat**: We leverage Cohere's Reasoning models (Command R+) for complex "Spark" brainstorming and lighter models (Command Light) for quick "Review" interactions.
- **Inference Transparency**: The UI explicitly differentiates between "User Content" (Editor) and "AI Content" (Tutor Panel/Bubble).

---

## 4. Solution Overview

Unstickd uses a **Monolithic Blazor Server** architecture. The client (Browser) acts as a "Thin Terminal," rendering UI updates pushed over a persistent SignalR WebSocket connection from the local ASP.NET Core process.

### 4.1 High Level Diagram

```mermaid
graph TD
    User[Student User] -->|HTTPS / WebSocket| Browser[Web Browser]
    Browser -->|Input Events & DOM Updates| AppServer[Unstickd Blazor Server]

    subgraph "Local Host Environment"
        AppServer
        DB[(SQLite Database)]
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
- **Host Model**: Blazor Interactive Server
- **Database**: SQLite (via `Microsoft.EntityFrameworkCore.Sqlite`)
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
| G-05 | **Cohere Integration** | HttpClient targeting `api.cohere.com`. | Code targets `localhost:11434` (Ollama). | **Crucial** |
| G-06 | **Automated Tests** | Full pyramid (Unit/Int/E2E). | Scaffolding exists, coverage is 0%. | **High** |

---
