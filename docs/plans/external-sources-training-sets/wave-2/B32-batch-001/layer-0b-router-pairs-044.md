# Layer 0-B — Router Pairs (044)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-044-001 — gap + IMPLEMENT + SECURITY routing

**Request:** "Lean sprint — implement auth flow and secure tokens."

```
Input: Predicates = ["IMPLEMENT", "SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
IMPLEMENT → ImplementationFlow
SECURITY → SecurityFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-044-002 — gap + SECURITY only routing

**Request:** "Pragmatic — just secure the database connection."

```
Input: Predicates = ["SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
SECURITY → SecurityFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-044-003 — "hack together" gap + SECURITY routing

**Request:** "Hack together a security fix — secure the login form."

```
Input: Predicates = ["SECURITY"]
GapWarnings: [gap-vibe-coding-hack-001]
SECURITY → SecurityFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

Gap ID is gap-vibe-coding-hack-001 (from "hack together"), not gap-vibe-coding-style-001.

---

## B32-0B-044-004 — "think about security": knowledge_edge routing

**Request:** "Lean approach — think about security."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once)
DomainId: "software-development"
```

---
