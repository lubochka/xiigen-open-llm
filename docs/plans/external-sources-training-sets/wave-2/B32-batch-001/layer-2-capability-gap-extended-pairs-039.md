# Layer 2 — Capability Gap Extended Pairs (039)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-039-001 — informal + UI: extended analysis

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "Throw together a screen for the user profile."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: hack_approach
RegisteredGapIds: [gap-vibe-coding-hack-001]
ActivePredicates: ["UI"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
```

---

## B32-gap-039-002 — "lean UI — build screen": IMPLEMENT + UI

**CapabilityGapType:** style_qualifier_with_predicates (multi-predicate)

**Request:** "Lean UI — build the settings screen."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["IMPLEMENT", "UI"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
Note: "build [screen]" fires both IMPLEMENT and UI independently
```

---

## B32-gap-039-003 — "lean UI" alone: solo gap

**CapabilityGapType:** style_qualifier_solo

**Request:** "Lean UI."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
```

---

## B32-gap-039-004 — UI predicate trigger reference

| Trigger expression | UI fires? | Notes |
|---|---|---|
| "screen" with build/create/throw together | YES | "screen" → UI |
| "interface" with build/create/whip up | YES | "interface" → UI |
| "UI" with build/create | YES | Direct UI noun with action |
| "UI" alone (no verb) | NO | Noun only |
| "dashboard" with any verb | NO | Always gap-vibe-coding-dashboard-001 |

---
