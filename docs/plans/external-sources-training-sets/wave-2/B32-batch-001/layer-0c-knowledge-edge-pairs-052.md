# Layer 0-C — Knowledge-Edge Pairs (052)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-052-001 — DomainId in knowledge_edge output: always "software-development"

**Knowledge edge: DomainId invariant check**

"Be pragmatic."

Analysis:
- "pragmatic" → gap-vibe-coding-style-001
- No action verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
DomainId: "software-development" (invariant)
Route: knowledge_edge + SCL

---

## B32-0C-052-002 — DomainId in active flow output

**Knowledge edge boundary: DomainId with active flow**

"Lean sprint — implement the feature."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires ✓

Predicates: ["IMPLEMENT"]
DomainId: "software-development" (invariant)

---

## B32-0C-052-003 — "expedient spike" + dashboard + IMPLEMENT: multi-gap

**Knowledge edge: multiple gap IDs with single predicate**

"Expedient spike — implement the dashboard."

Analysis:
- "expedient" → gap-vibe-coding-style-001
- "spike" → gap-vibe-coding-spike-001
- "implement" → IMPLEMENT fires ✓
- "dashboard" → gap-vibe-coding-dashboard-001

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-dashboard-001]
DomainId: "software-development"

---

## B32-0C-052-004 — All gap IDs: complete reference table

**Knowledge edge: complete gap ID reference for B32**

| Gap ID | Triggered by |
|---|---|
| gap-vibe-coding-style-001 | good enough, lean, pragmatic, expedient, barebones, stripped-down, no-frills, agile (informal), iterative, just get it working, clean up later, wing it, scrappy, throwaway, quick win, fast and loose, lo-fi, low-fi, rough cut, minimum viable, MVP |
| gap-vibe-coding-hack-001 | hack, hack together, cobble together, throw together, bang out, knock out, whip up, kludge together, hack it together |
| gap-vibe-coding-prototype-001 | prototype, PoC (Proof-of-Concept), proof-of-concept, paper prototype, wireframe |
| gap-vibe-coding-spike-001 | spike, technical spike, tracer bullet, walking skeleton, thin slice, vertical slice |
| gap-vibe-coding-rough-cut-001 | rough cut, rough-cut, rough draft |
| gap-vibe-coding-dashboard-001 | dashboard |

---
