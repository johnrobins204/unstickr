# UI/UX Detailed Requirements

This document serves as the detailed specification for downstream Agent implementation. It breaks down high-level user journeys (Micro-Stories) into granular technical and design requirements.

## MS-1: The "Clean Slate" Entry
**User Journey**: The user enters the editor for a new story. The page is blank.
**Core Philosophy**: "Invitation, never direction." The interface must welcome the child into their own world, not tell them what to write.

### 1. Immersive Theming (The "World")
**Requirement**: The Theme is strictly a UI/UX "Skin" and has zero bearing on the logical content of the story. It exists to foster immersion.
**Scope of Customization**:
-   **Color Palette**: Primary, Secondary, Background, Surface.
-   **Typography**: Headings and Body font (e.g., *MedievalSharp* for Fantasy, *Orbitron* for Space).
-   **Avatar**: The "Unstickr" (Alice) changes visual identity (e.g., Wizard, Robot, Detective).
-   **Environment**:
    -   Page background texturing (Parchment vs. Blueprint vs. Logbook).
    -   Editor borders and button shapes.
    -   Soundscapes (Optional future req: Ambient hum vs. nature sounds).

**Impl Notes**:
-   Extend `Theme` entity to include `CssDefinition` (JSON/Blob) or granular columns for these properties.
-   Frontend must inject these as CSS Variables on root or a wrapper container.

### 2. Editor Focus (The "Canvas")
**Requirement**: Minimize "Setup" friction.
-   **Auto-Focus**: Upon load, the text cursor must immediately blink in the first line of the Body content.
-   **Title De-emphasis**: The "Title" field should be secondary, perhaps "Untitled" by default, or auto-generated later. It should not block the user from writing the first sentence.
-   **Visual Hierarchy**: The blank page should look like "Ready to receive" (e.g., faint lines, generous padding) rather than "Empty void."

### 3. The Tutor's "Invitation"
**Requirement**: Address the "Blank Page" fear without being pushy.
-   **Initial State**: The Tutor is visible but passive.
-   **Welcome Message**: A single, non-blocking greeting bubble that fades after a few seconds.
    -   *Good*: "Ready for launch, Captain!" (Space Theme)
    -   *Bad*: "Try writing about a spaceship." (Directional)
-   **Intervention**: If the user sits for > 20s (Generative Pause) on a blank page:
    -   Tutor encourages: "I'm right here if you need a spark."
    -   **Strict Rule**: Do not offer plot ideas unless explicitly clicked.

### 4. Technical Tasks for Implementation
1.  **CSS Architecture**: Refactor `app.css` to use CSS Variables for all thematic elements.
2.  **Focus Logic**: Update `Editor.razor` `OnAfterRenderAsync` to call a JS function `focusQuillEditor()`.
3.  **Avatar Logic**: Ensure `TutorPanel.razor` loads the Sprite URL dynamically from the current `Story.Account.Theme`.
4.  **Tutor State**: Implement a localized "Onboarding/Welcome" state for the `TutorService` distinct from general inactivity.

## General UX & Inclusivity Standards
**Core Philosophy**: The interface must be physically forgiving, cognitively low-load, and accessible to neurodiverse users.

### 1. Inclusivity (Accessibility)
-   **Dyslexia Support**:
    -   **Requirement**: Add a toggle in Settings/Editor to enable "Readability Mode".
    -   **Specs**: Overrides current Theme font with *OpenDyslexic* or *Comic Sans*. Increases `line-height` by 1.5x.
    -   **Source**: *British Dyslexia Association*.
-   **Sensory Control (Autism/ADHD)**:
    -   **Requirement**: Add "Zen Mode" / "Reduced Motion" toggle.
    -   **Specs**: Disables Avatar bobbing/blinking. Disables background textures if they are high-contrast. Mutes all potential UI sounds.
    -   **Source**: *Porayska-Pomsta, K., et al.* (The ECHOES Project).

### 2. Interaction Design (Child Ergonomics)
-   **Motor Forgiveness**:
    -   **Requirement**: All clickable targets (Arrows, Tabs, Icons) must have a hit-box of at least 48x48px (regardless of visual icon size).
    -   **Rationale**: Compensate for developing fine motor control (Fitts's Law).
    -   **Source**: *Nielsen Norman Group* (Alita Joyce).
-   **Safe Exploration**:
    -   **Requirement**: "Destructive" actions (Delete Story) must not be a single click.
    -   **Specs**: Use a "Hold to Delete" interaction or a "Trash Can" drag-and-drop metaphor rather than a simple "Are you sure?" modal.
    -   **Source**: *Hanna, L., et al.* (Guidelines for Usability Testing with Children).
-   **Literal Metaphors**:
    -   **Requirement**: Iconography must be concrete.
    -   **Specs**: "Theme" = Paintbrush/Costume (Not a Gear). "Save" = Safe/treasure chest/Checkmark (Not a Floppy Disk).
    -   **Source**: *Piaget, J.* (Concrete Operational Stage).

## MS-4: The "Safe Exit" (Persistence Strategy)
**User Journey**: The user stops writing and leaves the app.
**Core Philosophy**: "Digital Object Permanence". The world must exist exactly as they left it, without requiring administrative chores.

### 1. The "Death of the Save Button"
**Requirement**: Remove the manual "Save" button entirely.
-   **Guidance**: Manual saving is a legacy pattern that induces anxiety ("Did I click it?"). Modern game design (Minecraft, Animal Crossing) and productivity tools (Docs, Notion) rely on continuous state persistence.
-   **Method**: **Debounced Auto-Save**.
    -   System detects keystrokes.
    -   Reset timer on every keystroke.
    -   Trigger save `2000ms` (2s) after *last* keystroke.
    -   **Technical Constraint**: Since this is Blazor Server, we must debounce on the client (JS) or carefully on the server to avoid flooding the WebSocket.

### 2. Trust Indicators (The "Dirty State")
**Requirement**: Provide visual assurance that the system is working.
-   **State 1 (Editing)**: While typing + during the 2s debounce window.
    -   *Visual*: A subtle animation (e.g., a spinning quill, a "thinking" localized icon, or simply "Saving...") in the header.
-   **State 2 (Saved)**: After efficient DB commit.
    -   *Visual*: "Saved" text or a discrete "Cloud Checkmark". The indicator should fade out or become static after 5s.
-   **State 3 (Error)**: If the WebSocket disconnects or DB fails.
    -   *Visual*: **High Visibility**. "Not Saved - Offline". Red indicator.
    -   *Action*: Disable the text editor to prevent data loss (User keeps typing into a void).

### 3. Exit Hooks
**Requirement**: "Assume the child wants to save."
-   **Navigation Interception**:
    -   If the user clicks "Home" or "Back" *while* the state is dirty (inside the 2s window), force an immediate synchronous save before navigation occurs.
    -   Implementation: Use `LocationChangingHandler`.

### 4. Technical Tasks for Implementation
1.  **Remove UI**: Delete the Save button from `Editor.razor`.
2.  **Debounce Logic**: Implement a `System.Timers.Timer` in `Editor.razor` (C#) or a `setTimeout` loop in JS Interop (Preferred for responsiveness).
3.  **Status Component**: Create a `SaveStatusBadge.razor` that subscribes to the Editor's state.
4.  **Error Handling**: Create a `ConnectionLost` overlay.

## MS-2: The "Passive Intervention" (The Bubble)
**User Journey**: The user stops typing for an extended period.
**Core Philosophy**: "Socratic Scaffolding & Locus of Control." The AI observes and offers support but never interrupts the user's agency.

### 1. Inactivity Logic (The "Generative Pause")
**Requirement**: Distinguish between "Thinking" and "Stuck."
-   **Static Timer IS NOT ENOUGH**: A 10s timer interrupts a child deep in thought.
-   **Context-Sensitive Logic**:
    -   *Mid-sentence pause*: 10 seconds. (Transcription block).
    -   *End-of-sentence/. pause*: 30 seconds. (Generative/Planning block).
-   **Refinement**: If the user is *scrolling* or clicking notebook tabs, they are NOT inactive. Input detection must include mouse/scroll events, not just keystrokes.

### 2. The Interaction Model (Ambient Signaling)
**Requirement**: Low Cognitive Load. No sudden movements.
-   **Stage 1: The "Looking" State (Pre-Intervention)**
    -   After ~50% of the pause time.
    -   Avatar eyes open/look at the user (or subtle idle animation). No text.
-   **Stage 2: The "Offer" (The Trigger)**
    -   Pause time limit reached.
    -   **Visual**: A small, static "speech bubble" icon or a gentle "glow" around the avatar.
    -   **Text**: "Pst...", "Need a hand?", or a themed variant ("Signal coming in...").
    -   **Behavior**: Fades in slowly (2s duration). Does NOT make sound. Does NOT block the editor.

### 3. The Intervention (Click-to-Reveal)
**Requirement**: The user must explicitly *ask* for the content.
-   **Action**: User clicks the bubble/avatar.
-   **Result**: The Tutor Panel expands (or bubble expands).
-   **Content**:
    -   **Socratic Question**: "What does the dragon smell like?" (Sensory prompt).
    -   **Reflective Question**: "Do you know where they are going next?" (Plot prompt).
-   **Validation**: The child can dismiss it ("I know what I'm doing") or answer it.
-   **Strict Rule**: The AI intervention closes/minimizes immediately once the user starts typing again.

### 4. Technical Tasks for Implementation
1.  **Refactor InactivityService**: Move JS inactivity logic to handle scroll/mouse events and report "Idle Type" (Mid-sentence vs End-of-sentence) to Blazor.
2.  **UI States**: Implement `TutorState` enum: `Idle`, `Watching`, `Offering`, `Active`.
3.  **Animation**: Add CSS transitions for the "fade in" of the bubble.
4.  **Rebound Timer**: Implement "Short-term Rebound" logic. If user resumes typing but stops again within < 5s, bypass Stage 1 and jump straight to Stage 2 (Offering Help), assuming they are still stuck.
5.  **Input Interruption**: Add event listener on Editor to `Unstickd.dismissTutor()` on first keystroke.

## MS-3: The "Quick Reference" (Notebooks)
**User Journey**: The user needs to verify a fact (e.g., character eye color) or create a new entity (e.g., a magic sword) without losing their writing flow.

### 1. The UX Pattern (Peek, Don't Switch)
**Requirement**: The Notebook panel must feel like a "reference card" held next to the book, not a separate room.
-   **Current Issue**: The Offcanvas sidebar obscures 30-40% of the screen.
-   **Refinement**: 
    -   When writing, valid screen real-estate is precious.
    -   **Tablet/Desktop**: The sidebar should *squeeze* the editor viewport (flexbox) rather than overlaying it, so no text is covered.
    -   **Mobile**: Overlay is acceptable, but with transparency or "Peek" height (bottom sheet).

### 2. Interaction Design (Link & Insert)
**Requirement**: Minimize typing for existing entities.
-   **Draggable Entities**: Users should be able to drag a Character card from the sidebar and drop it into the text editor.
    -   *Result*: Inserts the character's Name as clear text.
-   **Smart Linking (Future)**: If the user types "Gandalf", the system should subtly highlight it (after a delay) to show it recognizes the entity. (Backlog item).

### 3. Creation flow (Inline)
**Requirement**: Creating a new entity should not feel like "filling a form."
-   **The "Quick Add" Row**:
    -   Input: `[ Icon ] [ Name ] [ + ]`
    -   Location: Sticky top of the Notebook panel.
    -   Behavior: Hitting "Enter" or "+" immediately:
        1.  Creates the Entity.
        2.  Links it to the Story.
        3.  Adds it to the list below.
        4.  *Crucially*: Keeps focus in the Input (for rapid batch creation) OR returns focus to Editor (configurable?).

### 4. Technical Tasks for Implementation
1.  **Layout Refactor**: Change `NotebookPanel` from `Offcanvas` (Overlay) to a CSS Grid Column or Flexbox sibling to `Editor`. Add a toggle button to "Pin" it open.
2.  **Drag-and-Drop**: Implement HTML5 Drag API on `NotebookEntity` items and a Drop handler in QuillJS.
3.  **Quick-Create Refactor**: Simplify the "New Entity" form to a single line.

## MS-5: The "Learning Editor" (Spelling)
**User Journey**: The child finishes a burst of writing and pauses. The system encourages a "Review" session to polish the text and stimulate new ideas through analysis.
**Core Philosophy**: "Separation of Powers". Never interrupt the flow. Spell check is a separate mode that teaches, not corrects.

### 1. Default State: The "Flow" Zone
**Requirement**: No visual distractions during drafting.
-   **Rule**: `spellcheck="false"` is hard-coded on the Editor during active typing.
-   **Rationale**: "Red Pen Anxiety" inhibits creativity.

### 2. The Trigger: "Review Mode"
**Requirement**: The Tutor suggests a review session when natural pauses occur, treating "clean up" as a constructive break.
-   **Trigger Conditions**:
    1.  User has typed > 50 words since last review.
    2.  User pauses for > 15s (End-of-Thought Pause).
    3.  Spell check logic (background) detects > 3 errors.
-   **The Offer**: Avatar signals "Offering Help".
    -   *Bubble Text*: "Good burst! Want to polish those words?" or "Shall we check the spelling together?"
    -   *Action*: User clicks Avatar to enter **Review Mode**.

### 3. Review Mode Interaction (Scaffolded Correction)
**Requirement**: The system guides the user to self-correct using a "Progression of Nudges".
-   **State 1: Discovery (The Hunt)**
    -   Tutor: "I see a tricky word. Can you find it?"
    -   *Visual*: No highlights yet.
    -   **Escalation**: If user is idle for > 10s or clicks "Show me":
        -   **Action**: System highlights the misspelled word(s).
        -   Tutor: "Here it is! Let's look at this one."
-   **State 2: The Lesson (The Fix)**
    -   **Nudge 1 (Awareness)**: "Does that look right? Try sounding it out."
    -   **Escalation (Time-Based)**: If user hovers/stares for > 5s:
        -   **Nudge 2 (Rule/Phonetic)**: "It sounds like 'sh', but we usually use 'tion'."
    -   **Escalation (Frustration)**: If user fails again:
        -   **Nudge 3 (Explicit Rule)**: "Remember: 'I before E except after C'."
    -   **Resolution**: User types the correction. Tutor celebrates: "Perfect!"

### 4. Technical Tasks for Implementation
1.  **Spellcheck Toggle**: Bind `spellcheck` attribute of Quill editor to a Blazor boolean `IsReviewMode`.
2.  **Background Analysis**: Implement a background service (or simple JS script) that counts potential errors without highlighting them, to inform the Tutor's "Offer" logic.
3.  **Tutor Prompting**: Update `TutorService` to handle `ReviewIntent`.
    -   *Constraint*: The Prompt must inject the misspelled word and ask the LLM for "A phonetic hint for a child, not the answer."

## MS-7: The "Structural Scaffold" (Story Blueprints)
**User Journey**: The child wants to write a "Mystery" but doesn't know where to start. They select a "Blueprint" derived from a classic story (e.g., *Sherlock Holmes*), which pre-populates their Notebook with archetypes to fill in.
**Core Philosophy**: "Standing on the shoulders of giants." Learning structure by emulating successful patterns from literature.

### 1. Decoupling Theme vs. Blueprint
**Requirement**: Visuals (Theme) and Structure (Blueprint) must be independent selections.
-   **Theme (Skin)**: "Spooky House", "Spaceship". (Visuals: Fonts/Colors).
-   **Blueprint (Skeleton)**: "The Golden Ticket", "The Missing Detective". (Data: Notebook Entries).
-   **Freedom**: A child can write a "Golden Ticket" story inside a "Spaceship" visual theme.

### 2. The Blueprint Data Model
**Requirement**: A Blueprint is a template of empty Notebook Entities derived from literary analysis.
-   **Source Material**: Public Domain Classics (e.g., *Wizard of Oz*, *Treasure Island*).
-   **Extraction Logic (Offline/Pre-calc)**:
    1.  LLM analyzes the classic story.
    2.  Extracts Archetypes: "The Unlikely Hero", "The Magical Object", "The Mentor".
    3.  Extracts Plot Beats: "The Call to Adventure", "The Low Point".
-   **User Experience**: When applied, the user's Notebook is seeded with these entries as *Prompts* (e.g., Name: "Who is the Mentor?", Description: "In *Treasure Island*, this was Long John Silver. Who is it in your story?").

### 3. Selection Interaction
**Requirement**: "Pick your adventure shape."
-   **UI**: A "Blueprint Chooser" separate from the "Theme Chooser".
-   **Filtering**: By Age Group (LLM Classified) and Genre.
-   **Preview**: "This blueprint is good for stories about finding lost things." (Simple language).

### 4. Technical Tasks for Implementation
1.  **Data Model**: Create `Blueprint` and `BlueprintEntity` tables (seed data).
2.  **Seeding Pipeline**: Create a script/tool to ingest Gutenberg text -> LLM -> JSON Blueprint.
3.  **App Logic**: `AccountService.ApplyBlueprint(blueprintId)` copies the template entities into the user's `Notebooks` with a special "Unfilled" state.

## MS-6: The "Publisher" (Monetization & Loop)
**User Journey**: A story is complete. The child feels pride. A parent/teacher wants to celebrate the artifact.
**Core Philosophy**: "The Fridge Test." The digital effort must produce a tangible reward.

### 1. Two-Tier Export Logic
**Requirement**: Distinguish between "Educational Utility" (Free/School) and "Keepsake Quality" (Premium/Home).
-   **Tier 1: The Draft (School/Free)**
    -   *Format*: Clean PDF or HTML.
    -   *Styling*: Basic formatting (Times Roman), functional.
    -   *Use Case*: Handing in homework to a teacher.
-   **Tier 2: The Book (Home/Premium)**
    -   *Gate*: Paywall or License check.
    -   *Format*: ePub, High-Res PDF (Print Ready), or "Flipbook" web view.
    -   *Styling*: Applies the visual *Theme* (Fonts, Borders, Drop-caps) to the document. Injects the child's created Entity definitions (e.g., Character cards) as an appendix.

### 2. The "Trophy Room" (Dashboard)
**Requirement**: Visualizing progress.
-   **Visuals**: Finished stories move from the "Workbench" to the "Library" shelf with a distinct gold spine or cover.
-   **Sharing**: Parent-gated "Share to Family" link (generates a read-only secure web view).

### 3. Technical Tasks for Implementation
1.  **Export Engine**: Implement a Razor-to-PDF library (e.g., QuestPDF or Puppeteer).
2.  **License Logic**: Add `Account.SubscriptionTier` check to the "Publish" button.
3.  **Layout Designer**: Create CSS `@media print` styles that map the screen Theme variables to print-safe values (e.g., dark mode screen -> light mode print).

## MS-5: Wireframe-Inspired Enhancements
**User Journey**: The wireframe introduces a dynamic and immersive design lab, alongside a refined sidebar and editor layout.

### 1. Design Lab (Customization)
**Requirement**: Allow users to personalize their interface with real-time previews.
- **Color Mixer**:
  - Users can adjust primary, background, and card colors.
  - Changes are reflected immediately via CSS variables.
- **Typography Station**:
  - Offers font families grouped by style (Game, Inclusive, Creative).
  - Includes a text size slider (12px-24px) with live updates.
- **Shape Shifter**:
  - Provides options for blocky (0px radius) or rounded (12px radius) UI elements.

### 2. Sidebar Enhancements
**Requirement**: Improve navigation and integrate writing stats.
- **Dynamic Buttons**:
  - Sidebar buttons highlight active views and provide hover animations.
  - Includes "Save" and "Word Count" widgets for quick access.
- **Avatar Integration**:
  - Displays a pixel-art avatar (e.g., Alice) with status updates.
  - Avatar acts as a subtle, non-intrusive companion.

### 3. Editor Refinements
**Requirement**: Enhance the writing experience with intuitive controls.
- **Story Details Panel**:
  - Collapsible header with fields for title, genre, and archetype.
  - Includes quick font and formatting options.
- **Content Area**:
  - Full-screen textarea with placeholder text.
  - Subtle background elements (e.g., faint dragon icon) for thematic immersion.

### 4. Notebooks Panel
**Requirement**: Organize story elements visually.
- **Grid View**:
  - Displays notebooks as cards with icons and descriptions.
  - Includes a "New Notebook" button with hover effects.
- **Detail View**:
  - Shows notebook content with options to add new entries.

### 5. Accessibility Improvements
**Requirement**: Ensure inclusivity for diverse users.
- **Dyslexia-Friendly Fonts**:
  - Comic Neue and Atkinson Hyperlegible are available.
- **Reduced Motion**:
  - Option to disable animations for users with sensory sensitivities.
- **High Contrast Mode**:
  - Ensures readability across all themes.

---

## Implementation Plan: Style Lab & UI Overhaul (Wireframe Integration)

### 1. CSS Architecture & Font Imports
- Migrate all CSS variables from the wireframe's `<style>` block to `app.css`.
- Import Google Fonts as in the wireframe for all font options.
- Use Tailwind utility classes where possible for expedience.

### 2. Theme State & Debounced Persistence
- Extend `StoryState` to hold a `ThemePreference` object (colors, font, border radius).
- On any change in the Design Lab, update CSS variables immediately via JS Interop.
- Debounce all changes (e.g., 2s after last input) before persisting to the database/account settings, mirroring the editor's debounce logic.

### 3. Sidebar & Layout Refactor
- Implement a fixed sidebar with navigation, word count, and the "Alice" avatar as per the wireframe.
- Use Lucide icons via the JS script (as in the wireframe) for immediate integration.
- Sidebar buttons should highlight active view and animate on hover.

### 4. Design Lab (Settings) Implementation
- Create a `DesignLab.razor` component for the settings panel.
- Add color pickers, font selectors, and border radius toggles as shown in the wireframe.
- Use JS Interop to update CSS variables in real time.
- All UI controls should update the preview instantly and persist after debounce.

### 5. Editor & Notebooks UI Refresh
- Refactor `Editor.razor` to use a collapsible header, full-height textarea, and background iconography (e.g., faint dragon).
- Update Notebooks to a card grid layout with icons and descriptions, matching the wireframe.

### Expediency Principle
- Use the fastest available method for iconography and CSS (e.g., Lucide JS, Tailwind, direct CSS variable manipulation).
- Prioritize user experience and maintainability over strict adherence to any one framework if a more expedient solution is available.








