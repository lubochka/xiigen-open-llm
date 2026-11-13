# Layer 0-B — Router Pairs (047)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-047-001 — "whip up" + IMPLEMENT: routing

**Request:** "Whip up a quick implementation of the search."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

Gap ID is hack-001 (not style-001) from "whip up".

---

## B32-0B-047-002 — "throw together" + UI: routing with two gap IDs

**Request:** "Throw together a UI for dashboard settings screen."

```
Input: Predicates = ["UI"]
GapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001]
UI → UIFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-047-003 — "bang out" + TEST: routing

**Request:** "Bang out some tests for the payment module."

```
Input: Predicates = ["TEST"]
GapWarnings: [gap-vibe-coding-hack-001]
TEST → TestFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-047-004 — "wing it" alone: knowledge_edge routing

**Request:** "Just wing it."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once)
DomainId: "software-development"
```

---
