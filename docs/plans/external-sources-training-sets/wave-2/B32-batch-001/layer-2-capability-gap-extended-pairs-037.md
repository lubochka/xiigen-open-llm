# Layer 2 — Capability Gap Extended Pairs (037)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-037-001 — gap + all predicates negated: extended analysis

**CapabilityGapType:** style_qualifier_solo (all predicates negated)

**Request:** "Lean sprint — skip tests, skip docs, skip architecture, skip security."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
Note: All predicates negated; gap vocabulary still present → knowledge_edge + SCL
```

---

## B32-gap-037-002 — no gap vocabulary + negated TEST + IMPLEMENT: no gap type

**No gap vocabulary case:**

**Request:** "Build the service, no tests needed."

```
CapabilityGapType: (none; no gap vocabulary)
GapCategory: (none)
RegisteredGapIds: []
ActivePredicates: ["IMPLEMENT"]
DomainId: "software-development"
LearningSignal: (none; no gap vocabulary → no SCL, no gap warning)
```

---

## B32-gap-037-003 — gap + IMPLEMENT + multiple negations: gap with single predicate

**CapabilityGapType:** style_qualifier_with_predicates (despite multiple negations)

**Request:** "Expedient — implement the module, skip tests, skip docs, skip architecture."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["IMPLEMENT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
NegationNote: TEST, DOCUMENT, ARCHITECTURE all negated; IMPLEMENT is the only active predicate
```

---

## B32-gap-037-004 — Negation + gap: summary of effects

| Component | Negation present | Negation absent |
|---|---|---|
| Predicate | Absent if negated | Present if verb appears |
| GapWarnings | Unchanged by negation | Set by gap vocabulary |
| SCL | Emits if predicates = [] AND gap vocab present | Normal rules |
| GapWarnings from negation | NEVER | — |

---
