# Layer 0-C — Knowledge-Edge Pairs (051)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-051-001 — qualifier + three predicates: not knowledge edge

**Knowledge edge boundary: three predicates with gap qualifier**

"Lean sprint — implement, write tests, document the API."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires ✓
- "write tests" → TEST fires ✓
- "document" → DOCUMENT fires ✓

Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"]
Not knowledge_edge.

---

## B32-0C-051-002 — multi-predicate TransitionType check

**Knowledge edge: TransitionType with IMPLEMENT present in multi-predicate set**

Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"]
TransitionType: feature_addition (IMPLEMENT present)

Predicates: ["TEST", "DOCUMENT"]
TransitionType: (mixed; no IMPLEMENT) → documentation (DOCUMENT present; DOCUMENT > TEST for type)

Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition (IMPLEMENT present)

---

## B32-0C-051-003 — all eight predicates: not knowledge edge

**Knowledge edge: all eight predicates fired**

"Expedient sprint — implement, build UI, architect, define contract, secure, test, document, QA."

All eight predicates fire → eight factories invoked → not knowledge_edge.
TransitionType: feature_addition (IMPLEMENT and UI present).

---

## B32-0C-051-004 — multi-predicate with negations: reduced subset fires

**Knowledge edge: negations reduce predicate set**

"Lean — implement, test, skip docs, skip architecture."

Analysis:
- IMPLEMENT fires ✓
- TEST fires ✓
- DOCUMENT negated → absent
- ARCHITECTURE negated → absent

Predicates: ["IMPLEMENT", "TEST"]
Not knowledge_edge.

---

## B32-0C-051-005 — mixed TEST + DOCUMENT (no IMPLEMENT): TransitionType

**Knowledge edge: TEST + DOCUMENT without IMPLEMENT**

"Lean — write tests and document the API."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "write tests" → TEST fires ✓
- "document the API" → DOCUMENT fires ✓

Predicates: ["TEST", "DOCUMENT"]
TransitionType: documentation (DOCUMENT present; IMPLEMENT absent; documentation > no_contribution)

---
