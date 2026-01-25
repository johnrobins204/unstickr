# Academic Rationale for UX Decisions

This document outlines the psychological and educational principles guiding the User Experience (UX) design of Unstickd, specifically concerning the "Unstickr" (AI Tutor) intervention logic.

## 1. Generative Pauses vs. Transcription Pauses
**Concept:** Writing involves distinct cognitive phases. *Transcription pauses* (typing, spelling) are short. *Generative pauses* (planning narrative arcs) are significantly longer. Rigid activity timers often interrupt productive "thinking" time, breaking the writer's flow.
**Design Implication:** The inactivity timer must be context-sensitive. If the user stops after a punctuation mark (signaling the end of a thought), the allowable pause duration corresponds to a "Generative Pause" and is doubled compared to a mid-sentence stop.
*   **Matsuhashi, A.** (1981). Pausing and planning: The tempo of written discourse production. *Research in the Teaching of English*, 15(2), 113-134.
*   **Schilperoord, J.** (1996). *It's about time: Temporal aspects of cognitive processes in text production*. Utrecht Studies in Language and Communication.

## 2. Locus of Control & Autonomy
**Concept:** Children's motivation relies on a sense of *Autonomy* (Self-Determination Theory). An AI that intervenes uninvited shifts the *Locus of Control* to the machine ("The computer is making me write"), reducing ownership.
**Design Implication:** The AI never "pops up" with a solution. It uses an **Opt-In Trigger** (e.g., a subtle animation or "?" icon) that requires the child to actively click or request help. This preserves the internal locus of control ("I asked for help").
*   **Ryan, R. M., & Deci, E. L.** (2000). Self-determination theory and the facilitation of intrinsic motivation, social development, and well-being. *American Psychologist*, 55(1), 68–78.
*   **Rotter, J. B.** (1966). Generalized expectancies for internal versus external control of reinforcement. *Psychological Monographs: General and Applied*, 80(1), 1–28.

## 3. Scaffolding (Zone of Proximal Development)
**Concept:** Learning occurs best when assistance is provided only for tasks the learner cannot *quite* do alone (ZPD). Over-helping creates dependency; under-helping creates anxiety.
**Design Implication:** The "Unstickr" provides *Scaffolding*, not answers. Socratic questioning moves the child through the ZPD rather than effectively "ghostwriting," which would bypass the learning zone entirely.
*   **Vygotsky, L. S.** (1978). *Mind in society: The development of higher psychological processes*. Harvard University Press.
*   **Wood, D., Bruner, J. S., & Ross, G.** (1976). The role of tutoring in problem solving. *Journal of Child Psychology and Psychiatry*, 17(2), 89–100.

## 4. The CASA Paradigm (Computers As Social Actors)
**Concept:** Humans, especially children, apply social norms to computers. If an AI character appears "bored" or "judgmental" (e.g., tapping a watch) during a pause, the child experiences social performance anxiety.
**Design Implication:** The "Unstickr" primarily engages in **Parallel Play** (reading their own book) rather than supervision. Interventions are framed as peer sharing, not teacher correction.
*   **Reeves, B., & Nass, C.** (1996). *The media equation: How people treat computers, television, and new media like real people and places*. Cambridge University Press.
*   **Nass, C., & Moon, Y.** (2000). Machines and mindlessness: Social responses to computers. *Journal of Social Issues*, 56(1), 81–103.

## 5. Cognitive Load & Inhibition Control
**Concept:** Children have developing executive functions, specifically inhibition control. Sudden "bottom-up" stimuli (flashing lights, pop-ups) hijack attention, clearing the working memory of the sentence they were trying to formulate.
**Design Implication:** Triggers use **Ambient Signaling** (slow fades, color shifts) rather than sudden movement or sound, preserving the child's *Cognitive Load* for the creative task.
*   **Sweller, J.** (1988). Cognitive load during problem solving: Effects on learning. *Cognitive Science*, 12(2), 257–285.
*   **Diamond, A.** (2013). Executive functions. *Annual Review of Psychology*, 64, 135–168.

## 6. Over-Justification Effect
**Concept:** Extrinsic rewards (gamification, points, flashy animations) can diminish intrinsic motivation for tasks a child already enjoys.
**Design Implication:** The interaction is **Functionally Minimalist**. The reward for interacting with the AI is *conceptual help*, not entertainment or points.
*   **Lepper, M. R., Greene, D., & Nisbett, R. E.** (1973). Undermining children's intrinsic interest with extrinsic reward: A test of the "overjustification" hypothesis. *Journal of Personality and Social Psychology*, 28(1), 129–137.

## 7. Metacognition
**Concept:** Mature writers move from "knowledge-telling" (dumping ideas) to "knowledge-transforming" (reflecting on the text).
**Design Implication:** The trigger system supports **Metacognitive Prompting**. The prompt asking "Are you stuck?" forces the child to self-evaluate their mental state, a key step in developing writerly maturity.
*   **Bereiter, C., & Scardamalia, M.** (1987). *The psychology of written composition*. Lawrence Erlbaum Associates.
*   **Flavell, J. H.** (1979). Metacognition and cognitive monitoring: A new area of cognitive-developmental inquiry. *American Psychologist*, 34(10), 906–911.
