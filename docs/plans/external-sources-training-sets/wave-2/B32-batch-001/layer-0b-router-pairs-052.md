# Layer 0-B — Router Pairs (052)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-052-001 — "bang out tests": TEST routing

**Request:** "Bang out some unit tests for the checkout flow."

```
Input: Predicates = ["TEST"]
GapWarnings: [gap-vibe-coding-hack-001]
TEST → TestFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-052-002 — "quick tests": TEST routing with style gap

**Request:** "Quick — write tests for the payment handler."

```
Input: Predicates = ["TEST"]
GapWarnings: [gap-vibe-coding-style-001]
TEST → TestFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-052-003 — "testing would be nice": knowledge_edge routing

**Request:** "Lean approach — some testing would be nice."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once)
DomainId: "software-development"
```

---

## B32-0B-052-004 — "hack together module + bang out tests": IMPLEMENT + TEST routing

**Request:** "Hack together the module and bang out tests."

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
