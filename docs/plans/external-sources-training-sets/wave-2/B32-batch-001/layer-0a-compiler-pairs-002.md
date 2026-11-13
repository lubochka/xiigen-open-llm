# Layer 0-A — Compiler Pairs (002)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-002-001 — "spike" alone → zero predicates → knowledge_edge

**Request:** "Run a spike to explore the best approach for the recommendation engine."

**Compiler analysis:**

Step 1 — scan for action predicates:
- "spike" → not in IMPLEMENT lexicon; not in any registered predicate lexicon → GAP vocabulary
- "explore" → not in IMPLEMENT lexicon (IMPLEMENT fires on: implement, build, create, write, code) → no predicate
- "recommendation engine" → domain noun only; no action predicate

Step 2 — scan for UI vocabulary:
- "screen", "page", "interface", "component", "form", "layout", "button", "input", "widget", "modal" → none present → UI does not fire

Step 3 — scan for ARCHITECTURE vocabulary:
- "design", "architect", "architecture", "structure", "pattern" → none present → ARCHITECTURE does not fire

Step 4 — scan for remaining predicate lexicons (API_CONTRACT, SECURITY, TEST, DOCUMENT, QA):
- None of their trigger words present

Step 5 — gap vocabulary detected:
- "spike" → gap-vibe-coding-spike-001
- "explore" → gap-vibe-coding-exploratory-001

**Result:** `[]` — zero predicates fire

**GapWarnings:**
- gap-vibe-coding-spike-001 ("spike")
- gap-vibe-coding-exploratory-001 ("explore")

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-002-002 — "spike the solution": still gap even with "the solution" present

**Request:** "Run a spike on the caching solution."

**Compiler analysis:**

- "spike" → GAP; gap-vibe-coding-spike-001
- "solution" → domain noun; not an action predicate
- No action verbs from IMPLEMENT lexicon present
- No UI vocabulary present

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-spike-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-002-003 — "explore" alone: not in predicate lexicon

**Request:** "Let's explore options for the data pipeline architecture."

**Compiler analysis:**

- "explore" → not in IMPLEMENT lexicon → no predicate from "explore"
- "architecture" → ARCHITECTURE fires on "architecture" keyword

**Result:** `["ARCHITECTURE"]`

**GapWarnings:** gap-vibe-coding-exploratory-001 ("explore")

**Note:** Even though "explore" is gap, ARCHITECTURE fires independently on "architecture". Gap warnings are emitted alongside routing — they do not suppress predicates.

**OutputDecision:** route to ARCHITECTURE flow

**DomainId:** "software-development"

---

## B32-0A-002-004 — "spike" + "design": ARCHITECTURE fires; spike remains gap

**Request:** "Spike the design of the event-sourcing module."

**Compiler analysis:**

- "spike" → GAP; gap-vibe-coding-spike-001
- "design" → ARCHITECTURE fires on "design"

**Result:** `["ARCHITECTURE"]`

**GapWarnings:** gap-vibe-coding-spike-001

**Rule confirmed:** Style/methodology vocabulary (spike) does not suppress registered action predicates. ARCHITECTURE fires on "design" regardless of "spike" framing.

**OutputDecision:** route to ARCHITECTURE flow

---
