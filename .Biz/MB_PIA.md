This is a completed **Privacy Impact Assessment (PIA)** for the "StoryFort" application, populated strictly using the provided architecture, requirements, and governance documentation. It follows the structure of the Manitoba Ombudsman’s PIA Tool.

***

# Manitoba Ombudsman Privacy Impact Assessment Tool
**Project:** StoryFort (Ed-Tech Creative Writing Platform)
**Date:** January 24, 2026 (Based on Document Version 0.4)

---

## Part 1: Summary of Program or Activity

**Who?**
*   **Name of Project:** StoryFort.
*   **Project Representatives:** Technical Architects, School Administrators (Supervisor Role).
*   **External Entities:** Cohere (AI Intelligence Provider).

**What?**
*   **Summary:** StoryFort is a hybrid AI-augmented creative writing environment designed for children. Unlike generative AI tools that create content for users ("Ghostwriting"), StoryFort prioritizes "process scaffolding" by using AI for passive, Socratic guidance.
*   **Current State:** The application is a .NET 10 Blazor Server application running in a Docker container. It is currently in the "Draft" stage (Version 0.4).

**Purposes, goals and objectives**
*   **Primary Goal:** To foster creativity while maintaining user agency ("Locus of Control") and preventing "Skill Atrophy" in students.
*   **Privacy Goal:** To solve the "Privacy Paradox" in EdTech by ensuring Canadian data sovereignty and guaranteeing that children retain Intellectual Property (IP) ownership of their stories.

**Describe the type of application**
*   **Platform:** A monolithic web application (Blazor Server) accessible via browser.
*   **Hosting:** Docker container hosted specifically on Canadian infrastructure (e.g., OVH VPS-3 in Montreal/Toronto).
*   **AI Integration:** Connects via secure API to Cohere’s Intelligence Layer for reasoning and chat functions.

**Why?**
*   To provide schools and parents with the ease of a modern web application while adhering to strict "Rights-First Data Governance" and legal assurances of Canadian data residency, distinct from competitors defaulting to US hosting.

**Where?**
*   **Data Residency:** All persistent data (Stories, User Accounts) is hosted in Canada.
*   **Data Processing (Inference):** Anonymized text snippets are sent to Cohere (US-based API) strictly for temporary inference with a "non-training" guarantee.

**When?**
*   **Status:** Architecture is currently defined as of Jan 2026. A pilot is planned for 500 concurrent users.

---

## Part 2: Describe the Scope

**1. Describe the flow of personal information**
1.  **Collection:** The Child User enters text (Story Content) and Metadata (Genre, preferences) into the Browser.
2.  **Transmission:** Data is transmitted via HTTPS/WebSocket to the Canadian App Server.
3.  **Storage:** Story content is encrypted server-side and stored in a local SQLite database within the Canadian container.
4.  **AI Inference (Disclosure):**
    *   The system extracts a "Context Window" (only the last 4-6 sentences).
    *   This snippet is sent to Cohere (3rd Party) via secure API.
    *   **Constraint:** User PII (Name, Email) is **stripped**; only text context, Age, and Genre are injected.
    *   **Return:** The API returns a Socratic question (not story text).
5.  **Retention:** Story content is auto-saved every 2 seconds. AI Chat history is ephemeral (memory only) and deleted when the session ends.

**2. Who manages, accesses and uses the system?**
*   **Supervisors (Teachers/Parents):** Manage configuration, API keys, and sensitive settings via a "Teacher Gate" (PIN/Challenge).
*   **Child Users:** Access the writing interface. They own the IP of the story.
*   **System:** Automated services manage encryption and storage.

**3. Are there any linkages to other systems?**
*   **Current:** Linkage to Cohere API for intelligence.
*   **Future (Potential):** Integration with divisional library licenses (e.g., Sora/OverDrive) is considered but currently subject to legal review.
*   **Authentication:** Future support for MS Azure SSO is planned (Out of Scope for MVP).

**4. Do you anticipate any potential future enhancements?**
*   **Scale:** Migration from single-node VPS to multi-node scaling with Load Balancers.
*   **Features:** "Publisher Engine" (Print-on-Demand) and RAG "Mentor" Engine.

**5. Are there any potential future uses of information?**
*   **Research:** Limited to strictly anonymized metadata or small snippets, and strictly on an **Opt-In** basis for parents/guardians (never opt-out).

---

## Part 3: Collection, use and disclosure of personal information

**1. Authority for collection, use and disclosure**
*   **Authority:** The system operates under a "Rights-First Data Governance" framework designed to comply with Canadian data sovereignty requirements.
*   **Purpose:** Collection is authorized strictly for the educational purpose of the writing activity and the functionality of the software.

**2. Categories of personal information**
*   **Identity Information:** Account credentials (Teacher/Parent).
*   **Demographic Information:** Age (Note: Date of Birth is collected to *calculate* age but is **NOT persisted**; only the derived age integer is used).
*   **User Generated Content:** Story text (IP), Character names, User notes.
*   **Metadata:** Learning preferences (Themes, Fonts), Inactivity data (typing speed/pauses) used for "Affect Detection".

**3. Source and accuracy of personal information**
*   **Source:** Direct input from the Child User and configuration by the Supervisor.
*   **Accuracy:** Users (Children) create the content; Supervisors control the accuracy of account settings.

**4. Notification statements**
*   **Transparency:** The architecture dictates that the "No Ghostwriting" rule is an invariant firewall. Users are notified that the AI will *never* write for them.
*   **Data Location:** The architecture explicitly markets the benefit of Canadian hosting to schools and parents to address the "Privacy Paradox".

---

## Part 4: Access Rights for Individuals

**Ability to provide access and correction**
*   **Access:** The architecture ensures "Digital Object Permanence." Stories are auto-saved and exist exactly as the child left them.
*   **Correction:** The "Editor" phase and "Polishing Station" are built-in workflows designed specifically for the user to review and correct their own information (stories).
*   **Ownership:** StoryFort explicitly treats the story as the child's property, ensuring they have full rights to access and export (future PDF/EPUB) their work.

---

## Part 5: Privacy and Security Measures

**1. Security safeguards**

*   **i. Administrative safeguards**
    *   **Supervisor Primacy (GP-3):** Configuration of AI models and strictness settings is the domain of the Supervisor. These features are gated behind a PIN or Challenge.
    *   **Opt-In Research:** Data collection for research is strictly opt-in.

*   **ii. Technical safeguards**
    *   **Encryption:** Server-Side Encryption at Rest. Story content is encrypted to the account.
    *   **No Training (GP-1):** The API agreement with Cohere strictly forbids using user data for model training.
    *   **In-Memory Chat:** AI conversations ("TutorNotes") are volatile and do not persist to the database. They are wiped upon session disconnect.
    *   **Privacy Filters in Logs:** Logging (Serilog) is explicitly configured to *exclude* `Story.Content` to prevent creative work from leaking into plaintext server logs.
    *   **Input Guardrails:** "Bad Word" regex filters on AI inputs (identified as a Medium Risk gap currently in progress).

*   **iii. System audit functions**
    *   **Auditable Actions:** Supervisor controls allow for the auditing of agent activities.

**2. The location of the personal information**
*   **Electronic Records:** Hosted in a Docker container on a Virtual Private Server (VPS) located in Canada (Montreal or Toronto data centers).
*   **Volatile Data:** In-memory state (RAM) is held on the Canadian server.

**3. Will any personal information be stored by organizations outside Manitoba? Canada?**
*   **Storage:** NO. Persistent storage is strictly Canadian.
*   **Processing:** YES. Anonymized text segments are sent to the Cohere API (US/Cloud) for *processing only*. No data rests there.
*   **Rationale:** Use of specific "Reasoning" models (Command R+) required for the "Spark Protocol" which are accessed via API.

**4. Records retention and destruction**
*   **Retention:** Story content is retained in SQLite until deleted by the user/supervisor.
*   **Destruction (Ephemeral):** Chat logs with the AI are destroyed immediately upon session termination.
*   **Destruction (Account):** Dashboard allows for the deletion of stories.

**5. Information Managers**
*   **Third Party:** Cohere (AI Provider).
*   **Agreement:** The system relies on Cohere's enterprise-grade privacy commitments (GP-1), specifically the "non-training" agreement.

---

## Part 6: PIA Summary and Findings

**Description of Proposal**
StoryFort is a writing education tool hosted in **Canada** that uses AI to assist students without writing for them. It collects student stories and metadata, encrypts them locally, and uses a US-based third-party (Cohere) strictly for transient inference of anonymized text segments.

**Identified Risks & Mitigation**

1.  **Risk: Data Sovereignty / US Cloud Act**
    *   *Impact:* Student data exposed to foreign jurisdiction.
    *   *Mitigation:* **Containerized Core.** The database and full story content never leave the Canadian server. Only small, anonymized "context windows" are sent to the AI API.

2.  **Risk: AI Model Training (Data scraping)**
    *   *Impact:* Student IP used to train public AI models.
    *   *Mitigation:* **GP-1 Trusted AI Partnership.** usage of enterprise API keys that explicitly opt-out of model training. Data is not persisted on the inference side.

3.  **Risk: Generative AI "Hallucination" or inappropriate content**
    *   *Impact:* Student exposed to harmful text.
    *   *Mitigation:* **GP-2 "No Ghostwriting" Firewall.** The system is hard-coded to refuse generation of narrative prose. It can only output questions or structural templates. UI lacks "Insert" buttons.

4.  **Risk: Unauthorized Access**
    *   *Impact:* Student work accessed by peers or bad actors.
    *   *Mitigation:* **Encryption & Supervisor Gate.** Stories are encrypted at rest. Sensitive settings require a Teacher PIN.

**Conclusion**
The "StoryFort" architecture proactively addresses privacy risks through its **Sovereign Hybrid Engine**. By separating the *storage* of data (Canada) from the *intelligence* processing (API), and by stripping PII before inference, it meets the functional needs of the classroom while adhering to the principles of data privacy and protection.

**Submitted by:** StoryFort Architecture Team
**Date:** January 24, 2026
