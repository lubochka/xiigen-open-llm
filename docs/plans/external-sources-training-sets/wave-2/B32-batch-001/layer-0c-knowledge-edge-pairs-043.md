# Layer 0-C — Knowledge-Edge Pairs (043)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-043-001 — "no tests needed" alone: knowledge edge (no gap vocabulary)

**Knowledge edge: negation only, no gap vocabulary**

"No tests needed — just ship it."

Analysis:
- "no tests needed" → negation; TEST does not fire
- "just ship it" → delivery phrase; no predicate
- No gap vocabulary present → no SCL

Predicates: []
GapWarnings: []
Route: (normal empty; no SCL since no gap vocabulary)

---

## B32-0C-043-002 — "good enough" + negation: gap fires SCL, negation does not fire TEST

**Knowledge edge: gap vocabulary + negated TEST**

"Good enough — no tests needed."

Analysis:
- "good enough" → gap-vibe-coding-style-001
- "no tests needed" → TEST negated; does NOT fire

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-043-003 — IMPLEMENT + negated TEST: only IMPLEMENT fires

**Knowledge edge boundary: negation co-existing with action verb**

"Lean sprint — implement feature, don't bother with tests."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires ✓
- "don't bother with tests" → TEST negated

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted

Not knowledge_edge. IMPLEMENT fires; TEST correctly absent.

---

## B32-0C-043-004 — negation patterns summary

| Negation expression | Effect on predicate |
|---|---|
| "no tests needed" | TEST does NOT fire |
| "don't bother with tests" | TEST does NOT fire |
| "skip the tests" | TEST does NOT fire |
| "no docs needed" | DOCUMENT does NOT fire |
| "skip documentation" | DOCUMENT does NOT fire |
| "no security needed" | SECURITY does NOT fire |
| "forget the architecture" | ARCHITECTURE does NOT fire |

---

## B32-0C-043-005 — negation + multiple positives: only positives fire

**Knowledge edge boundary: mixed negations and positives**

"Expedient — build it, skip tests, skip docs."

Analysis:
- "expedient" → gap-vibe-coding-style-001
- "build" → IMPLEMENT fires ✓
- "skip tests" → TEST negated
- "skip docs" → DOCUMENT negated

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

---
