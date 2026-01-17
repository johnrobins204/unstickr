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
  - Optional story starter template
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
**FR-3.1: Inactivity Detection**
- JavaScript monitors keyboard activity in editor
- If idle > 10 seconds → Trigger Unstickr

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
- **Client-Side JS**: Inactivity detection, animation triggers

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
  - Type (Character|Place|Custom)
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

---

## 📊 Success Criteria
1. A child can complete the onboarding flow and select a theme in < 30 seconds
2. The Unstickr appears within 15 seconds of inactivity
3. Theme switching completes the transition animation without errors
4. A child can create a Character, link it to a Story, and view the relationship
5. Stories persist across browser refreshes
