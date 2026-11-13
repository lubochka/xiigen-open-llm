# Layer 0-C — Knowledge-Edge Pairs (050)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-050-001 — "testing would be nice": knowledge edge (aspirational)

**Knowledge edge: aspirational test mention, no imperative**

"Lean approach — some testing would be nice."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "testing would be nice" → aspirational/conditional; not an imperative verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-050-002 — "bang out tests" fires TEST: not knowledge edge

**Knowledge edge boundary:**

"Bang out some unit tests for the checkout flow."

Analysis:
- "bang out" → gap-vibe-coding-hack-001
- "unit tests" → TEST fires ✓

Predicates: ["TEST"]
Not knowledge_edge.

---

## B32-0C-050-003 — TEST trigger summary

**Knowledge edge: what fires TEST predicate**

| Expression | TEST fires? |
|---|---|
| "test X" (imperative) | YES |
| "write tests" | YES |
| "unit test X" | YES |
| "bang out tests" | YES |
| "add tests" | YES |
| "testing would be nice" | NO (aspirational) |
| "we should test" | NO (suggestion) |
| "no tests needed" | NO (negation) |
| "skip the tests" | NO (negation) |

---

## B32-0C-050-004 — "throwaway" + TEST: TEST fires

**Knowledge edge: "throwaway" gap qualifier + TEST**

"Write throwaway tests for the upload module."

Analysis:
- "throwaway" → gap-vibe-coding-style-001
- "write tests" → TEST fires ✓

Predicates: ["TEST"]
GapWarnings: [gap-vibe-coding-style-001]

---

## B32-0C-050-005 — "hack together [module] + bang out tests": both fire

**Knowledge edge: hack phrase + module fires IMPLEMENT; "bang out tests" fires TEST**

"Hack together the module and bang out tests."

Analysis:
- "hack together" → gap-vibe-coding-hack-001
- "module" (specific artifact) → IMPLEMENT fires ✓
- "bang out tests" → TEST fires ✓

Predicates: ["IMPLEMENT", "TEST"]
Not knowledge_edge.

---
