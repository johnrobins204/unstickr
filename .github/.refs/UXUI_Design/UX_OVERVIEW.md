# Unstickd UX Architecture: The Story Fort

**Status:** Authoritative 
*(This document supersedes previous design specifications found in `UI_UX_detail_reqs.md` and redefines the aesthetic direction of all sub-requirements)*

## ⛺ Core Philosophy
The Unstickd User Experience is designed as a **"Story Fort"**—a private, magical sanctuary where the child feels safe to pretend their stories are real. 

Gone is the "Retro Game" aesthetic (referenced in early wireframes). Instead, we embrace **Immersive Creativity**. The interface behaves like a personalized creative space (a blanket fort, a treehouse, a spaceship cockpit) that the child constructs "from the inside out." The workspace is not just a tool; it is a manifestation of the story they are writing, decorated with themes, pictures, and artifacts that ground them in their narrative world.

---

## 🗺️ The Writing Lifecycle (Phases)

The application flow guides the writer through the creative process, but reframes these phases as stations within their Fort.

### 1. The Reading Nook (Inspiration)
*Formerly The Bookshelf*
**[📄 View Detailed Functional Reqs](reader_reqs.md)**
*(Note: Visual requirements shift from Pixel Art to "Cozy/Themed" aesthetic)*

*   **Atmosphere**: A warm, comfortable corner of the fort with soft lighting.
*   **Key Feature**: **The Immersive Reader** – Opening a book should feel like pulling a treasure off a shelf. The transition isn't digital; it's atmospheric.
*   **Workflow**: Children "collect" ideas (sentences/vocab) like physical tokens or pinned photos to use in their own workspace later.

### 2. The Map Room (Planning)
*Formerly The Planner*
**[📄 View Detailed Functional Reqs](planner_reqs.md)**
*(Note: Visual requirements shift from Pixel Art Mountains to "Hand-Drawn/Pinned Maps")*

*   **Atmosphere**: A large table covered in maps and diagrams.
*   **Key Feature**: **The Plot Wall** – The structure of the story is visualized not as a digital graph, but as a path pinned onto a corkboard or drawn on parchment.
*   **Inclusivity**: Supports global narrative traditions via different "Map Styles" (e.g., a Scroll for Kishōtenketsu, a Treasure Map for The Quest).

### 3. The Writing Desk (Drafting)
*Formerly The Drafter*
**[📄 View Detailed Functional Reqs](drafter_reqs.md)**
*(Note: Design Lab functionality expands to include Thematic Immersion)*

*   **Atmosphere**: The heart of the fort. This Viewport adapts to the user's imagination.
*   **Key Feature**: **Inside-Out Immersion** – If writing a Pirate story, the "desk" looks like weathered wood, the background might feature a porthole view of the ocean, and the "Design Lab" allows placing thematic stickers (parrots, coins) on the UI chrome.
*   **AI Monitoring**: Alice acts as a "Co-Pilot" or "Fellow Adventurer" sitting across the desk, monitoring flow without breaking the immersive spell.

### 4. The Lantern (Reviewing)
*Formerly The Reviewer*
**[📄 View Detailed Functional Reqs](reviewer_reqs.md)**
*(Note: Visuals shift from Retro Arcade to "Detective/Explorer Mode")*

*   **Atmosphere**: The lights dim, focusing a spotlight (The Lantern) on the text.
*   **Key Feature**: **The Investigation** – Reviewing isn't a game score; it's an exploration. Errors are "clues" to be uncovered.
*   **Coaching**: Alice holds the lantern, guiding the child to look closer at specific areas.

### 5. The Polishing Station (Editing)
*Formerly The Editor*
**[📄 View Detailed Functional Reqs](editor_reqs.md)**
*(Note: Visuals shift from Checklist to "Crafting Table")*

*   **Atmosphere**: A clean, bright workbench for final assembly.
*   **Key Feature**: **Crafting the Final Piece** – Turning flags into fixes feels like polishing a gem or tightening a bolt.
*   **Completion**: "Submitting" is sending the ship out to sea or launching the rocket—a moment of release and pride.

---

## 🛠️ Cross-Cutting Capabilities

### 🎨 The Design Lab: "Decorate Your Fort"
**[📄 Details in Drafter Reqs](drafter_reqs.md)**
*The Engine of Immersion.*
Profoundly expanded from simple font/color settings, the Design Lab is how the child claims ownership of the space.
*   **Theme Selection**: One-click total transformations (e.g., "Deep Space," "Enchanted Forest," "Detective Office") that change backgrounds, UI textures, and ambient sounds.
*   **Wall Gallery**: Users can pin "pictures" (generated assets or thematic icons) to the sidebar or headers, making the UI feel lived-in.
*   **Atmosphere Control**: Sliders for "Lighting" (Dark Mode/Light Mode) and "Cozy Factor" (UI softness/roundness).

### 📚 The Chest (Notebooks)
*Formerly Notebooks Management*
**[📄 Details in Drafter Reqs](drafter_reqs.md)**
*   **Visual Logic**: A physical chest or drawer system within the fort where collected "Treasures" (Characters, Places ratings) are kept safe.
*   **Integration**: Items pinned in the Reading Nook appear here as physical cards or polaroids.
