# Unstickd: Requirements Document

## 🎯 Vision
Unstickd is a child-focused creative writing tool that uses AI to provide passive, Socratic guidance without ghostwriting. The app simulates a "notebook" experience with themed pixel-art companions called "Unstickrs."

---

## 👤 Target User
- **Primary**: Children (age-gated via Date of Birth)
- **Use Case**: Creative writing practice with gentle AI guidance
- **Environment**: Localhost only (no authentication required)

---

## 🏛️ Core Principles (Unbreakable Rules)

### Rule 1: Passive Observation
The child **never** directly prompts the AI. The system observes inactivity and responds proactively with questions.

### Rule 2: No Ghostwriting
The AI **never** generates story prose. It only asks leading questions using Socratic teaching methods.

### Rule 3: Separation of Powers
- **Drafting**: The child writes in flow state (Passive AI).
- **Reviewing**: The AI points out vocabulary or style improvements only during explicitly triggered review modes (Learning Mode).

---

## 📋 Functional Requirements

### 1. Onboarding Flow
**FR-1.1: Age Gate**
- Display Date of Birth input on first launch
- Calculate age (do NOT persist DoB)
- Use age to filter theme options

**FR-1.2: Theme Selection**
- Display grid of themed "worlds" (e.g., Detective, Knight, Astronaut)
- Each theme defines:
  - Color palette (CSS variables)
  - Font family
  - Pixel-art Unstickr sprite
  - UI Skinning (Backgrounds, Borders, Icons)
- Selection triggers visual transition (see UI Requirements)

### 2. Story Editor (The Notebook)
**FR-2.1: Page-Based Architecture**
- Stories are divided into **Pages** (not infinite scroll)
- Each page contains:
  - Rich text content (HTML)
  - Page number
  - Previous/Next navigation
- Target: Simulates a physical notebook

**FR-2.2: Rich Text Editing**
- Basic formatting: Bold, Italic, Underline, Font Size
- Use existing `Blazored.TextEditor` (QuillJS wrapper)
- Auto-save to `StoryState` on page navigation

**FR-2.3: Page Navigation**
- "Next Page" button creates new page if at end
- "Previous Page" loads existing content
- Future: Page-flip animation

### 3. The Unstickr (AI Tutor)
**FR-3.1: Activity & Affect Detection**
- JavaScript monitors keyboard, mouse, and text entry patterns.
- **Triggers**:
  - **Inactivity**: Idle > 10-30 seconds.
  - **Rage Clicks**: >3 clicks on same coordinates < 1s.
  - **Button Mashing**: High-velocity random keystrokes followed by deletion.
  - **Repetition**: Frequency of non-stop-words > threshold.
- Outcome: Triggers `TutorState.OfferingHelp` immediately or sets `ReviewIntent`.

**FR-3.2: Visual Presentation**
- Pixel-art character animates/glows to draw attention
- Displays a speech bubble with ONE question
- No input field for user replies
- Questions are contextual to current page content

**FR-3.3: LLM Service Abstraction**
- Interface: `ITutorService` with three implementations:
  1. **MockTutorService**: Returns pre-written Socratic questions
  2. **OllamaTutorService**: Calls local Llama3 (http://localhost:11434)
  3. **GeminiTutorService**: (Placeholder) Calls Google API
- Configuration: Switch via `appsettings.json` (`"TutorProvider": "Mock"`)
- System Prompt: Explicitly forbids generating prose; only questions

**FR-3.4: Spark Protocol (The Electric Starter)**
- **Trigger**: `Story.Length == 0` (Blank Page).
- **Process**: "Divergent -> Convergent" brainstorming loop (max 4 turns).
  1. Sensory Question (The Wish)
  2. Attribute Narrowing (The Vibe)
  3. Scenario Prompt (The Scenario)
- **Output**: JSON `{"status": "READY_TO_WRITE"}` triggers "Spark Handoff".

**FR-3.5: Review Protocol (The Learning Loop)**
- **Trigger**: User opts-in or Affect Detection flags `ReviewIntent`.
- **Orthographic Coaching**: Identifies rule broken (e.g., "Bossy-E") and offers rules, not just corrections.
- **Style Coaching**: Quotes famous authors from `Story.Genre` to suggest vocabulary improvements (e.g., "Doyle called it an 'immense hound'").

### 4. Notebook Entities (Character/Place Management)
**FR-4.1: Global "Toy Box"**
- Entities exist independently of stories
- Entity Types: Character, Place (extensible to custom types)
- Fields:
  - Name
  - Description
  - Type (enum)
  - Metadata (JSON blob for flexible attributes)

**FR-4.2: Many-to-Many Linking**
- A Story can reference multiple Entities
- An Entity can appear in multiple Stories
- Join table: `StoryReferences` (StoryId, NotebookEntryId)

**FR-4.3: Continuity Aide**
- When viewing an Entity, display: "Appears in: Story A, Story B"
- (MVP: Simple list. Future: LLM-generated summary of actions)

**FR-4.4: Entity Pages**
- `/characters`: List + Create Character form
- `/places`: List + Create Place form
- Forms include: Name, Description, Custom Metadata fields

### 5. Story Management
**FR-5.1: Dashboard**
- Display list of user's stories
- Show: Title, Theme, Last Modified Date
- Actions: Open, Delete

**FR-5.2: Save/Load**
- Stories save to SQLite automatically
- "New Story" creates empty Story record
- "Load Story" navigates to selected story's first page

**FR-5.3: Persistence**
- Database: SQLite with Entity Framework Core
- Tables: Stories, StoryPages, NotebookEntries, StoryReferences

### 6. Theme System
**FR-6.1: Theme Definitions**
- Presets: Detective, Knight, Astronaut, Fairytale, Spooky (5 minimum)
- Each theme includes:
  - Primary/Secondary/Accent colors
  - Background texture (optional)
  - Font family (e.g., Courier, MedievalSharp, Comic Sans)
  - Unstickr sprite path

**FR-6.2: Runtime Theme Switching**
- User can change theme mid-story (Settings menu)
- CSS variables update instantly
- New theme persists to Story record

**FR-6.3: Visual Transition (MVP)**
- "Wipe" effect: Horizontal sweep from left to right
- As wipe passes over:
  - Background color transforms
  - Unstickr sprite swaps
  - Font family changes
- Duration: ~1.5 seconds

### 7. The Library (Reader)
**FR-7.1: Reader Interface**
- Distraction-free reading view separate from Editor.
- Typography: Large serif font (e.g., Merriweather).
- Content: Public Domain Corpus texts defined in `Corpus_Definition.json`.

**FR-7.2: Text Pinning**
- Interaction: Select text -> "Pin" -> Target Notebook.
- Result: Creates `NotebookEntry` (Type: Quote) with metadata (SourceBook, Author).

**FR-7.3: Book Unlock System**
- Trigger: `Story.Status` = Completed.
- Logic: Unlock book matching `Story.Genre` / `Story.Archetype`.
- Visual: Celebration Modal.

### 8. Creative Tools
**FR-8.1: The Blender**
- Interface for mixing story elements (Characters, Settings, Themes).

**FR-8.2: Spark Handoff**
- Logic: Transitions from Spark Protocol to Editor.
- Action: Creates temporary "Idea" entry, dismisses Tutor, focuses Editor.

**FR-8.3: RAG Bridge**
- Static `Corpus_Definition.json` maps weak words to genre-specific strong words and provides full text for Reader.

### 9. Teacher Tools (The Classroom)
**FR-9.1: Assignment Architecture**
- **Data Model**: `Assignment` is a specialized `NotebookEntry` (Type: `Assignment`).
- **Content**: Prompt text, Grading Rubric (Optional), Resource Links.
- **Context Injection**: Allows defining a "Target Corpus" (e.g., "Use vocab from 'Sea Wolf'") and "Custom Nudges" (System Prompt overrides).

**FR-9.2: The Split-View Workspace**
- **Layout**: When opening an Assignment, the Editor enters "Workshop Mode".
- **Left Pane**: The Assignment details (Always visible).
- **Right Pane**: The Student's Draft.
- **Goal**: Constant reference to the prompt without switching tabs.

**FR-9.3: Adaptive Tuning (The Thermostat)**
- **Role**: Teachers (not students).
- **Controls**: "Patience" slider (Idle Timeout: 5s - 60s).
- **Semantics**: "Faster" (More help) vs "Slower" (More independence).
- **Security**: Settings are behind a simple "Teacher Gate" (e.g., Math problem or PIN).

---

## 🎨 UI/UX Requirements

### UX-1: Notebook Aesthetic
- All pages simulate physical notebook pages
- Lined paper texture (subtle)
- Handwritten-style fonts where appropriate
- Warm, inviting color schemes

### UX-2: Child-Friendly Navigation
- Large, obvious buttons
- Icons + Text labels
- No complex menus or settings
- Persistent "Home" button

### UX-3: The "Wow" Factor
- **Primary**: Theme Door Transition (see FR-6.3)
- **Secondary**: Unstickr Animation (gentle bounce/glow)

---

## 🔧 Technical Requirements

### TECH-1: Stack
- Framework: .NET 10 Blazor Web App (Interactive Server)
- Database: SQLite + Entity Framework Core
- Rich Text: Blazored.TextEditor (QuillJS)
- LLM: Ollama (local), Gemini (cloud, future)

### TECH-2: Architecture
- **State Management**: Scoped `StoryState` service for in-session persistence
- **Service Layer**: Abstracted `ITutorService` for swappable LLM backends
- **Client-Side JS**: Inactivity detection, animation triggers, **Input Pattern Analysis (Rage/Mashing)**.

### TECH-3: Data Model
```
Stories
  - Id (PK)
  - Title
  - ThemeId
  - CreatedDate
  - LastModifiedDate

StoryPages
  - Id (PK)
  - StoryId (FK)
  - PageNumber
  - Content (HTML)

NotebookEntries
  - Id (PK)
  - Name
  - Description
  - Type (Character|Place|Custom|Quote|Assignment)
  - Metadata (JSON)

StoryReferences (Join Table)
  - StoryId (FK)
  - NotebookEntryId (FK)

Themes
  - Id (PK)
  - Name
  - ColorPrimary
  - ColorSecondary
  - FontFamily
  - SpritePath
```

### TECH-4: Configuration
- `appsettings.json`:
  ```json
  {
    "TutorProvider": "Mock",
    "OllamaUrl": "http://localhost:11434",
    "GeminiApiKey": ""
  }
  ```

---

## 🚫 Out of Scope (For MVP)
- Authentication/User Accounts
- Cloud deployment
- Image generation for illustrations
- Audio narration
- Multiplayer/Collaboration
- Advanced continuity tracking (character death/revival logic)
- Custom theme creation (user-defined palettes)
- Export to PDF/EPUB
- **Post-MVP**:
  - The RAG "Mentor" Engine (Vector DB)
  - The "Publisher" Engine (Print-on-Demand)
  - Blueprint Data Seeding
  - Advanced Analytics

---

## 📊 Success Criteria
1. A child can complete the onboarding flow and select a theme in < 30 seconds
2. The Unstickr appears within 15 seconds of inactivity
3. Theme switching completes the transition animation without errors
4. A child can create a Character, link it to a Story, and view the relationship
5. Stories persist across browser refreshes
