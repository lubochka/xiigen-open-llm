# Layer 0-B — Router Pairs (042)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-042-001 — gap + ARCHITECTURE routing

**Request:** "No-frills architecture — architect the service boundary."

```
Input: Predicates = ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-style-001]
ARCHITECTURE → ArchitectureFlow31
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-042-002 — gap + ARCHITECTURE + IMPLEMENT routing

**Request:** "Iterative design — architect the modules and implement the core."

```
Input: Predicates = ["ARCHITECTURE", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
ARCHITECTURE → ArchitectureFlow31
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-042-003 — "lean architecture" noun only: knowledge_edge routing

**Request:** "We need a lean architecture."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once)
DomainId: "software-development"
```

---

## B32-0B-042-004 — mixed multi-qualifier, no predicates: knowledge_edge routing

**Request:** "Lean, agile, iterative approach."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once; three qualifiers but same ID)
DomainId: "software-development"
```

---
