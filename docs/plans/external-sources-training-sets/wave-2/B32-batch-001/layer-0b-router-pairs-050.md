# Layer 0-B — Router Pairs (050)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-050-001 — "hack it together" + IMPLEMENT: routing

**Request:** "Hack it together — implement the report exporter."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-050-002 — "hack together" + IMPLEMENT + TEST: routing

**Request:** "Hack together the auth module and write some tests."

```
Input: Predicates = ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-hack-001]
IMPLEMENT → ImplementationFlow
TEST → TestFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-050-003 — "let's hack something together" alone: knowledge_edge

**Request:** "Let's hack something together."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-hack-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once)
DomainId: "software-development"
```

---

## B32-0B-050-004 — "cobble together" + IMPLEMENT: routing

**Request:** "Cobble something together — build the notification sender."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
