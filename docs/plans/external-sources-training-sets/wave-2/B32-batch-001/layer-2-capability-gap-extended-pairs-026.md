# Layer 2 — Capability Gap Extended Pairs (026)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-026-001 — "barebones" gap vocabulary: extended analysis

**CapabilityGapType:** style_qualifier_solo

**Request:** "Just barebones for now."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
Explanation: "barebones" signals a minimal-feature acceptance stance
```

---

## B32-gap-026-002 — "stripped-down" gap vocabulary: extended analysis

**CapabilityGapType:** style_qualifier_solo

**Request:** "Stripped-down is the goal."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
```

---

## B32-gap-026-003 — "barebones" + "stripped-down": same gap ID, one SCL

**CapabilityGapType:** style_qualifier_solo (multi-term, same ID)

**Request:** "Barebones stripped-down prototype."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted (once)
Note: "barebones" and "stripped-down" share gap-vibe-coding-style-001; no duplication of SCL
```

---

## B32-gap-026-004 — Round 3 new qualifiers summary (barebones, stripped-down)

| Term | Gap ID | Notes |
|---|---|---|
| barebones | gap-vibe-coding-style-001 | Minimal-feature framing |
| stripped-down | gap-vibe-coding-style-001 | Reduced-feature framing |

Both hyphenated and unhyphenated forms ("bare bones") trigger the same gap ID.

---
