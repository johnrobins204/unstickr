# Unstickd: Workflow Diagrams

This document contains Mermaid diagrams illustrating the key user flows and system interactions.

---

## 1. Onboarding Flow

```mermaid
flowchart TD
    Start([App Launch]) --> AgeGate[Enter Date of Birth]
    AgeGate --> CalcAge{Calculate Age}
    CalcAge --> FilterThemes[Filter Age-Appropriate Themes]
    FilterThemes --> ThemeGrid[Display Theme Selection Grid]
    ThemeGrid --> UserSelect[User Selects Theme]
    UserSelect --> Transition[Theme Door Transition Animation]
    Transition --> InitStory[Initialize New Story with Theme]
    InitStory --> Editor([Navigate to Editor - Page 1])
```

---

## 2. Story Editing Session

```mermaid
flowchart TD
    Start([Open Story]) --> LoadPage[Load Current Page from DB]
    LoadPage --> Editor[Rich Text Editor Active]
    Editor --> UserTypes{User Typing?}
    
    UserTypes -->|Yes| ResetTimer[Reset Inactivity Timer]
    ResetTimer --> Editor
    
    UserTypes -->|No - Idle > 10s| TriggerUnstickr[Activate Unstickr]
    TriggerUnstickr --> Animate[Sprite Glows/Bounces]
    Animate --> GetContext[Send Page Content to LLM Service]
    GetContext --> LLMCheck{Tutor Provider?}
    
    LLMCheck -->|Mock| MockResponse[Return Pre-Written Question]
    LLMCheck -->|Ollama| CallLocal[POST to http://localhost:11434]
    LLMCheck -->|Gemini| CallCloud[Call Google API]
    
    MockResponse --> DisplayBubble[Show Speech Bubble with Question]
    CallLocal --> DisplayBubble
    CallCloud --> DisplayBubble
    
    DisplayBubble --> Editor
    
    Editor --> NavAction{User Action}
    NavAction -->|Next Page| SaveCurrent[Auto-Save Current Page]
    NavAction -->|Previous Page| SaveCurrent
    NavAction -->|Home| SaveCurrent
    
    SaveCurrent --> LoadNewPage{Page Exists?}
    LoadNewPage -->|Yes| LoadPage
    LoadNewPage -->|No - Next| CreatePage[Create New Blank Page]
    CreatePage --> LoadPage
```

---

## 3. Theme Switching Flow

```mermaid
sequenceDiagram
    participant User
    participant UI as Blazor UI
    participant JS as Client JS
    participant Server as Blazor Server
    participant DB as SQLite

    User->>UI: Click "Spooky Theme" Button
    UI->>JS: Trigger Theme Transition
    JS->>JS: Start Wipe Animation (1.5s)
    
    par Parallel Actions
        JS->>JS: Update CSS Variables (Colors)
        JS->>JS: Swap Unstickr Sprite (src)
        JS->>JS: Apply Font Family
    and
        UI->>Server: UpdateTheme(StoryId, ThemeId)
        Server->>DB: UPDATE Stories SET ThemeId = ...
        DB-->>Server: Success
    end
    
    JS-->>UI: Animation Complete
    UI-->>User: Theme Fully Applied
```

---

## 4. Notebook Entity Management

```mermaid
flowchart TD
    Start([Navigate to /characters]) --> ListPage[Display Character List]
    ListPage --> UserAction{User Action}
    
    UserAction -->|Create New| Form[Show Character Form]
    Form --> FillFields[User Enters Name, Description]
    FillFields --> Submit[Click Save]
    Submit --> SaveDB[(Save to NotebookEntries Table)]
    SaveDB --> ListPage
    
    UserAction -->|View Details| ShowChar[Display Character Card]
    ShowChar --> QueryLinks[Query StoryReferences Join Table]
    QueryLinks --> DisplayStories[Show: "Appears in Story A, Story B"]
    DisplayStories --> OptionLink{User Action}
    
    OptionLink -->|Open Story| NavStory([Navigate to Story Editor])
    OptionLink -->|Back| ListPage
    
    UserAction -->|Link to Story| LinkForm[Show Story Selection]
    LinkForm --> SelectStory[User Picks Story]
    SelectStory --> CreateLink[(Insert into StoryReferences)]
    CreateLink --> ListPage
```

---

## 5. LLM Service Architecture

```mermaid
flowchart LR
    Editor[Story Editor Component] --> Interface[ITutorService Interface]
    
    Interface --> Mock[MockTutorService]
    Interface --> Ollama[OllamaTutorService]
    Interface --> Gemini[GeminiTutorService]
    
    Mock --> StaticQs[(Hardcoded Questions Array)]
    StaticQs --> Response1[Return Random Question]
    
    Ollama --> HttpClient1[Named HttpClient: LLM]
    HttpClient1 --> LocalAPI[http://localhost:11434/api/generate]
    LocalAPI --> Prompt1[System: You are a Socratic tutor...]
    Prompt1 --> Response2[Return Question Only]
    
    Gemini --> HttpClient2[HttpClient w/ API Key]
    HttpClient2 --> CloudAPI[Google Gemini API]
    CloudAPI --> Prompt2[System: No prose, questions only]
    Prompt2 --> Response3[Return Question Only]
    
    Response1 --> Editor
    Response2 --> Editor
    Response3 --> Editor
```

---

## 6. Data Relationships (Entity Diagram)

```mermaid
erDiagram
    STORIES ||--o{ STORY_PAGES : contains
    STORIES ||--o{ STORY_REFERENCES : links
    NOTEBOOK_ENTRIES ||--o{ STORY_REFERENCES : appears_in
    THEMES ||--o{ STORIES : defines_look
    
    STORIES {
        int Id PK
        string Title
        int ThemeId FK
        datetime CreatedDate
        datetime LastModifiedDate
    }
    
    STORY_PAGES {
        int Id PK
        int StoryId FK
        int PageNumber
        string Content
    }
    
    NOTEBOOK_ENTRIES {
        int Id PK
        string Name
        string Description
        string Type
        string Metadata
    }
    
    STORY_REFERENCES {
        int StoryId FK
        int NotebookEntryId FK
    }
    
    THEMES {
        int Id PK
        string Name
        string ColorPrimary
        string ColorSecondary
        string FontFamily
        string SpritePath
    }
```

---

## 7. System Startup & Configuration

```mermaid
flowchart TD
    Start([App Launch]) --> LoadConfig[Read appsettings.json]
    LoadConfig --> CheckProvider{TutorProvider Value?}
    
    CheckProvider -->|Mock| RegisterMock[builder.Services.AddScoped<ITutorService, MockTutorService>]
    CheckProvider -->|Ollama| RegisterOllama[builder.Services.AddScoped<ITutorService, OllamaTutorService>]
    CheckProvider -->|Gemini| RegisterGemini[builder.Services.AddScoped<ITutorService, GeminiTutorService>]
    
    RegisterMock --> InitDB[Initialize EF Core + SQLite]
    RegisterOllama --> InitDB
    RegisterGemini --> InitDB
    
    InitDB --> Migrate{Pending Migrations?}
    Migrate -->|Yes| ApplyMigrations[Database.Migrate]
    Migrate -->|No| SeedData{Themes Table Empty?}
    
    ApplyMigrations --> SeedData
    SeedData -->|Yes| InsertThemes[Insert 5 Default Themes]
    SeedData -->|No| Ready
    
    InsertThemes --> Ready([App Ready])
```

---

## Notes
- All diagrams use **Mermaid** syntax compatible with GitHub/VS Code renderers
- Diagrams can be viewed inline in VS Code with Mermaid preview extensions
- For detailed implementation specs, see `requirements.md`
