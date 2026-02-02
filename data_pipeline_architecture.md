# Public Domain Text Ingestion Pipeline - Architecture

**Project**: StoryFort  
**Date**: February 2, 2026  
**Status**: Revised Post-Audit - Compliance & Technical Enhancements  
**Audit Date**: February 2, 2026

---

## ⚠️ CRITICAL: Audit Compliance Updates

This architecture has been updated to address legal compliance and technical optimization issues identified in an independent audit. Key changes:

1. **Legal Compliance**: Project Gutenberg access via rsync mirrors (not direct HTTP)
2. **Token Safety**: Chunking limited to 380 tokens (was 400 words ≈ 520 tokens)
3. **Model Upgrade**: Cohere v4 multimodal embeddings (128K context)
4. **Advanced RAG**: Parent-Child retrieval pattern for context preservation
5. **Quality Control**: OCR correction layer for Archive.org sources
6. **Canadian Law**: AIDA compliance strategy for high-impact AI systems

**See**: [data_pipeline_architecture_audit.md](data_pipeline_architecture_audit.md) for full audit report.  

---

## Executive Summary

This document describes the architecture for ingesting public domain literary texts into StoryFort's dual-purpose system:
1. **Reading Interface**: Children can read, select, and "pin" passages to notebooks
2. **RAG Retrieval**: AI tutor can answer semantic queries about literature

**Core Decision**: Use **EPUB as primary source format** where available, with fallback strategies for HTML-only sources.

**Technology Stack**: Full C# implementation to maintain unified codebase with existing .NET 10 Blazor application.

---

## Legal Compliance & Risk Mitigation ⚠️

### Project Gutenberg Compliance

**Critical Requirements**:
1. **No Direct Site Scraping**: Use rsync mirrors only
2. **Download Limits**: Max 100 downloads/day from any single source
3. **Politeness Delay**: 2 seconds between automated requests
4. **Trademark**: Remove "Project Gutenberg" branding if commercial (20% royalty otherwise)

**Implementation**:
```bash
# Compliant download via rsync
rsync -av --include='*/' --include='*.epub' --exclude='*' \
  --delay-updates --timeout=300 \
  aleph.gutenberg.org::gutenberg/pg{BOOK_ID}/ \
  epub_cache/
```

**Trademark Stripping**: `ITrademarkStripperService` removes:
- "Project Gutenberg" from metadata
- License headers from text
- Branding from EPUB metadata

### Sacred Texts Compliance

**Critical Requirements**:
1. **Copyrighted Metadata**: Exclude introductory essays and commentary (Evinity Publishing INC)
2. **Bandwidth Respect**: 5-second delay between page requests
3. **Short File Structure**: Scrape multi-page texts sequentially

**Implementation**:
- `IHtmlCleaner` identifies and removes copyrighted sections
- Rate limiter enforces 5-second politeness delay
- Logs all excluded content for audit trail

### Canadian Regulatory Compliance (AIDA)

**High-Impact AI System Classification**:
StoryFort's public domain RAG system qualifies as "high-impact" under AIDA due to:
- Religious/sacred text interpretation
- Potential for discriminatory outcomes (cultural bias)
- Use with children (vulnerable population)

**Mitigation Strategy**:
1. **Human-in-the-Loop**: Adult review of sacred text responses
2. **Audit Logging**: All RAG queries logged for bias detection
3. **Risk Assessment**: Quarterly review of hallucination rates
4. **Transparency**: Disclose AI limitations to users

### Data Sovereignty

- All data stored in Canadian region (SQLite)
- Embeddings stored separately from source text (re-embedding capability)
- Cohere API (US-based) receives only derived chunks, not full texts

---

## Architecture Principles

### 1. Dual-Storage Strategy ✅ AUDIT ENHANCEMENT: Parent-Child RAG

**Problem**: Reading interface needs structured chapters/paragraphs, while RAG needs optimized semantic chunks.

**Solution**: Parent-Child retrieval pattern with separate storage layers:

```
Source Layer (EPUB/HTML)          RAG Layer (Parent-Child)
┌─────────────────────┐          ┌─────────────────────────┐
│ PublicText          │          │ TextChunk (CHILD)       │
│ - Original EPUB     │          │ - 380 tokens (small)    │
│ - Metadata          │◄─────────│ - Precise vector search │
│ - Chapter structure │          │ - Links to PARENT       │
└─────────────────────┘          └──────────┬──────────────┘
         │                                  │
         │                       ┌──────────▼──────────────┐
         │                       │ TextParent              │
         │                       │ - Full paragraph/section│
         │                       │ - Provides LLM context  │
         │                       └─────────────────────────┘
         ▼                                  │
    Reader UI                               ▼
                                   RAG: Search child → Retrieve parent
```

**How Parent-Child Works**:
1. **Child chunks**: Small (380 tokens) for precise vector matching
2. **Parent chunks**: Large (full paragraph/section) for LLM context
3. **Search**: Vector search finds child chunk
4. **Retrieve**: Return parent chunk to provide full context to LLM
5. **Benefit**: Solves "context loss" problem in literary fiction

### 2. Multi-Source Ingestion

**Sources in Seed Data**:
- **Project Gutenberg** (12 texts): EPUB available → Primary pipeline
- **Sacred Texts** (5 texts): HTML-only → Convert to EPUB via Pandoc
- **Archive.org** (1 text): Mixed formats → EPUB preferred, fallback to OCR

**Pipeline Strategy**:
```
┌─────────────────┐
│ Source Detection│
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌──────┐  ┌────────┐
│ EPUB │  │  HTML  │
│ Path │  │  Path  │
└───┬──┘  └───┬────┘
    │         │
    │    ┌────▼─────┐
    │    │ Pandoc   │
    │    │ Convert  │
    │    └────┬─────┘
    │         │
    └────┬────┘
         ▼
    ┌─────────┐
    │  Parse  │
    │  Chunk  │
    │  Embed  │
    │  Store  │
    └─────────┘
```

---

## Data Models

### Core Entities

```csharp
// Source layer - preserves original structure
public class PublicText
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string AuthorTranslator { get; set; }
    public string Region { get; set; }  // Western, East Asian, African, etc.
    public string PlaceOfOrigin { get; set; }  // USA, Japan, Ghana, etc.
    
    // Source attribution
    public string SourceName { get; set; }  // "Project Gutenberg"
    public string SourceUrl { get; set; }
    public string License { get; set; }  // "Public Domain"
    public DateTime IngestionDate { get; set; }
    
    // Format metadata
    public string OriginalFormat { get; set; }  // "EPUB", "HTML", "PDF"
    public string EpubVersion { get; set; }  // "3.0", null
    
    // Reading interface data (optional EPUB blob)
    public byte[]? EpubBlob { get; set; }  // Store original EPUB for rendering
    
    // Enriched metadata
    public string Genre { get; set; }  // "Fantasy", "Folk Tale", "Epic"
    public string TargetAgeRange { get; set; }  // "8-12"
    public int EstimatedWordCount { get; set; }
    
    // AUDIT: Quality control metadata
    public bool IsOcrSource { get; set; }  // Requires correction layer
    public bool HasStructuralMetadata { get; set; }  // Has verse/chapter hierarchy
    public double OcrConfidenceScore { get; set; }  // 0.0-1.0 if OCR corrected
    public bool TrademarkStripped { get; set; }  // For commercial compliance
    
    // Relationships
    public ICollection<TextChapter> Chapters { get; set; }
}

// Structural layer - preserves literary organization
public class TextChapter
{
    public int Id { get; set; }
    public int PublicTextId { get; set; }
    public PublicText PublicText { get; set; }
    
    public int ChapterNumber { get; set; }
    public string ChapterTitle { get; set; }  // "Chapter 1: Down the Rabbit Hole"
    public string ChapterAnchor { get; set; }  // "ch01" for deep linking
    
    // Original EPUB structure IDs for precise references
    public string EpubItemId { get; set; }  // From EPUB spine
    
    public int WordCount { get; set; }
    public int StartParagraphIndex { get; set; }
    public int EndParagraphIndex { get; set; }
    
    // Relationships
    public ICollection<TextChunk> Chunks { get; set; }
}

// RAG layer - optimized for semantic retrieval
// AUDIT: Implements Parent-Child pattern
public class TextChunk  // CHILD chunk - for vector search
{
    public int Id { get; set; }
    public int PublicTextId { get; set; }
    public PublicText PublicText { get; set; }
    
    public int? TextChapterId { get; set; }  // Nullable for preambles
    public TextChapter? TextChapter { get; set; }
    
    // AUDIT: Parent-Child linkage
    public int? ParentChunkId { get; set; }  // Links to larger context chunk
    public TextParent? ParentChunk { get; set; }
    
    // Content - AUDIT: Smaller chunks for precise retrieval
    public string PlainText { get; set; }  // 380 tokens max (not 400 words)
    public int WordCount { get; set; }
    public int TokenCount { get; set; }  // For LLM context calculations
    
    // Position tracking for "Pin It" feature
    public int StartParagraphIndex { get; set; }
    public int EndParagraphIndex { get; set; }
    public int ChunkIndexInChapter { get; set; }  // 0-based
    
    // Deep linking back to reading interface
    public string SourceAnchor { get; set; }  // "alice#ch01-p05"
    
    // Embedding storage
    public int? ChunkEmbeddingId { get; set; }
    public ChunkEmbedding? ChunkEmbedding { get; set; }
}

// AUDIT: Parent chunk - provides full context to LLM
public class TextParent  // PARENT chunk - for context injection
{
    public int Id { get; set; }
    public int PublicTextId { get; set; }
    public PublicText PublicText { get; set; }
    
    public int? TextChapterId { get; set; }
    public TextChapter? TextChapter { get; set; }
    
    // Content - Full paragraph or section
    public string PlainText { get; set; }  // Up to 2000 tokens
    public int WordCount { get; set; }
    public int TokenCount { get; set; }
    
    // Structure - For sacred texts (Book > Chapter > Verse)
    public string? StructuralPath { get; set; }  // "Genesis/1/1"
    public int? VerseNumber { get; set; }
    
    // Relationships
    public ICollection<TextChunk> ChildChunks { get; set; }  // Multiple children per parent
}

// Vector storage for RAG
public class ChunkEmbedding
{
    public int Id { get; set; }
    public int TextChunkId { get; set; }
    public TextChunk TextChunk { get; set; }
    
    // Cohere embed-english-v3.0 produces 1024-dimensional vectors
    public string EmbeddingModel { get; set; }  // "embed-english-v3.0"
    public DateTime GeneratedAt { get; set; }
    
    // SQLite BLOB storage for float[] array
    // Serialized as JSON or binary for MVP
    public byte[] VectorData { get; set; }  // float[1024] serialized
    
    // For future sqlite-vec extension integration
    public int Dimensions { get; set; }  // 1024
}

// Pin integration - existing NotebookEntry extended
public class NotebookEntry  // Existing entity, extended with new fields
{
    // ... existing fields ...
    
    // NEW: Link to public domain text source
    public int? PublicTextId { get; set; }
    public PublicText? PublicText { get; set; }
    
    public int? TextChunkId { get; set; }
    public TextChunk? TextChunk { get; set; }
    
    public string? SourceAttribution { get; set; }  // "Alice in Wonderland, Ch. 1"
}
```

---

## Component Architecture

### 1. Console Application: `StoryFort.Ingest`

**Purpose**: CLI tool for ingesting public domain texts.

**Structure**:
```
StoryFort.Ingest/
├── Program.cs                  # CLI entry point
├── Models/
│   └── PublicTextSeedDto.cs    # Deserializes seed JSON
├── Pipelines/
│   ├── IGutenbergPipeline.cs
│   ├── ISacredTextsPipeline.cs
│   └── IArchivePipeline.cs
├── Services/
│   ├── IGutenbergMirrorService.cs   # AUDIT: rsync mirrors, not HTTP
│   ├── IEpubParser.cs
│   ├── ISemanticChunker.cs          # AUDIT: Token-based, not word-based
│   ├── ICohereEmbedService.cs       # AUDIT: v4, not v3
│   ├── IHtmlToEpubConverter.cs
│   ├── IOcrCorrectionService.cs     # AUDIT: New - LLM-based OCR cleanup
│   ├── ITrademarkStripperService.cs # AUDIT: New - Remove "Project Gutenberg"
│   ├── IMetadataEnricher.cs         # AUDIT: New - Extract structural metadata
│   └── IPublicTextRepository.cs
└── appsettings.json            # Cohere API key, paths
```

**Usage**:
```bash
# Ingest from seed file
dotnet run --project StoryFort.Ingest -- \
  --seed public_text_seed.json \
  --download \
  --chunk \
  --embed

# Dry-run (validate downloads only)
dotnet run --project StoryFort.Ingest -- \
  --seed public_text_seed.json \
  --download \
  --dry-run

# Re-embed existing chunks (if switching models)
dotnet run --project StoryFort.Ingest -- \
  --re-embed \
  --model embed-english-v3.0
```

### 2. Core Services

#### **A. IGutenbergMirrorService** ✅ AUDIT COMPLIANT
```csharp
public interface IGutenbergMirrorService
{
    /// <summary>
    /// Downloads EPUB from Project Gutenberg MIRRORS via rsync
    /// Complies with ToS: No direct site scraping
    /// </summary>
    Task<string> SyncFromMirrorAsync(int gutenbergId, string outputPath);
    
    /// <summary>
    /// Extracts Gutenberg book ID from URL
    /// </summary>
    int? ExtractGutenbergId(string url);
}
```

**Implementation Notes** (COMPLIANCE CRITICAL):
- **DO NOT** scrape main Gutenberg site (violates ToS)
- Use rsync mirrors: `rsync -av --include='*/' --include='*.epub' --exclude='*' \
  aleph.gutenberg.org::gutenberg/ epub_cache/`
- Politeness delay: 2 seconds between requests
- Max 100 downloads/day from any single mirror
- Cache downloads in `Data/epub_cache/` to avoid re-downloading
- **Trademark Stripping**: Remove "Project Gutenberg" references if commercial

#### **B. IEpubParser**
```csharp
public interface IEpubParser
{
    /// <summary>
    /// Parses EPUB and extracts structured content
    /// </summary>
    Task<ParsedEpubBook> ParseAsync(string epubPath);
}

public class ParsedEpubBook
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string Language { get; set; }
    public string License { get; set; }
    public List<ParsedChapter> Chapters { get; set; }
}

public class ParsedChapter
{
    public string Title { get; set; }
    public string Anchor { get; set; }
    public string HtmlContent { get; set; }
    public List<string> Paragraphs { get; set; }  // Extracted <p> elements
}
```

**Library**: VersOne.Epub
```csharp
var book = await EpubReader.ReadBookAsync(epubPath);

// Extract metadata
var title = book.Title;
var author = book.Author;

// Extract chapters
foreach (var chapter in book.ReadingOrder)
{
    var htmlContent = await chapter.ReadContentAsTextAsync();
    // Parse with AngleSharp to extract paragraphs
}
```

#### **C. ISemanticChunker** ✅ AUDIT COMPLIANT - TOKEN BASED
```csharp
public interface ISemanticChunker
{
    /// <summary>
    /// Chunks paragraphs into RAG-optimized segments
    /// CRITICAL: Uses TOKEN COUNT not word count to prevent truncation
    /// </summary>
    List<TextChunkDto> ChunkParagraphs(
        List<string> paragraphs, 
        int maxTokens = 380,  // Safety margin below 512 limit
        int overlapTokens = 50
    );
}

public class TextChunkDto
{
    public string PlainText { get; set; }
    public int WordCount { get; set; }
    public int TokenCount { get; set; }  // AUDIT: Must track tokens
    public int StartParagraphIndex { get; set; }
    public int EndParagraphIndex { get; set; }
}
```

**Algorithm** (TOKEN-SAFE):
1. **Tokenize** each paragraph using Cohere tokenizer
2. Accumulate until `maxTokens` reached (380, not 512)
3. **Don't split mid-paragraph** (preserve readability)
4. Add `overlapTokens` from previous chunk (improves RAG context)
5. Generate `SourceAnchor` for deep linking

**Why 380 tokens?**
- Cohere v3: 512 token max
- Safety margin: 512 - 380 = 132 tokens buffer
- Prevents silent truncation that loses narrative endings

**Example**:
```
Input: [Para1: 120 tokens, Para2: 200 tokens, Para3: 180 tokens, Para4: 150 tokens]
Output:
  Chunk1: Para1 + Para2 (320 tokens) → #ch01-p01
  Chunk2: Para2(last 50) + Para3 + Para4 (380 tokens) → #ch01-p03
```

#### **D. ICohereEmbedService** ✅ AUDIT UPGRADE: v4 MULTIMODAL
```csharp
public interface ICohereEmbedService
{
    /// <summary>
    /// Generates embeddings for batch of texts
    /// UPGRADE: Uses Cohere v4 (128K context, Matryoshka embeddings)
    /// Cohere allows up to 96 texts per request
    /// </summary>
    Task<float[][]> EmbedBatchAsync(
        string[] texts, 
        string model = "embed-v4.0",  // AUDIT: Upgraded from v3
        int dimensions = 1024  // Matryoshka: Can truncate to 256/512 later
    );
}
```

**Cohere v4 Advantages**:
- **128K token context** (vs 512 in v3) → supports full chapters
- **Multimodal**: Can embed images from illustrated EPUBs
- **Matryoshka**: Truncate vectors to 256/512 dims for storage optimization
- **Better retrieval**: Trained on 2024-2025 data

**Implementation** (extends existing `CohereTutorService` pattern):
```csharp
public class CohereEmbedService : ICohereEmbedService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    
    public async Task<float[][]> EmbedBatchAsync(string[] texts, string model)
    {
        var client = _httpClientFactory.CreateClient("LLM");
        var apiKey = _configuration["Cohere:ApiKey"];
        
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", apiKey);
        
        var payload = new { 
            texts, 
            model,  // "embed-v4.0" per audit recommendation
            input_type = "search_document",  // For RAG storage
            embedding_types = new[] { "float" },  // Or "int8" for storage optimization
            truncate = "NONE"  // Fail if text exceeds context (safety check)
        };
        
        var response = await client.PostAsJsonAsync(
            "https://api.cohere.com/v1/embed", payload);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Cohere API error: {error}");
        }
        
        var result = await response.Content
            .ReadFromJsonAsync<CohereEmbedResponse>();
        
        return result.Embeddings;
    }
}

public class CohereEmbedResponse
{
    public float[][] Embeddings { get; set; }
    public string Model { get; set; }
}
```

**Rate Limiting** (AUDIT: Updated for v4):
- Cohere Embed v4: **100 requests/minute** (Trial tier)
- Batch 96 texts per request → ~9,600 chunks/minute
- For 18 books × ~100 chunks = ~1,800 chunks → **2 minutes** total
- AUDIT: Add 2-second politeness delay for Sacred Texts HTTP requests

#### **E. IHtmlToEpubConverter**
```csharp
public interface IHtmlToEpubConverter
{
    /// <summary>
    /// Converts HTML (Sacred Texts) to EPUB via Pandoc
    /// </summary>
    Task<string> ConvertAsync(string htmlPath, string outputEpubPath, 
        string title, string author);
}
```

**Implementation**:
```csharp
public class HtmlToEpubConverter : IHtmlToEpubConverter
{
    public async Task<string> ConvertAsync(
        string htmlPath, string outputEpubPath, 
        string title, string author)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pandoc",
                Arguments = $"-f html -t epub " +
                           $"--metadata title=\"{title}\" " +
                           $"--metadata author=\"{author}\" " +
                           $"-o \"{outputEpubPath}\" \"{htmlPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
        
        process.Start();
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Pandoc conversion failed: {error}");
        }
        
        return outputEpubPath;
    }
}
```

**Prerequisite**: Install Pandoc
```powershell
# Windows (via Chocolatey)
choco install pandoc

# Or download from https://pandoc.org/installing.html
```

#### **F. IOcrCorrectionService** ✅ AUDIT NEW - Quality Control
```csharp
public interface IOcrCorrectionService
{
    /// <summary>
    /// Corrects OCR errors using LLM (Command R7B)
    /// For Archive.org texts with variable quality scans
    /// </summary>
    Task<string> CorrectOcrErrorsAsync(string rawOcrText);
    
    /// <summary>
    /// Estimates OCR quality score (0.0-1.0)
    /// </summary>
    double EstimateOcrQuality(string text);
}
```

**Implementation** (uses Cohere Command R7B):
```csharp
public class OcrCorrectionService : IOcrCorrectionService
{
    public async Task<string> CorrectOcrErrorsAsync(string rawOcrText)
    {
        var prompt = $@"Fix OCR errors in this text. 
Common errors: l/1/!, 0/O, rn/m substitutions.
Preserve archaic spellings (shew, connexion).

Text:
{rawOcrText}

Corrected:";
        
        // Use Command R7B (fast, cheap for correction)
        var response = await _cohereClient.GenerateAsync(prompt, 
            model: "command-r-08-2024");
        
        return response.Text;
    }
}
```

#### **G. ITrademarkStripperService** ✅ AUDIT NEW - Compliance
```csharp
public interface ITrademarkStripperService
{
    /// <summary>
    /// Removes "Project Gutenberg" trademark for commercial use
    /// Required if StoryFort operates commercially (avoids 20% royalty)
    /// </summary>
    string StripGutenbergTrademarks(string text);
    
    /// <summary>
    /// Cleans EPUB metadata of trademarked references
    /// </summary>
    EpubMetadata CleanMetadata(EpubMetadata metadata);
}
```

**Patterns to Remove**:
- "Project Gutenberg" → "Public Domain Archive"
- Gutenberg license headers
- Trademark notices in metadata

#### **H. IMetadataEnricher** ✅ AUDIT NEW - Structural Hierarchy
```csharp
public interface IMetadataEnricher
{
    /// <summary>
    /// Extracts structural metadata (verse numbers, chapters)
    /// Critical for sacred texts (Bible, Quran, Bhagavad Gita)
    /// </summary>
    Task<StructuralMetadata> ExtractStructureAsync(ParsedEpubBook book);
}

public class StructuralMetadata
{
    public string HierarchyType { get; set; }  // "Book/Chapter/Verse", "Chapter/Section"
    public Dictionary<int, string> ChapterTitles { get; set; }
    public Dictionary<int, int> VerseNumbers { get; set; }  // Paragraph index → verse number
    public List<string> StructuralPaths { get; set; }  // "Genesis/1/1", "Genesis/1/2"
}
```

#### **I. IPublicTextRepository**
```csharp
public interface IPublicTextRepository
{
    Task<PublicText> CreatePublicTextAsync(PublicText text);
    Task AddChaptersAsync(int publicTextId, List<TextChapter> chapters);
    Task AddChunksAsync(List<TextChunk> chunks);
    Task AddEmbeddingsAsync(List<ChunkEmbedding> embeddings);
    Task<bool> ExistsAsync(string title);
}
```

**Implementation** (uses EF Core):
```csharp
public class PublicTextRepository : IPublicTextRepository
{
    private readonly AppDbContext _db;
    
    public async Task<PublicText> CreatePublicTextAsync(PublicText text)
    {
        _db.PublicTexts.Add(text);
        await _db.SaveChangesAsync();
        return text;
    }
    
    public async Task<bool> ExistsAsync(string title)
    {
        return await _db.PublicTexts.AnyAsync(t => t.Title == title);
    }
    
    // ... other methods
}
```

---

## Processing Flow

### End-to-End Pipeline

```
┌─────────────────────────────────────────────────────────────┐
│ 1. DOWNLOAD PHASE                                           │
├─────────────────────────────────────────────────────────────┤
│ Read public_text_seed.json                                  │
│ For each entry:                                             │
│   - Extract source type (Gutenberg/Sacred/Archive)          │
│   - Download EPUB (if available) OR HTML                    │
│   - Cache in Data/epub_cache/{title}.epub                   │
└─────────────────────────────────────────────────────────────┘
                            ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. CONVERSION PHASE (if needed)                             │
├─────────────────────────────────────────────────────────────┤
│ For HTML sources:                                           │
│   - Clean HTML (remove nav, ads)                            │
│   - Convert to EPUB via Pandoc                              │
│   - Validate EPUB structure                                 │
└─────────────────────────────────────────────────────────────┘
                            ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. PARSING PHASE                                            │
├─────────────────────────────────────────────────────────────┤
│ For each EPUB:                                              │
│   - Extract metadata (title, author, license)               │
│   - Parse reading order (chapters)                          │
│   - Extract HTML content                                    │
│   - Parse paragraphs with AngleSharp                        │
│   - Generate chapter anchors                                │
└─────────────────────────────────────────────────────────────┘
                            ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. CHUNKING PHASE                                           │
├─────────────────────────────────────────────────────────────┤
│ For each chapter:                                           │
│   - Apply semantic chunking (400-word target)               │
│   - Preserve paragraph boundaries                           │
│   - Add 50-word overlap between chunks                      │
│   - Generate source anchors (ch01-p05)                      │
│   - Calculate token counts                                  │
└─────────────────────────────────────────────────────────────┘
                            ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. EMBEDDING PHASE                                          │
├─────────────────────────────────────────────────────────────┤
│ Batch chunks (96 per request):                              │
│   - Call Cohere Embed API                                   │
│   - Store float[1024] vectors                               │
│   - Handle rate limits (retry with backoff)                 │
│   - Log progress (chunk 1500/1800)                          │
└─────────────────────────────────────────────────────────────┘
                            ▼
┌─────────────────────────────────────────────────────────────┐
│ 6. STORAGE PHASE                                            │
├─────────────────────────────────────────────────────────────┤
│ Transaction per book:                                       │
│   - Insert PublicText                                       │
│   - Insert TextChapters                                     │
│   - Insert TextChunks                                       │
│   - Insert ChunkEmbeddings                                  │
│   - Store original EPUB blob (optional)                     │
│   - Commit or rollback                                      │
└─────────────────────────────────────────────────────────────┘
                            ▼
┌─────────────────────────────────────────────────────────────┐
│ 7. INDEXING PHASE (Future)                                  │
├─────────────────────────────────────────────────────────────┤
│ - Build sqlite-vec index (when available)                   │
│ - Pre-compute similarity matrices (optional)                │
│ - Generate reading level scores                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Implementation Phases ✅ AUDIT REVISED

### **Phase 0: Compliance Infrastructure** (2 days) ⚠️ NEW

**Goal**: Establish legal compliance foundation before ingestion.

**Tasks**:
1. Implement `IGutenbergMirrorService` with rsync
2. Implement `ITrademarkStripperService`
3. Implement `ISacredTextsPolietnessService` (5-sec delay)
4. Add AIDA compliance logging framework
5. Test rsync mirror connectivity

**Success Criteria**:
- Can download from Gutenberg mirror (not main site)
- Trademark stripping verified on sample EPUB
- Rate limiting enforced for Sacred Texts

---

### **Phase 1: Token-Safe Chunking PoC** (3 days) ✅ AUDIT PRIORITY

**Goal**: Validate token-based chunking with Alice in Wonderland.

**Tasks**:
1. ❌ ~~Create `StoryFort.Ingest` console project~~ (DONE)
2. ❌ ~~Add EF Core models to `StoryFort` project~~ (DONE)
3. ✅ **CRITICAL**: Implement token-based `SemanticChunker` (380 token max)
4. ✅ **CRITICAL**: Add Cohere tokenizer for accurate counting
5. ✅ Implement `EpubParser` (VersOne.Epub)
6. ✅ Implement `CohereEmbedService` with v4 API
7. ✅ Test: Alice → Chunks → Verify NO chunks exceed 380 tokens
8. Create migration for Parent-Child tables

**Success Criteria**:
- Alice in Wonderland fully ingested
- ~120 chunks (smaller due to token limits)
- **Zero chunks exceed 380 tokens** (audit compliance)
- Can query: "rabbit hole" → returns Chapter 1 child chunks → retrieves parent context

---

### **Phase 2: Quality Control & Multi-Source** (4-5 days) ✅ AUDIT ENHANCED

**Goal**: Handle all 18 texts with quality controls.

**Tasks**:
1. Implement `IOcrCorrectionService` (Command R7B for Archive.org texts)
2. Implement `IMetadataEnricher` (extract verse numbers, structural hierarchy)
3. Implement `SacredTextsDownloader` + `HtmlCleaner` (copyright exclusion)
4. Implement `HtmlToEpubConverter` (Pandoc wrapper)
5. Implement `ArchiveOrgDownloader` (metadata API)
6. Add Parent-Child chunk generation
7. Batch process all 18 texts
8. Validate attribution metadata

**Success Criteria**:
- All 18 texts ingested
- ~2,000 child chunks + ~500 parent chunks
- OCR errors corrected in Archive.org sources
- Structural metadata extracted for sacred texts (verse numbers)
- Mixed sources display correctly in reader

---

### **Phase 3: Reading Interface Integration** (4-5 days)

**Goal**: Display public domain texts in StoryFort reader.

**Tasks**:
1. Create `PublicTextReader.razor` component
2. Implement chapter navigation
3. Add "Pin It" button with source tracking
4. Link `NotebookEntry` to `TextChunk`
5. Display attribution on pins

**Success Criteria**:
- Child can browse Alice in Wonderland
- Selecting text → "Pin to Notebook" → saves with attribution
- Clicking pin → navigates to original passage

---

### **Phase 4: RAG Retrieval Service** (3-4 days)

**Goal**: Enable tutor to query public domain corpus.

**Tasks**:
1. Implement `PublicTextRetrievalService`
2. Cosine similarity search (in-memory for MVP)
3. Integrate into `TutorOrchestrator`
4. Add citation formatting
5. Add context injection to prompts

**Success Criteria**:
- Tutor query: "Show me examples of courage" → retrieves Beowulf chunks
- Citations appear: *"From Beowulf, Chapter 3"*
- User can click citation → open reader at that chunk

---

### **Phase 5: Advanced Features** (Future)

- **sqlite-vec integration**: Replace in-memory search with SQLite extension
- **Metadata enrichment**: Reading levels, themes, archetype tagging
- **Re-chunking**: Allow different strategies (200/400/800 words)
- **Multilingual support**: Cohere multilingual embeddings
- **Admin UI**: Web-based ingestion dashboard

---

## Technical Considerations

### Database Schema Changes ✅ AUDIT ENHANCED

**New Tables**:
```sql
CREATE TABLE PublicTexts (
    Id INTEGER PRIMARY KEY,
    Title TEXT NOT NULL,
    AuthorTranslator TEXT,
    Region TEXT,
    PlaceOfOrigin TEXT,
    SourceName TEXT,
    SourceUrl TEXT,
    License TEXT,
    IngestionDate TEXT,
    OriginalFormat TEXT,
    EpubVersion TEXT,
    EpubBlob BLOB,
    Genre TEXT,
    TargetAgeRange TEXT,
    EstimatedWordCount INTEGER,
    -- AUDIT: Quality control fields
    IsOcrSource INTEGER DEFAULT 0,
    HasStructuralMetadata INTEGER DEFAULT 0,
    OcrConfidenceScore REAL DEFAULT 1.0,
    TrademarkStripped INTEGER DEFAULT 0
);

CREATE TABLE TextChapters (
    Id INTEGER PRIMARY KEY,
    PublicTextId INTEGER NOT NULL,
    ChapterNumber INTEGER,
    ChapterTitle TEXT,
    ChapterAnchor TEXT,
    EpubItemId TEXT,
    WordCount INTEGER,
    StartParagraphIndex INTEGER,
    EndParagraphIndex INTEGER,
    FOREIGN KEY (PublicTextId) REFERENCES PublicTexts(Id)
);

-- AUDIT: Parent chunks for context injection
CREATE TABLE TextParents (
    Id INTEGER PRIMARY KEY,
    PublicTextId INTEGER NOT NULL,
    TextChapterId INTEGER,
    PlainText TEXT NOT NULL,
    WordCount INTEGER,
    TokenCount INTEGER,
    StructuralPath TEXT,  -- "Genesis/1/1" for sacred texts
    VerseNumber INTEGER,
    FOREIGN KEY (PublicTextId) REFERENCES PublicTexts(Id),
    FOREIGN KEY (TextChapterId) REFERENCES TextChapters(Id)
);

-- AUDIT: Child chunks for vector search (smaller)
CREATE TABLE TextChunks (
    Id INTEGER PRIMARY KEY,
    PublicTextId INTEGER NOT NULL,
    TextChapterId INTEGER,
    ParentChunkId INTEGER,  -- AUDIT: Links to parent for context
    PlainText TEXT NOT NULL,
    WordCount INTEGER,
    TokenCount INTEGER,  -- AUDIT: Must be <= 380
    StartParagraphIndex INTEGER,
    EndParagraphIndex INTEGER,
    ChunkIndexInChapter INTEGER,
    SourceAnchor TEXT,
    FOREIGN KEY (PublicTextId) REFERENCES PublicTexts(Id),
    FOREIGN KEY (TextChapterId) REFERENCES TextChapters(Id),
    FOREIGN KEY (ParentChunkId) REFERENCES TextParents(Id)
);

CREATE TABLE ChunkEmbeddings (
    Id INTEGER PRIMARY KEY,
    TextChunkId INTEGER NOT NULL UNIQUE,
    EmbeddingModel TEXT,  -- "embed-v4.0" per audit
    GeneratedAt TEXT,
    VectorData BLOB,
    Dimensions INTEGER,  -- 1024 (can truncate to 256/512 with Matryoshka)
    FOREIGN KEY (TextChunkId) REFERENCES TextChunks(Id)
);

-- Add to existing NotebookEntries table
ALTER TABLE NotebookEntries ADD COLUMN PublicTextId INTEGER;
ALTER TABLE NotebookEntries ADD COLUMN TextChunkId INTEGER;
ALTER TABLE NotebookEntries ADD COLUMN SourceAttribution TEXT;
```

### Vector Storage Options

**MVP**: Store as JSON-serialized `float[]` in BLOB
```csharp
// Serialize
var json = JsonSerializer.Serialize(embedding);
entity.VectorData = Encoding.UTF8.GetBytes(json);

// Deserialize
var json = Encoding.UTF8.GetString(entity.VectorData);
var vector = JsonSerializer.Deserialize<float[]>(json);
```

**Future**: Use `sqlite-vec` extension for native vector similarity
```sql
-- After installing sqlite-vec
CREATE VIRTUAL TABLE vec_chunks USING vec0(
    chunk_id INTEGER PRIMARY KEY,
    embedding FLOAT[1024]
);

-- Similarity search
SELECT chunk_id, distance 
FROM vec_chunks 
WHERE embedding MATCH ? 
ORDER BY distance 
LIMIT 10;
```

### Cohere API Costs ✅ AUDIT UPDATED

**Embed API Pricing** (Feb 2026):
- `embed-v4.0`: **$0.10 per 1M tokens** (same as v3)

**Cost Estimate** (revised for token-based chunking):
- 18 texts × 30k words avg = 540k words
- ~1.33 tokens/word = 720k tokens
- **Chunking**: ~2,000 child chunks × 380 tokens = 760k tokens
- **Embedding cost**: 760k tokens × $0.10/1M = **$0.08 one-time**

**OCR Correction Cost** (Command R7B):
- Archive.org texts: ~2 texts × 30k words = 60k words
- Command R7B: $0.15 per 1M tokens
- Cost: ~$0.01

**Total MVP cost**: ~$0.09

**Scaling to 100 texts**: ~$0.50

### Error Handling Strategy

**Download failures**:
- Retry 3 times with exponential backoff
- Log unavailable sources to `ingestion_errors.log`
- Continue processing other texts

**Parsing failures**:
- Skip malformed chapters
- Log to Serilog with source URL
- Store partial data (e.g., chapters 1-5 succeed, 6 fails)

**Embedding failures**:
- Batch failed chunks into retry queue
- Resume from last successful chunk
- Store chunks without embeddings (can re-embed later)

---

## Security & Compliance

### Data Sovereignty
- All data stored in Canadian region (existing SQLite database)
- No data sent to external services except Cohere API (embeddings only)
- Embeddings are derived data, not raw content

### License Compliance
- All sources are **Public Domain** or CC0
- Store `License` field for each `PublicText`
- Display attribution in UI: *"Source: Project Gutenberg, Public Domain"*

### Privacy
- Public domain texts contain no PII
- User pins (`NotebookEntry`) are encrypted via existing `StoryEncryptionProvider`

---

## Testing Strategy

### Unit Tests (`StoryFort.Tests.Unit`)
- `SemanticChunker_Specs`: Validate word counts, overlap, paragraph boundaries
- `EpubParser_Specs`: Parse sample EPUB, verify metadata extraction
- `CohereEmbedService_Specs`: Mock HTTP client, verify payload format

### Integration Tests (`StoryFort.Tests.Integration`)
- `PublicTextIngestion_Specs`: End-to-end Alice ingestion
- `ChunkEmbedding_Specs`: Verify embeddings stored correctly
- `PinToChunk_Specs`: Link NotebookEntry → TextChunk

### Manual QA
- Download all 18 texts, inspect for missing chapters
- Test Sacred Texts HTML → EPUB conversion
- Validate attribution metadata accuracy

---

## Success Metrics

### MVP (Phase 1-2)
- ✅ 18 texts successfully ingested
- ✅ ~1,800 chunks with embeddings
- ✅ Zero data loss (all chapters preserved)
- ✅ Ingestion completes in < 30 minutes

### Phase 3-4 (Reading + RAG)
- ✅ Child can read and pin passages
- ✅ Tutor can answer: "Show me examples of X"
- ✅ Citations link back to source
- ✅ 95% user satisfaction with attribution clarity

---

## Dependencies

### NuGet Packages
```xml
<!-- StoryFort.Ingest -->
<PackageReference Include="VersOne.Epub" Version="4.0.0" />
<PackageReference Include="AngleSharp" Version="1.0.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />

<!-- StoryFort (existing, no changes needed) -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />
<PackageReference Include="Serilog" Version="3.1.1" />
```

### External Tools
- **Pandoc**: For HTML → EPUB conversion (Sacred Texts sources)
  - Install: `choco install pandoc`
  - Version: 3.0+

### API Dependencies
- **Cohere API**: Embed endpoint
  - Rate limit: 100 req/min (Trial)
  - Model: `embed-english-v3.0`

---

## Rollout Plan

### Week 1: Infrastructure
- Days 1-2: Create `StoryFort.Ingest` project, add models
- Day 3: Implement core services (downloader, parser)

### Week 2: Pipeline Implementation
- Days 4-5: Implement chunking and embedding
- Day 6: Alice in Wonderland PoC end-to-end
- Day 7: Batch process 5 more Gutenberg texts

### Week 3: Multi-Source Support
- Days 8-9: Sacred Texts HTML pipeline
- Day 10: Archive.org integration
- Day 11: Full 18-text ingestion

### Week 4: Integration
- Days 12-14: Reader UI with "Pin It" feature
- Days 15-16: RAG service integration
- Day 17: QA and refinement

---

## Open Questions / Future Decisions

1. **Vector Search**: When to migrate from in-memory to `sqlite-vec`?
   - Answer: After 500-user pilot validates RAG usage patterns

2. **Re-chunking Strategy**: What if we need different chunk sizes later?
   - Answer: Keep `OriginalFormat` strategy pattern; re-embed is cheap ($0.07)

3. **Metadata Enrichment**: Auto-tag themes/archetypes?
   - Answer: Phase 5 - use LLM to analyze and tag chunks

4. **Image Handling**: EPUBs contain illustrations. Store them?
   - Answer: MVP ignores images; Phase 5 extracts and displays

5. **Multi-language**: Ramayana has Sanskrit terms. Special handling?
   - Answer: Use Cohere multilingual embeddings if translation needed

---

## Appendix: Sample Code Snippets

### A. Main Ingestion Loop
```csharp
// Program.cs in StoryFort.Ingest
var seedPath = "public_text_seed.json";
var seed = JsonSerializer.Deserialize<PublicTextSeed>(
    await File.ReadAllTextAsync(seedPath));

foreach (var entry in seed.Texts)
{
    Console.WriteLine($"Processing: {entry.Title}...");
    
    // Check if already ingested
    if (await _repository.ExistsAsync(entry.Title))
    {
        Console.WriteLine($"  → Skipping (already exists)");
        continue;
    }
    
    try
    {
        // Download
        var epubPath = await _downloader.DownloadEpubAsync(
            entry.SourceUrl, $"epub_cache/{entry.Title}.epub");
        
        // Parse
        var parsed = await _parser.ParseAsync(epubPath);
        
        // Chunk
        var chunks = new List<TextChunkDto>();
        foreach (var chapter in parsed.Chapters)
        {
            var chunkBatch = _chunker.ChunkParagraphs(chapter.Paragraphs);
            chunks.AddRange(chunkBatch);
        }
        
        // Embed
        var texts = chunks.Select(c => c.PlainText).ToArray();
        var embeddings = await _embedder.EmbedBatchAsync(texts);
        
        // Store
        var publicText = new PublicText
        {
            Title = entry.Title,
            AuthorTranslator = entry.AuthorTranslator,
            Region = entry.Region,
            SourceUrl = entry.SourceUrl,
            IngestionDate = DateTime.UtcNow
        };
        
        await _repository.CreatePublicTextAsync(publicText);
        await _repository.AddChunksAsync(chunks, embeddings);
        
        Console.WriteLine($"  ✓ Ingested {chunks.Count} chunks");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✗ Error: {ex.Message}");
        _logger.LogError(ex, "Failed to ingest {Title}", entry.Title);
    }
}
```

### B. Semantic Chunking Algorithm
```csharp
public List<TextChunkDto> ChunkParagraphs(
    List<string> paragraphs, 
    int targetWords = 400, 
    int overlapWords = 50)
{
    var chunks = new List<TextChunkDto>();
    var currentChunk = new StringBuilder();
    var currentWordCount = 0;
    var startParaIndex = 0;
    var overlapBuffer = new Queue<string>();
    
    for (int i = 0; i < paragraphs.Count; i++)
    {
        var para = paragraphs[i];
        var words = para.Split(' ').Length;
        
        if (currentWordCount + words > targetWords && currentWordCount > 0)
        {
            // Emit current chunk
            chunks.Add(new TextChunkDto
            {
                PlainText = currentChunk.ToString().Trim(),
                WordCount = currentWordCount,
                StartParagraphIndex = startParaIndex,
                EndParagraphIndex = i - 1
            });
            
            // Start new chunk with overlap
            currentChunk.Clear();
            currentChunk.Append(string.Join(" ", overlapBuffer));
            currentWordCount = overlapBuffer.Sum(p => p.Split(' ').Length);
            startParaIndex = Math.Max(0, i - overlapBuffer.Count);
        }
        
        currentChunk.Append(para).Append(" ");
        currentWordCount += words;
        
        // Maintain overlap buffer
        overlapBuffer.Enqueue(para);
        if (overlapBuffer.Sum(p => p.Split(' ').Length) > overlapWords)
        {
            overlapBuffer.Dequeue();
        }
    }
    
    // Emit final chunk
    if (currentWordCount > 0)
    {
        chunks.Add(new TextChunkDto
        {
            PlainText = currentChunk.ToString().Trim(),
            WordCount = currentWordCount,
            StartParagraphIndex = startParaIndex,
            EndParagraphIndex = paragraphs.Count - 1
        });
    }
    
    return chunks;
}
```

---

**End of Architecture Document**

*Next Steps: Proceed to implementation Phase 1 (Alice in Wonderland PoC)*
