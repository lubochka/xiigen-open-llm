# Layer 0-B — Router Pairs (040)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-040-001 — "iterative" routing: zero-predicate path

**Request:** "Let's be iterative about this."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once)
DomainId: "software-development"
```

---

## B32-0B-040-002 — "iterative" + IMPLEMENT routing

**Request:** "Iterative build — implement the search feature."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-040-003 — gap + SECURITY routing

**Request:** "Lean security — secure the API gateway."

```
Input: Predicates = ["SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
SECURITY → SecurityFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-040-004 — "iterative" + IMPLEMENT + TEST routing

**Request:** "Iterative sprint — implement the dashboard and test the endpoints."

```
Input: Predicates = ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]
IMPLEMENT → ImplementationFlow
TEST → TestFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
