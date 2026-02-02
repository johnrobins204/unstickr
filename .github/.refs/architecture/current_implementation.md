# StoryFort Architecture Diagram

## System Overview

This document provides a visual representation of the StoryFort architecture, including the component structure, data flow, and service dependencies.

## High-Level Architecture

```mermaid
graph TB
    subgraph "Client Layer"
        Browser[Web Browser]
        BlazorUI[Blazor Interactive Server Components]
    end
    
    subgraph "Presentation Layer"
        Pages[Razor Pages]
        Components[Shared Components]
        Layouts[Layout Components]
    end
    
    subgraph "Service Layer"
        SessionState[SessionState - Scoped]
        StoryContext[StoryContext - Scoped]
        TutorOrch[TutorOrchestrator - Scoped]
        ArchService[ArchetypeService - Singleton]
        CohereSvc[ICohereTutorService - Scoped]
        TokenSvc[TextTokenizer - Singleton]
        AchieveSvc[AchievementService - Scoped]
        SafeguardSvc[SafeguardService - Scoped]
        TutorSessionSvc[TutorSessionService - Scoped]
    end
    
    subgraph "Data Layer"
        AppDb[AppDbContext]
        SQLite[(SQLite Database)]
    end
    
    subgraph "External Services"
        CohereAPI[Cohere AI API]
    end
    
    Browser --> BlazorUI
    BlazorUI --> Pages
    BlazorUI --> Components
    BlazorUI --> Layouts
    
    Pages --> SessionState
    Pages --> TutorOrch
    Pages --> ArchService
    Components --> SessionState
    Components --> AppDb
    
    TutorOrch --> CohereSvc
    TutorOrch --> StoryContext
    TutorOrch --> SafeguardSvc
    CohereSvc --> CohereAPI
    
    SessionState --> AppDb
    StoryContext --> SessionState
    ArchService --> AppDb
    AppDb --> SQLite
    
    style Browser fill:#e1f5ff
    style BlazorUI fill:#b3e5fc
    style SessionState fill:#fff9c4
    style TutorOrch fill:#fff9c4
    style ArchService fill:#c8e6c9
    style SQLite fill:#ffccbc
    style CohereAPI fill:#f8bbd0
```

## Component Structure

```mermaid
graph LR
    subgraph "Pages"
        Home[Home.razor]
        Editor[Editor.razor]
        Planner[Planner.razor]
        Review[Review.razor]
        Polish[Polish.razor]
        Settings[Settings.razor]
        Notebooks[Notebooks.razor]
        Places[Places.razor]
        Admin[Admin.razor]
    end
    
    subgraph "Editor Panels"
        TutorPanel[TutorPanel.razor]
        NotebookPanel[NotebookPanel.razor]
    end
    
    subgraph "Layouts"
        MainLayout[MainLayout.razor]
        EmptyLayout[EmptyLayout.razor]
        NavMenu[NavMenu.razor]
    end
    
    subgraph "Shared Components"
        DesignLab[DesignLab.razor]
        EntityList[EntityList.razor]
    end
    
    Editor --> TutorPanel
    Editor --> NotebookPanel
    Home --> MainLayout
    Editor --> MainLayout
    Planner --> MainLayout
    MainLayout --> NavMenu
    
    style Editor fill:#ffeb3b
    style Planner fill:#ffeb3b
    style TutorPanel fill:#81c784
    style MainLayout fill:#64b5f6
```

## Data Model

```mermaid
erDiagram
    Account ||--o{ Story : owns
    Account ||--o{ Notebook : has
    Account ||--o| Theme : uses
    
    Story ||--o{ StoryEntityLink : references
    Story {
        int Id PK
        int AccountId FK
        string Title
        string Content
        string Genre
        string Metadata
        datetime Created
        datetime LastModified
    }
    
    Notebook ||--o{ NotebookEntity : contains
    Notebook {
        int Id PK
        int AccountId FK
        string Name
        string Icon
        bool IsSystemDefault
    }
    
    NotebookEntity ||--o{ NotebookEntry : has
    NotebookEntity ||--o{ StoryEntityLink : linkedTo
    NotebookEntity {
        int Id PK
        int NotebookId FK
        string Name
        string Summary
        string ImageUrl
    }
    
    Archetype ||--o{ ArchetypePoint : defines
    Archetype {
        string Id PK
        string Name
        string Description
        string SvgPath
    }
    
    ArchetypePoint ||--o{ ArchetypeExample : includes
    ArchetypePoint {
        int Id PK
        string ArchetypeId FK
        int StepId
        string Label
        string Prompt
        double X
        double Y
        string Align
    }
    
    ArchetypeExample {
        int Id PK
        int ArchetypePointId FK
        string Title
        string Content
    }
    
    Theme {
        int Id PK
        string Name
        string Description
        string PrimaryColor
        string SecondaryColor
        string BackgroundTexture
        string FontName
        string SpritePath
    }
    
    Account {
        int Id PK
        string Name
        string SupervisorName
        string SupervisorEmail
        string CohereApiKey
        bool UseReasoningModel
        string ThemePreferenceJson
    }
```

## Service Dependencies

```mermaid
graph TD
    subgraph "Scoped Services"
        SS[SessionState]
        SC[StoryContext]
        TO[TutorOrchestrator]
        CT[ICohereTutorService]
        SG[SafeguardService]
        TS[TutorSessionService]
        DB[AppDbContext]
    end
    
    subgraph "Singleton Services"
        AS[ArchetypeService]
        TT[TextTokenizer]
        PS[PromptService]
        PR[PromptRepository]
    end
    
    subgraph "Infrastructure"
        SF[IServiceScopeFactory]
        HTTP[HttpClient Named 'LLM']
    end
    
    SS --> DB
    SC --> SS
    TO --> SC
    TO --> CT
    TO --> SG
    AS --> SF
    SF --> DB
    CT --> HTTP
    HTTP --> CohereAPI[Cohere API]
    PS --> PR
    
    style SS fill:#fff9c4
    style SC fill:#fff9c4
    style TO fill:#fff9c4
    style CT fill:#fff9c4
    style AS fill:#c8e6c9
    style TT fill:#c8e6c9
    style PS fill:#c8e6c9
    style DB fill:#ffccbc
    style CohereAPI fill:#f8bbd0
```

## Request Flow: Story Editing

```mermaid
sequenceDiagram
    participant User
    participant Editor
    participant SessionState
    participant QuillJS
    participant AppDbContext
    participant SQLite
    
    User->>Editor: Open Story
    Editor->>SessionState: LoadStory(storyId)
    SessionState->>AppDbContext: Stories.FindAsync(storyId)
    AppDbContext->>SQLite: SELECT * FROM Stories
    SQLite-->>AppDbContext: Story Data
    AppDbContext-->>SessionState: Story Object
    SessionState-->>Editor: Story Loaded
    Editor->>QuillJS: LoadHTML(content)
    QuillJS-->>User: Display Content
    
    User->>QuillJS: Edit Text
    QuillJS->>Editor: Content Changed
    Editor->>SessionState: UpdateContent(html)
    SessionState->>AppDbContext: SaveChangesAsync()
    AppDbContext->>SQLite: UPDATE Stories
    SQLite-->>AppDbContext: Success
    AppDbContext-->>SessionState: Saved
    SessionState-->>Editor: Update Confirmed
```

## AI Tutor Flow

```mermaid
sequenceDiagram
    participant User
    participant TutorPanel
    participant TutorOrch as TutorOrchestrator
    participant Cohere as ICohereTutorService
    participant API as Cohere API
    participant Context as StoryContext
    
    User->>TutorPanel: Ask Question
    TutorPanel->>TutorOrch: ProcessUserMessage(text)
    TutorOrch->>TutorOrch: ValidateSafeguards()
    TutorOrch->>Context: GetStoryContext()
    Context-->>TutorOrch: Story Content (Sanitized)
    TutorOrch->>Cohere: SendPrompt(context)
    Cohere->>API: POST /chat (non-training)
    API-->>Cohere: AI Response
    Cohere-->>TutorOrch: Parsed Response
    TutorOrch->>TutorOrch: ApplySparkProtocol()
    TutorOrch-->>TutorPanel: Socratic Question
    TutorPanel-->>User: Display Response
    
    Note over TutorOrch: No Ghostwriting: AI never generates story content
```

## Archetype System Flow

```mermaid
sequenceDiagram
    participant User
    participant Planner
    participant ArchService as ArchetypeService
    participant SessionState
    participant AppDb as AppDbContext
    
    User->>Planner: Navigate to Planner
    Planner->>ArchService: GetArchetypes()
    ArchService->>AppDb: Archetypes.Include(Points).Include(Examples)
    AppDb-->>ArchService: Archetype Data
    ArchService-->>Planner: ArchetypeDefinitions
    
    User->>Planner: Select Archetype (e.g., "Hero's Journey")
    Planner->>SessionState: CurrentPlan.ArchetypeId = "hero"
    
    User->>Planner: Select Plot Point
    Planner->>Planner: Display Point Prompt
    Planner->>Planner: Show Literary Examples
    
    User->>Planner: Enter Notes
    Planner->>SessionState: Update PlotPoint Notes
    SessionState->>AppDb: SaveChangesAsync()
    AppDb-->>Planner: Saved
```

## Deployment Architecture

```mermaid
graph TB
    subgraph "Container Environment"
        subgraph "Docker Container"
            AspNet[ASP.NET Core App]
            Blazor[Blazor Server Runtime]
            SQLiteFile[SQLite DB File]
        end
    end
    
    subgraph "External Services"
        Cohere[Cohere AI API<br/>US-based<br/>Non-training Mode]
    end
    
    subgraph "Client Access"
        Browser1[Student Browser 1]
        Browser2[Student Browser 2]
        BrowserN[Student Browser N]
    end
    
    Browser1 -->|WebSocket| Blazor
    Browser2 -->|WebSocket| Blazor
    BrowserN -->|WebSocket| Blazor
    
    Blazor --> AspNet
    AspNet -->|EF Core| SQLiteFile
    AspNet -->|HTTPS| Cohere
    
    SQLiteFile -->|WAL Mode| SQLiteFile
    
    style SQLiteFile fill:#ffccbc
    style Cohere fill:#f8bbd0
    style Blazor fill:#b3e5fc
    
    note1[WAL Mode Enabled<br/>for Concurrency]
    note1 -.-> SQLiteFile
    
    note2[Anonymized Inference<br/>Data Sovereignty: Canada]
    note2 -.-> Cohere
```

## Key Design Patterns

### 1. **Scoped State Management**
- [`SessionState`](StoryFort/Services/SessionState.cs) acts as the session-level cache for the active story, notebooks, and tutor context
- [`StoryContext`](StoryFort/Services/StoryContext.cs) provides structured context to the AI orchestrator
- Prevents excessive database queries during editing sessions

### 2. **Singleton Archetype Provider**
- [`ArchetypeService`](StoryFort/Services/ArchetypeService.cs) provides read-only access to story structure templates
- Uses `IServiceScopeFactory` to access scoped `AppDbContext` when needed

### 3. **Spark Protocol (No Ghostwriting)**
- [`TutorOrchestrator`](StoryFort/Services/TutorOrchestrator.cs) ensures AI only asks questions, never writes content
- Defense-in-depth with `ValidateSafeguards()` before every LLM call

### 4. **WAL Mode for Concurrency**
- SQLite configured with Write-Ahead Logging in [`Program.cs`](StoryFort/Program.cs)
- Critical for supporting 30+ concurrent student writers

## Technology Stack

```mermaid
mindmap
  root((StoryFort))
    Frontend
      Blazor Server
      Blazored.TextEditor QuillJS
      Tailwind CSS
      Lucide Icons
    Backend
      .NET 10
      ASP.NET Core
      Entity Framework Core
    Database
      SQLite
      WAL Mode
    AI Services
      Cohere API
      Typed HttpClient
    Logging
      Serilog
      File Rotation
    Testing
      xUnit
      Playwright E2E
```

## Security & Privacy Architecture

```mermaid
graph TD
    subgraph "Defense Layers"
        A[User Input] --> B[ValidateSafeguards]
        B --> C{Passes Checks?}
        C -->|Yes| D[Anonymize Context]
        C -->|No| E[Block Request]
        D --> F[Send to Cohere]
        F --> G[Non-Training Mode]
        G --> H[Response Filtering]
        H --> I[Return to User]
    end
    
    subgraph "Data Protection"
        J[Story Content] --> K[Never Logged]
        J --> L[Never Sent to AI Full]
        J --> M[Encrypted at Rest]
        J --> N[Regional Hosting Canada]
    end
    
    subgraph "Access Control"
        O[Supervisor Gate] --> P[PIN/Challenge]
        P --> Q[API Key Config]
        P --> R[Account Settings]
    end
    
    style B fill:#ffeb3b
    style E fill:#ef5350
    style K fill:#66bb6a
    style M fill:#66bb6a
    style O fill:#42a5f5
```

## Notes

- **Interactive Server Mode**: All components use `@rendermode InteractiveServer` for real-time updates via SignalR
- **No Authentication (MVP)**: Future versions will integrate MS Azure SSO
- **SQLite Limitations**: Current design supports ~500 concurrent users; migration to PostgreSQL planned for scale
- **Content IP**: Child users retain full intellectual property rights to their stories
- **Pedagogical Alignment**: System maps to Manitoba ELA curriculum (GLO 4: Generate, Appraise, Edit)

---

**Last Updated**: January 2026  
**Version**: 0.4 (Draft)