# Layer 0-C — Knowledge-Edge Pairs (017)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-017-001 — "low-fi" and "lo-fi": gap vocabulary for quality-level framing

**Knowledge edge:** "low-fi" and "lo-fi" communicate low-fidelity, low-polish quality expectations. Neither is an action predicate.

**Rule:**
```
IF "low-fi" or "lo-fi" appears WITHOUT action verb
THEN predicates = [] AND gap-vibe-coding-style-001 emitted

IF "low-fi" or "lo-fi" appears WITH action verb
THEN predicate fires AND gap-vibe-coding-style-001 is a warning
```

**Examples:**

| Request | Predicates | Notes |
|---|---|---|
| "Low-fi version" | [] | No action verb |
| "Low-fi onboarding flow" | [] | Domain noun, no action |
| "Build a low-fi version" | [IMPLEMENT] | "build" fires |
| "Low-fi build" | [IMPLEMENT] | "build" fires |
| "Lo-fi implementation" | [IMPLEMENT] | "implementation" root fires |
| "Low-fi wireframe design" | [ARCHITECTURE] | "design" fires |

---

## B32-0C-017-002 — "low-fi prototype": compound gap case

**Request:** "Just a low-fi prototype for now."

Both "low-fi" and "prototype" are gap vocabulary with no action verb:
- "low-fi" → gap-vibe-coding-style-001
- "prototype" alone → gap-vibe-coding-prototype-001

Result: two gap IDs, zero predicates, knowledge_edge route.

**Teaching:** Multiple gap terms in a single request produce multiple gap IDs. SCHEDULE_CAPABILITY_LEARNING is still emitted only once.

---

## B32-0C-017-003 — "low-fi" does not imply reduced feature set; it implies quality framing only

**Conceptual note for training:**

"Low-fi" is a quality-bar indicator. It does not change which predicates fire. A "low-fi implementation" still fires IMPLEMENT exactly the same as a "high-quality implementation". The system makes no distinction in routing based on "low-fi" vs "high-fi" framing — it routes on what to do (action verbs) not on quality expectations.

Gap warning is emitted to signal that the user is working in an exploratory, low-quality-bar mode, which may be relevant for downstream learning.

---
