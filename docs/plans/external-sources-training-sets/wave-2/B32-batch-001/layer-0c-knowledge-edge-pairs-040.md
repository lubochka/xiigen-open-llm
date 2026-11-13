# Layer 0-C — Knowledge-Edge Pairs (040)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-040-001 — gap + ARCHITECTURE verb fires: not knowledge edge

**Knowledge edge: "architect" verb fires ARCHITECTURE**

"Lean architecture — architect the service boundary."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "architect" → ARCHITECTURE fires ✓

Predicates: ["ARCHITECTURE"]
Not knowledge_edge.

---

## B32-0C-040-002 — "architecture" noun only: knowledge edge

**Knowledge edge: "architecture" as noun with no action verb**

"We need a lean architecture."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "architecture" → noun
- "need" → desire verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-040-003 — "design" verb fires ARCHITECTURE

**Knowledge edge: "design" as ARCHITECTURE trigger**

"Lean approach — design the data layer."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "design" → ARCHITECTURE fires ✓

Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-style-001]

---

## B32-0C-040-004 — mixed multi-qualifier alone: knowledge edge

**Knowledge edge: three qualifiers, no action verbs**

"Lean, agile, iterative."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "agile" → gap-vibe-coding-style-001
- "iterative" → gap-vibe-coding-style-001
- No action verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL (once)

---

## B32-0C-040-005 — ARCHITECTURE verbs summary

| Verb | ARCHITECTURE fires? |
|---|---|
| "architect the service" | YES |
| "design the module" | YES |
| "structure the layers" | YES |
| "plan the architecture" | YES |
| "architecture" (noun only) | NO |
| "we need an architecture" | NO |

---
