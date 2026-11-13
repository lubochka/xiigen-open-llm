# Layer 0-C — Knowledge-Edge Pairs (047)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-047-001 — "no tests needed" alone (no gap vocab): pure negation, no SCL

**Knowledge edge: negation without gap vocabulary**

"No tests needed — just ship it."

Analysis:
- "no tests needed" → TEST negated
- No gap vocabulary
- No SCL (no gap vocabulary present)

Predicates: []
GapWarnings: []
(No SCL; no gap vocabulary)

---

## B32-0C-047-002 — gap + all predicates negated: knowledge edge

**Knowledge edge: gap vocabulary but all predicates negated**

"Lean sprint — skip tests, skip docs, skip architecture, skip security."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "skip tests" → TEST negated
- "skip docs" → DOCUMENT negated
- "skip architecture" → ARCHITECTURE negated
- "skip security" → SECURITY negated
- No positive action verb present

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-047-003 — gap + IMPLEMENT + all other predicates negated: only IMPLEMENT

**Knowledge edge boundary:**

"Lean sprint — build the feature, skip tests, skip docs."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "build" → IMPLEMENT fires ✓
- "skip tests" → TEST negated
- "skip docs" → DOCUMENT negated

Predicates: ["IMPLEMENT"]
Not knowledge_edge.

---

## B32-0C-047-004 — "just build it" as IMPLEMENT trigger

**Knowledge edge boundary: "just build" fires IMPLEMENT**

"Lean — just build the handler."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "just" → intensifier; no predicate effect
- "build" → IMPLEMENT fires ✓

Predicates: ["IMPLEMENT"]
Not knowledge_edge.

---

## B32-0C-047-005 — DomainId invariant check: knowledge_edge path

**Knowledge edge: DomainId always "software-development" even in empty predicate path**

```
Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
DomainId: "software-development"  ← invariant; always present
```

---
