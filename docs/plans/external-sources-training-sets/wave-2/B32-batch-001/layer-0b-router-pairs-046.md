# Layer 0-B — Router Pairs (046)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-046-001 — gap + IMPLEMENT + negated DOCUMENT: routing

**Request:** "Lean sprint — implement endpoint, don't bother documenting."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

DocumentFactory NOT invoked.

---

## B32-0B-046-002 — "just get it working" alone: knowledge_edge routing

**Request:** "Just get it working."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once)
DomainId: "software-development"
```

---

## B32-0B-046-003 — "just get it working" + IMPLEMENT: routing

**Request:** "Just get it working — implement the login flow."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-046-004 — "clean up later" + IMPLEMENT: routing

**Request:** "Clean up later — just implement the core now."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
