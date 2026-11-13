# Layer 2 — Capability Gap Extended Pairs (033)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-033-001 — negation + gap vocabulary: extended analysis

**CapabilityGapType:** style_qualifier_solo (negation does not produce predicates)

**Request:** "Good enough — no tests needed."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
NegationNote: "no tests needed" negates TEST; TEST correctly absent
```

---

## B32-gap-033-002 — gap + IMPLEMENT + negated TEST: extended analysis

**CapabilityGapType:** style_qualifier_with_predicates (negation present but IMPLEMENT fires)

**Request:** "Lean sprint — implement feature, don't bother with tests."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["IMPLEMENT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
NegationNote: "don't bother with tests" negates TEST; TEST correctly absent
```

---

## B32-gap-033-003 — "no tests needed" alone: no gap vocabulary → different profile

**No gap vocabulary present:**

```
CapabilityGapType: (none; no gap vocabulary)
GapCategory: (none)
RegisteredGapIds: []
ActivePredicates: []
DomainId: "software-development"
LearningSignal: (none; SCL only emits when gap vocabulary present)
```

---

## B32-gap-033-004 — negation pattern taxonomy for B32

| Negation expression | Predicate suppressed | Effect |
|---|---|---|
| "no tests needed" | TEST | Not negated predicate → correct absence |
| "skip the tests" | TEST | Correct absence |
| "don't bother with docs" | DOCUMENT | Correct absence |
| "skip documentation" | DOCUMENT | Correct absence |
| "no QA required" | QA | Correct absence |
| "don't secure it" | SECURITY | Correct absence |
| "skip the architecture" | ARCHITECTURE | Correct absence |

---
