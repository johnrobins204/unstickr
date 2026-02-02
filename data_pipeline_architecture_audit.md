Regulatory Compliance and Architectural Optimization of Literary Data Pipelines for Retrieval-Augmented Generation
The convergence of historical preservation and generative artificial intelligence has necessitated the development of specialized ingestion frameworks, such as the Public Domain Text Ingestion Pipeline employed by the StoryFort project. This system, which seeks to integrate literary and sacred texts into a dual-storage architecture for reading and retrieval-augmented generation (RAG), operates at the intersection of significant legal, ethical, and technical challenges. As the digital humanities transition toward large-scale vectorization of the human record, the methodologies used to acquire, normalize, and chunk these data must be evaluated against the shifting landscape of repository governance and the rigorous benchmarks of modern computational linguistics.   

Analysis of Repository Terms of Service and Regulatory Compliance
The StoryFort pipeline relies on a multi-source strategy to ingest literary works from Project Gutenberg, the Internet Sacred Text Archive (ISTA), and Archive.org. Each of these repositories maintains a distinct governance model designed to protect bandwidth, respect donor intent, and manage intellectual property rights. A critical evaluation of these policies reveals several points of friction between the planned automated work and the providers' established terms of service.   

Project Gutenberg: Automated Access and Mirroring Restrictions
Project Gutenberg serves as the primary text provider for the pipeline due to its robust offering of EPUB formats. However, the project’s policies regarding automated access are restrictive. The website is explicitly intended for human users, and any perceived use of automated tools to access the main site typically results in a temporary or permanent block of the originating IP address. The StoryFort pipeline’s current implementation, which utilizes a C# based IEpubDownloader to fetch files directly from specific URLs, stands in direct conflict with this human-use requirement.   

To remain compliant, the architecture must transition away from direct site scraping toward the use of the Project Gutenberg mirror network. The project defines "many books" as more than approximately 100 downloads per day; exceeding this threshold requires the use of mirrors or the rsync protocol. Furthermore, the project mandates a minimum two-second delay between requests when using automated harvesting tools like wget. The current pipeline, which batches requests for embedding services, does not explicitly mention a throttling mechanism for the ingestion stage, posing a risk of automated blocking.   

Policy Dimension	Project Gutenberg Requirement	Pipeline Compliance Status
Access Method	
Human-only web interface 

Non-compliant (Automated C# downloader) 

Download Volume	
Max 100/day from main site 

High Risk (Depending on seed size) 

Harvesting Delay	
2-second wait between requests 

Unspecified in architecture 

Automated Channel	
Mirroring via rsync 

Not utilized in current plan 

Trademark Usage	
20% royalty for commercial use 

Contingent on StoryFort's business model 

  
The "Project Gutenberg" trademark represents another significant compliance hurdle. While the texts themselves are in the public domain in the United States, the name and branding are protected trademarks. If the StoryFort project operates as a commercial entity, it must either pay a 20% royalty on gross income derived from the use of the trademarked texts or remove all references to Project Gutenberg from the ingested files. The current architecture’s metadata extraction includes the license field, which may inadvertently retain trademarked strings in the RAG layer, complicating future commercialization.   

Internet Sacred Text Archive: Bandwidth Preservation and Site Integrity
The inclusion of five texts from the Internet Sacred Text Archive (ISTA) introduces complexities related to the "short file" structure of the repository. ISTA typically breaks long books into numerous small HTML fragments to minimize server load and hosting costs. The StoryFort pipeline’s reliance on HTML-to-EPUB conversion using Pandoc implies a need for sequential scraping of these fragments. However, ISTA explicitly identifies itself as a "quiet place in cyberspace" and has historically been sensitive to high-volume scraping that ignores its structural nuances.   

A primary conflict involves the copyright status of the meta-data and descriptions hosted on ISTA. While the ancient texts themselves are public domain, the introductory essays, category index descriptions, and founder-written commentaries are copyrighted by Evinity Publishing INC. The pipeline’s use of AngleSharp to parse HTML must be precisely calibrated to extract only the public domain content and exclude these proprietary additions. Furthermore, ISTA discourages the use of PDFs and prefers contributors provide RTF or TXT files, suggesting that their internal systems are optimized for text-light interactions rather than heavy-duty AI ingestion.   

Archive.org: Fair Use and API Governance
Archive.org provides a more sophisticated API environment, including metadata and full-text search endpoints. The archive’s Terms of Use, updated in 2014, emphasize that use should be limited to non-infringing or fair use under copyright law. While the archive offers free storage and bandwidth, it does not guarantee the copyright status of the items it hosts, placing the burden of "non-infringing use" entirely on the pipeline operator.   

The StoryFort pipeline’s preference for EPUB but willingness to fall back to OCR for Archive.org files introduces a potential quality and compliance issue. OCR data on Archive.org can be of variable quality, and the right to use the scanned images of a book may be distinct from the right to use the underlying text. The archive’s OpenSearch and CDX APIs are designed for researchers, and while they support automated access, they require adherence to "legitimate interest" frameworks under regulations like GDPR.   

Technical Evaluation of the RAG Pipeline Architecture
The design of the StoryFort data pipeline reflects several modern conventions but also reveals critical bottlenecks that could undermine its effectiveness for literary and sacred text retrieval. Evaluation of the chunking and embedding strategies shows a potential mismatch between the semantic nature of the data and the mathematical constraints of the chosen models.

Semantic Chunking and the Tokenization Mismatch
The ISemanticChunker component processes extracted paragraphs into segments of approximately 400 words with a 50-word overlap. In literary text processing, paragraph integrity is vital because context—such as character names and coreference chains—is often contained within these boundaries. However, the use of word count as the primary unit for chunking is an architectural weakness when paired with the Cohere embed-english-v3.0 model.   

The Cohere v3.0 model has a maximum context length of 512 tokens. In standard English prose, the ratio of tokens to words is approximately 1.3 to 1. Consequently, a 400-word chunk will typically result in roughly 520 to 540 tokens.   

Token Count≈Word Count×1.35
If the pipeline sends a 400-word chunk to the Cohere API without prior token-level validation, the model will truncate the trailing text to fit the 512-token limit. This silent truncation leads to the loss of information at the end of every chunk, potentially severing critical narrative or theological conclusions. To align with best practices, the chunking strategy should be adjusted to token-based limits or utilize a model with a larger context window, such as Cohere v4.0, which supports up to 128,000 tokens.   

Embedding Model Selection and Dimensionality
The pipeline utilizes Cohere embed-english-v3.0, generating 1024-dimensional vectors. This model is highly effective for semantic search and retrieval-augmented generation due to its compression-aware training and high performance on the Massive Text Embedding Benchmark (MTEB). A key feature of this model is the input_type parameter, which optimizes the vector space for different tasks.   

Parameter	Recommended Value	Impact on Retrieval
input_type	
search_document 

Optimizes text passages for storage in a vector database.

input_type	
search_query 

Optimizes the query vector to match the document space.

embedding_types	
float or int8 

Higher dimensions (float) improve accuracy; int8 reduces storage.

Dimensions	
1024 

Captures fine-grained semantic relationships in English text.

  
While v3.0 is a robust choice, the emergence of Cohere v4.0 in late 2025 and early 2026 suggests an upgrade path. v4.0 is multimodal, allowing for the ingestion of PDF screenshots and interleaved images, which could be relevant for archival texts containing illustrations or complex layouts that AngleSharp might fail to parse. Furthermore, v4.0 supports "Matryoshka Embeddings," which allow for the truncation of vectors to lower dimensions (e.g., 256 or 512) while retaining most of the retrieval performance, thereby optimizing the SQLite storage layer.   

Advanced Retrieval Techniques: Late Chunking and Parent-Child RAG
Current best practices in RAG development for 2026 emphasize the importance of context-sensitive chunking over fixed-length or simple paragraph-based splitting. The "Chunking Goldilocks Problem"—where chunks are either too large (introducing noise) or too small (losing context)—is particularly acute in literary fiction.   

One revolutionary approach is "Late Chunking," which leverages long-context embedding models. Instead of splitting the text before embedding, the system embeds the entire document (or a large macro-chunk of up to 8,000 tokens) as a whole. The transformer model’s self-attention mechanism ensures that the embedding for each token is informed by the surrounding context across the entire text. Only after the token embeddings are generated does the system apply mean pooling to create smaller chunk vectors.   

h 
i
​
 =Pooling(Transformer(x 
1
​
 ,...,x 
n
​
 ))for x∈chunk j
For a narrative where anaphoric references (e.g., "he," "it," "the former") span multiple paragraphs, late chunking ensures that the chunk-level vector retains a semantic trace of the entities introduced chapters earlier.   

Alternatively, "Parent-Child" retrieval strategies offer a middle ground for production systems. This involves creating small, highly specific "child" chunks for precise vector matching, but retrieving the larger "parent" chunk (the full chapter or section) to provide the LLM with sufficient context for response generation. This would address the StoryFort pipeline's dual-storage intent by linking precise retrieval units to the original structural entities in the SQLite source layer.   

Literary and Sacred Text Processing: Linguistic and Technical Nuance
Processing literary and sacred texts for AI retrieval is fundamentally different from processing technical documentation. The nature of the language—often archaic, highly metaphorical, or structurally complex—requires a more nuanced preprocessing pipeline than the one currently described.

Addressing OCR Artifacts and Archaic Spelling
Public domain texts, especially those derived from scans on Archive.org, are often plagued by character-level misrecognitions. Common OCR errors include substitutions like "l/1/!" or "0/O," which can fundamentally alter the semantic meaning of a passage and lead to degraded retrieval scores. Best practice involves a denoising stage using frameworks like REVISE, which systematically corrects errors at the character, word, and structural levels before the text is sent to the embedding service.   

For literary texts, archaic spellings (e.g., "shew" for "show" or "connexion" for "connection") can also cause issues. While modern embedding models like Cohere v3.0 are trained on diverse datasets, they may struggle with fine-grained text differentiation in noisy or non-standard English. Implementing a normalization layer that uses lemmatization while preserving the original context is often necessary to align archaic texts with modern user queries.   

Metadata Enrichment and Document Hierarchy
The StoryFort pipeline extracts basic metadata such as Title, Author, and License. However, current best practices for RAG in 2026 suggest that "metadata enrichment" is a high-impact, low-cost method for improving retrieval precision. Adding attributes such as parent_id, chapter_index, temporal_setting, and category_depth allows for filtered searches that significantly reduce the search space and improve relevance.   

For sacred texts, structural metadata is even more critical. Many religious works follow a strict hierarchy (Book > Chapter > Verse). The pipeline should utilize AngleSharp to capture these structural markers and store them as metadata alongside the text chunks. This ensures that the AI tutor can not only retrieve the text but also cite its precise location in the sacred corpus, a requirement for high-stakes domains like theology.   

Jurisdictional Risk and Global Regulatory Trends
The deployment of the StoryFort pipeline in 2026 must also account for a shifting global regulatory environment, particularly concerning AI training on copyrighted or public domain materials.

Canada and the Fair Dealing Doctrine
If the StoryFort pipeline is developed or deployed in Canada, it faces a more restrictive intellectual property regime than the United States. Canada does not have a broad "Fair Use" doctrine; instead, it utilizes "Fair Dealing," which is limited to eight specific purposes: research, private study, education, parody, satire, criticism, review, and news reporting.   

AI developers in Canada have lobbied for a Text and Data Mining (TDM) exception to the Copyright Act, but this has been resisted by the creative community. As a result, using Retrieval-Augmented Generation to surface content can be interpreted not only as a reproduction right issue but also as a "communication right" issue, potentially exposing the project to liability if it distributes RAG outputs to Canadian users.   

Bill C-27 and the Artificial Intelligence and Data Act (AIDA)
The Canadian government’s Bill C-27, which includes the Artificial Intelligence and Data Act (AIDA), is expected to come into force as early as 2025 or 2026. AIDA will regulate "high-impact" AI systems, requiring proactive identification and mitigation of risks related to bias and safety. For a project like StoryFort, which involves the ingestion of sacred and historical texts, the risk of "discriminatory outcomes" or "hallucinations" in theological contexts would likely classify the system as high-impact, requiring rigorous documentation and human oversight.   

Furthermore, the 2026 Public Domain Day in the United States will see the release of thousands of works from 1930. While this expands the available data corpus, the pipeline must remain vigilant regarding the "Rule of 95," which protects works first published between 1923 and 1978 for 95 years. The current pipeline’s focus on 18 seed texts from 2023-updated sources suggests it is operating within these bounds, but scalability will require automated copyright clearance verification.   

Comprehensive Design Recommendations for Pipeline Optimization
To transform the StoryFort pipeline into a well-designed, compliant, and state-of-the-art system, several architectural and procedural modifications are recommended.

Immediate Structural Adjustments
The current automated downloader should be replaced with an asynchronous mirroring system. By utilizing rsync to pull from a Project Gutenberg mirror, the pipeline can avoid IP blocks and comply with the site’s intention for human-only interaction. For Sacred Texts, a crawler should be implemented that respects the "short file" structure and includes a five-second politeness delay to prevent server strain.   

The chunking logic must be moved from word-based to token-based. Implementing the Cohere tokenizer within the ISemanticChunker will ensure that no chunk exceeds the 512-token limit of the v3.0 model, preventing data loss.   

Integration of Advanced RAG Patterns
To address the contextual needs of literary fiction, the pipeline should adopt a "Parent-Child" storage model. The SQLite RAG layer should store both the child chunks (for vector search) and the parent paragraphs (for context injection). When a similarity search returns a high-scoring child chunk, the system should automatically inject the parent paragraph into the LLM prompt, ensuring that character names and settings are preserved.   

For Archive.org sources, an LLM-based OCR correction layer should be added. This would involve taking the raw OCR text and passing it through a small, fast model (e.g., Command R7B) to fix obvious character errors before embedding.   

Metadata and Governance Enhancements
The storage of embeddings separately from the text snippets is a critical recommendation to allow for future re-embedding with improved models without losing the source data. Additionally, the SQLite layer should be enriched with structural metadata extracted via AngleSharp, including verse numbers for sacred texts and chapter titles for fiction.   

Feature	Current Strategy	Proposed Enhancement
Chunking	
400 words (Approx 532 tokens) 

380 tokens (Safety margin) 

Parsing	
AngleSharp 

AngleSharp + Structural Tagging 

Embedding	
Cohere v3.0 

Cohere v4.0 (Multimodal) 

Retrieval	
Single-tier chunking 

Parent-Child Retrieval 

Licensing	
Trademark retention 

Automated Trademark Stripping 

  
Finally, the project should establish a proactive risk management strategy to comply with AIDA and PIPEDA regulations. This includes obtaining informed consent for data processing and maintaining a human-in-the-loop validation process for the AI tutor’s responses, particularly when interpreting sacred knowledge that carries communal and spiritual significance. By synthesizing these legal, technical, and linguistic considerations, the StoryFort project can develop a pipeline that is not only well-designed but also sustainable in the evolving landscape of 2026.   



data_pipeline_architecture.md

gutenberg.org
Information About Robot Access to our Pages - Project Gutenberg
Opens in a new window

gutenberg.org
Terms of Use | Project Gutenberg
Opens in a new window

gutenberg.org
Privacy Policy - Project Gutenberg
Opens in a new window

gutenberg.org
Mirroring How To | Project Gutenberg
Opens in a new window

stackoverflow.com
How to strip headers/footers from Project Gutenberg texts? - Stack Overflow
Opens in a new window

en.wikipedia.org
Project Gutenberg - Wikipedia
Opens in a new window

gutenberg.org
Project Gutenberg License
Opens in a new window

sacred-texts.com
Faq | Internet Sacred Text Archive
Opens in a new window

sacred-texts.com
(ISTA) - World's Largest Free Religious & Spiritual Text Archive ...
Opens in a new window

help.archive.org
API information - Internet Archive Help Center
Opens in a new window

support.archive-it.org
About Archive-It APIs and access integrations
Opens in a new window

blog.archive.org
Update to Terms of Use | Internet Archive Blogs
Opens in a new window

help.archive.org
Rights - Internet Archive Help Center
Opens in a new window

help.archive.org
Archive.org Information
Opens in a new window

zilliz.com
The guide to embed-english-v3.0 model | Cohere - Zilliz
Opens in a new window

docs.oracle.com
Cohere Embed English 3 - Oracle Help Center
Opens in a new window

aws.amazon.com
AWS Marketplace: Cohere Embed Model v3 - English
Opens in a new window

ai.azure.com
Embed-v4.0 Evaluations - AI Model Catalog | Microsoft Foundry Models
Opens in a new window

aws.amazon.com
Powering enterprise search with the Cohere Embed 4 multimodal embeddings model in Amazon Bedrock | Artificial Intelligence
Opens in a new window

clarifai.com
cohere-embed-english-v3_0 model | Clarifai - The World's AI
Opens in a new window

docs.cohere.com
Introduction to Embeddings at Cohere
Opens in a new window

docs.cohere.com
Cohere's Embed Models (Details and Application)
Opens in a new window

medium.com
Comparing Cohere, Amazon Titan, and OpenAI Embedding Models: A Deep Dive - Medium
Opens in a new window

app.ailog.fr
Best Embedding Models 2025: MTEB Scores & Leaderboard (Cohere, OpenAI, BGE) - Ailog
Opens in a new window

medium.com
Chunking in RAG: The RAG Optimization Nobody Talks About | by Nikhil Dharmaram | Jan, 2026 | Medium
Opens in a new window

medium.com
The Science of Chunking: Why Your RAG System's Success Lives and Dies Here - Medium
Opens in a new window

arxiv.org
Late Chunking: Contextual Chunk Embeddings Using Long-Context Embedding Models - arXiv
Opens in a new window

github.com
jina-ai/late-chunking: Code for explaining and evaluating late chunking (chunked pooling) - GitHub
Opens in a new window

bluetickconsultants.com
Late Chunking in RAG: Improving Text Retrieval Performance - Bluetick Consultants Inc.
Opens in a new window

datacamp.com
Late Chunking for RAG: Implementation With Jina AI | DataCamp
Opens in a new window

reddit.com
Should I use late chunking or stick with naïve chunking for 4–5k token articles? - Reddit
Opens in a new window

reddit.com
The Beauty of Parent-Child Chunking. Graph RAG Was Too Slow for Production, So This Parent-Child RAG System was useful - Reddit
Opens in a new window

eu-opensci.org
Enhancing Optical Character Recognition (OCR) Accuracy in Healthcare Prescription Processing using Artificial Neural Networks - European Open Science
Opens in a new window

aclanthology.org
A Framework for Revising OCRed text in Practical Information Systems with Data Contamination Strategy - ACL Anthology
Opens in a new window

analyticsvidhya.com
Text Cleaning Methods in NLP - Analytics Vidhya
Opens in a new window

analyticsvidhya.com
How to Become a RAG Specialist in 2026? - Analytics Vidhya
Opens in a new window

unstructured.io
Metadata for RAG: Improve Contextual Retrieval | Unstructured
Opens in a new window

neo4j.com
Advanced RAG Techniques for High-Performance LLM Applications - Graph Database & Analytics - Neo4j
Opens in a new window

kanerika.com
Top 7 RAG Tools You Should Know About in 2026 - Kanerika
Opens in a new window

dhiwise.com
Building a Robust RAG Pipeline: The Complete Guide for 2025 - DhiWise
Opens in a new window

dev.to
Learn How to Build Reliable RAG Applications in 2026! - DEV Community
Opens in a new window

hughstephensblog.net
OpenAI – Hugh Stephens Blog
Opens in a new window

osler.com
AI training on trial: the next legal frontier in copyright law - Osler, Hoskin & Harcourt LLP
Opens in a new window

ised-isde.canada.ca
The Artificial Intelligence and Data Act (AIDA) – Companion document - Canada.ca
Opens in a new window

torkin.com
Top Five Artificial Intelligence Trends Shaping Canada's Legal Landscape in 2026
Opens in a new window

web.law.duke.edu
Public Domain Day 2026 | Duke University School of Law
Opens in a new window

gutenberg.org
Copyright How-To | Project Gutenberg
Opens in a new window

webapps.stackexchange.com
How to download all English books from Gutenberg? - Web Applications Stack Exchange
Opens in a new window

metacto.com
Cohere API Pricing 2026: Command R+, Rerank & Embed Costs | MetaCTO
Opens in a new window

spiedigitallibrary.org
Correction of OCR results using large language models - SPIE Digital Library
Opens in a new window

csriprnusrl.wordpress.com
Copyrighting Scared Knowledge: Legal and Ethical Challenges in the Commercialization of Religious Texts, Chants and Indigenous