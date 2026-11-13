# Layer 0-C — Knowledge-Edge Pairs (035)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-035-001 — "expedient" alone: knowledge edge

**Knowledge edge: expedient qualifier with no action verb**

"The expedient solution is fine."

Analysis:
- "expedient" → gap-vibe-coding-style-001
- "solution" → noun; not an action verb
- "fine" → approval; not an action verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL (SCHEDULE_CAPABILITY_LEARNING)

---

## B32-0C-035-002 — "pragmatic" alone: knowledge edge

**Knowledge edge: pragmatic qualifier with no action verb**

"Just be pragmatic."

Analysis:
- "pragmatic" → gap-vibe-coding-style-001
- "be" → existential verb; not in action predicate lexicon
- "just" → intensifier; no predicate effect

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-035-003 — "no-frills" alone: knowledge edge

**Knowledge edge: no-frills qualifier with no action verb**

"We want a no-frills result."

Analysis:
- "no-frills" → gap-vibe-coding-style-001 (low-bar quality framing)
- "result" → noun; not an action verb
- "want" → desire verb; not in predicate lexicon

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-035-004 — "expedient" + SECURITY: knowledge edge boundary

**Knowledge edge: expedient with security verb fires SECURITY**

"Expedient security patch — secure the API."

Analysis:
- "expedient" → gap-vibe-coding-style-001
- "secure" → SECURITY fires ✓ (positive imperative)

Predicates: ["SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted

Note: This is NOT knowledge_edge — SECURITY fires. Gap warning is forwarded to flow metadata.

---

## B32-0C-035-005 — "pragmatic" + IMPLEMENT + TEST: knowledge edge boundary

**Knowledge edge: multiple predicates with pragmatic**

"Pragmatic sprint: implement the payment module and test the checkout."

Analysis:
- "pragmatic" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires ✓
- "test" → TEST fires ✓

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"

---
