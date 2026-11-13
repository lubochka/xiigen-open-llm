# Layer 0-B — Router Pairs (049)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-049-001 — "no tests needed" + IMPLEMENT: routing (no gap vocabulary)

**Request:** "Just build the API, no tests needed."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: []
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (no gap vocabulary)
DomainId: "software-development"
```

No gap vocabulary → no CapabilityGapWarning in metadata.

---

## B32-0B-049-002 — gap + "no tests needed" + IMPLEMENT: routing

**Request:** "Lean sprint — build the API, no tests needed."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-049-003 — gap + negated ARCHITECTURE + IMPLEMENT: routing

**Request:** "Pragmatic — implement, don't bother with architecture."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

ArchitectureFactory NOT invoked.

---

## B32-0B-049-004 — multiple negations + IMPLEMENT: only ImplementationFlow

**Request:** "Expedient — implement, skip tests, skip docs, skip architecture."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

TestFactory, DocumentFactory, ArchitectureFactory all NOT invoked.

---
