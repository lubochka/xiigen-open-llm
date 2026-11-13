# Layer 2 — Capability Gap Extended Pairs (032)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-032-001 — gap + SECURITY: extended analysis

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "Pragmatic — just secure the database connection."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["SECURITY"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
```

---

## B32-gap-032-002 — "hack together" + SECURITY: different gap ID

**CapabilityGapType:** style_qualifier_with_predicates (hack gap variant)

**Request:** "Hack together a security fix — secure the login form."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: hack_approach
RegisteredGapIds: [gap-vibe-coding-hack-001]
ActivePredicates: ["SECURITY"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
Note: gap ID is hack-001 (not style-001); reflects actual vocabulary used
```

---

## B32-gap-032-003 — "think about security": solo gap

**CapabilityGapType:** style_qualifier_solo

**Request:** "Lean approach — think about security."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
Note: "think about" does not fire SECURITY; cognitive phrase not in predicate lexicon
```

---

## B32-gap-032-004 — ARCHITECTURE + SECURITY: TransitionType = documentation

**Gap metadata note:** When ARCHITECTURE and SECURITY are both present (no IMPLEMENT):
- TransitionType = documentation (ARCHITECTURE drives the type)
- SECURITY contributes SecurityFlow but does not override ARCHITECTURE's type

---
