# Layer 0-B — Router Pairs (038)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-038-001 — "barebones" routing: zero-predicate path

**Request:** "Keep it barebones."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once)
DomainId: "software-development"
```

---

## B32-0B-038-002 — "barebones" + IMPLEMENT routing

**Request:** "Barebones implementation — build the upload handler."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-038-003 — "stripped-down" + IMPLEMENT + UI routing

**Request:** "Stripped-down UI — build the settings screen."

```
Input: Predicates = ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
UI → UIFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-038-004 — "barebones" + "stripped-down" combined: multi-gap routing

**Request:** "Barebones stripped-down prototype."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once despite two gap IDs)
DomainId: "software-development"
```

---
