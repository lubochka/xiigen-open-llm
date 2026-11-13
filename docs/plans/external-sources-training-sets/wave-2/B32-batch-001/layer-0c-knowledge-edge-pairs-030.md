# Layer 0-C — Knowledge-Edge Pairs (030)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-030-001 — "tracer bullet" knowledge edge

**Term:** tracer bullet / tracer-bullet
**Category:** Category 4 — Technical investigation framing
**Gap ID:** gap-vibe-coding-spike-001
**Origin:** Pragmatic Programmer methodology; thin end-to-end path through all system layers to validate integration

**Knowledge edge summary:**
- Alone → zero predicates → knowledge_edge → SCHEDULE_CAPABILITY_LEARNING
- + implement → IMPLEMENT fires
- + design/architect → ARCHITECTURE fires
- + test → TEST fires

---

## B32-0C-030-002 — "walking skeleton" knowledge edge

**Term:** walking skeleton
**Category:** Category 4 — Technical investigation framing
**Gap ID:** gap-vibe-coding-spike-001
**Origin:** architecture/test-driven development (TDD) community; thin system with all layers but no full behaviour

**Knowledge edge summary:**
- Alone → zero predicates → knowledge_edge → SCHEDULE_CAPABILITY_LEARNING
- All Round 2 rules apply identically to walking skeleton as to other spike-family terms

---

## B32-0C-030-003 — "thin slice" knowledge edge

**Term:** thin slice / thin-slice / vertical slice
**Category:** Category 4 — Technical investigation framing
**Gap ID:** gap-vibe-coding-spike-001
**Origin:** Agile delivery; minimal end-to-end feature cutting all architectural layers

**Knowledge edge summary:**
- Alone → zero predicates → knowledge_edge
- "thin slice: implement feature and document API" → ["IMPLEMENT", "DOCUMENT"]

---

## B32-0C-030-004 — "paper prototype" knowledge edge

**Term:** paper prototype / wireframe / mockup
**Category:** Category 3 — Artifact-type without action (noun-only)
**Gap ID:** gap-vibe-coding-prototype-001

**Knowledge edge summary:**
- "paper prototype" alone → zero predicates
- "sketch out a paper prototype for the screen" → UI fires (screen triggers UI); sketch does not fire IMPLEMENT
- "create a paper prototype then implement" → IMPLEMENT fires (create = action verb)

---

## B32-0C-030-005 — "good enough" + "minimum viable": co-occurrence of same-ID qualifiers

**Edge case: two qualifiers mapping to same gap ID**

"Minimum viable good-enough auth service":
- "minimum viable" → gap-vibe-coding-style-001
- "good-enough" → gap-vibe-coding-style-001

Both map to same ID → one gap-vibe-coding-style-001 warning (not two).
No action verb → Predicates: []
SCHEDULE_CAPABILITY_LEARNING: emitted once.

DomainId: "software-development"

---
