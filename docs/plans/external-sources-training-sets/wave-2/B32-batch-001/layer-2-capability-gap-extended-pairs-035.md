# Layer 2 — Capability Gap Extended Pairs (035)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-035-001 — "whip up" hack gap vocabulary: extended analysis

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "Whip up a quick implementation of the search."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: hack_approach
RegisteredGapIds: [gap-vibe-coding-hack-001]
ActivePredicates: ["IMPLEMENT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
Note: "whip up" → hack-001 (not style-001); gap ID accuracy required
```

---

## B32-gap-035-002 — "throw together" + dashboard + screen: multi-gap

**CapabilityGapType:** style_qualifier_with_predicates (multi-gap)

**Request:** "Throw together a UI for the dashboard settings screen."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: hack_approach + dashboard_scope
RegisteredGapIds: [gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001]
ActivePredicates: ["UI"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
```

---

## B32-gap-035-003 — "wing it" alone: solo gap

**CapabilityGapType:** style_qualifier_solo

**Request:** "Just wing it."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
```

---

## B32-gap-035-004 — Informal IMPLEMENT phrase forms: "knock out", "bang out", "whip up"

| Informal phrase | Gap ID | Predicate effect |
|---|---|---|
| "whip up" + "implementation" | gap-vibe-coding-hack-001 | IMPLEMENT fires |
| "bang out" + "tests" | gap-vibe-coding-hack-001 | TEST fires |
| "knock out" + "implementation" | gap-vibe-coding-hack-001 | IMPLEMENT fires |
| "throw together" + "screen" | gap-vibe-coding-hack-001 | UI fires |
| "wing it" | gap-vibe-coding-style-001 | no predicate |

---
