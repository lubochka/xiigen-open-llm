# Layer 0-C — Knowledge-Edge Pairs (044)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-044-001 — "just get it working" alone: knowledge edge

**Knowledge edge: vibe phrase with no explicit action verb**

"Just get it working."

Analysis:
- "just get it working" → gap-vibe-coding-style-001 (vibe-code phrasing)
- "get working" → not an explicit IMPLEMENT trigger ("get it working" ≠ "implement")

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-044-002 — "clean up later" alone: knowledge edge

**Knowledge edge: deferred quality phrase with no action verb**

"We'll clean up later."

Analysis:
- "clean up later" → gap-vibe-coding-style-001 (deferred quality framing)
- "clean up" → refactoring intent; not in IMPLEMENT lexicon for B32
- No current-scope action verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-044-003 — "don't bother with" negation + IMPLEMENT: knowledge edge boundary

**Knowledge edge boundary: negation + positive predicate**

"Lean — implement the endpoint, don't bother documenting it."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires ✓
- "don't bother documenting" → DOCUMENT negated

Predicates: ["IMPLEMENT"]
Not knowledge_edge; IMPLEMENT fires.

---

## B32-0C-044-004 — "skip the QA" + IMPLEMENT: QA negated, IMPLEMENT fires

**Knowledge edge boundary:**

"Pragmatic push — implement feature, skip the QA."

Analysis:
- "pragmatic" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires ✓
- "skip the QA" → QA negated

Predicates: ["IMPLEMENT"]
Not knowledge_edge.

---

## B32-0C-044-005 — new gap vocabulary: "just get it working" and "clean up later"

**Knowledge edge: Round 4 gap vocabulary (049-060 preview)**

| Term / phrase | Gap ID | Zero predicates alone? |
|---|---|---|
| just get it working | gap-vibe-coding-style-001 | YES |
| clean up later | gap-vibe-coding-style-001 | YES |
| don't bother with | negation; depends on object | — |
| skip the | negation; depends on object | — |

---
