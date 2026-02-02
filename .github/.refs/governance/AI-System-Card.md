# AI System Card: StoryFort (Pilot Phase)

**Date:** February 1, 2026  
**Model:** Cohere Command R+ (Reasoning)  
**Role:** Creative Writing Tutor

## 1. Intended Use and Scope
StoryFort is designed as a **scaffolding tool** for creative writing in Grades 4-8. 
*   **Primary Function:** To ask sensory questions, suggest plot alternatives, and unblock "writer's block."
*   **Out of Scope:** It is not a fact-checking engine, a math tutor, or a mental health counselor. It is not designed to write the story for the student ("No Ghostwriting" Policy).

## 2. Limitations (What Teachers Need to Know)
*   **Probabilistic Nature:** The AI generates text based on patterns, not facts. It may "hallucinate" (invent) details. It should not be treated as an authoritative source on history or science.
*   **Context Window:** The AI currently sees only the most recent summary of the story. It may forget details written 10 pages ago if they are not in the current context summary.
*   **Bias:** Like all Large Language Models (LLMs), it was trained on vast internet text and may reflect societal biases.

## 3. Safety Guardrails implemented
We employ a "Defense in Depth" strategy:

| Layer | Control | Status |
|-------|---------|--------|
| **Input Filter** | Blocks patterns like "Ignore previous instructions" (Prompt Injection), potential PII (email addresses), and inappropriate content via BannedWordsPatterns regex. | ✅ Active (SafeguardService implemented with configurable patterns in appsettings.json) |
| **System Prompt** | Instructions to the AI to be polite, Socratic (ask questions rather than answer), and refuse harmful requests. | ✅ Active |
| **Data Privacy** | PII stripping before transmission. No user data used for model training (Cohere Enterprise Policy). | ✅ Active |
| **Human Oversight** | "Supervisor Primacy" principle—Teachers can review AI interaction logs. | ⚠️ Planned (Post-Pilot) |

## 4. Data Usage
*   **Input:** Student narrative text (anonymized) sent to Cohere API in Canada.
*   **Storage:** No student data is stored by Cohere.
*   **Training:** Data sent via StoryFort is **excluded** from Cohere's training sets.

## 5. Feedback Mechanism
Extensive controls make it extremely unlikely that any unintended AI output is displayed on screen, however a "Report AI Output" button will be available that will alert StoryFort engineers who will investigate, remediate, and report back to the originator.
