# Layer 2 — Capability Gap Extended Pairs (028)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-028-001 — gap + SECURITY: extended analysis

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "Quick-win security — secure the endpoints."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["SECURITY"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
Note: SECURITY fires from "secure" verb; gap framing does not suppress it
```

---

## B32-gap-028-002 — "iterative" + IMPLEMENT + TEST: multi-predicate with gap

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "Iterative sprint — implement the module and test the output."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["IMPLEMENT", "TEST"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
```

---

## B32-gap-028-003 — "iterative" + dashboard: two gap IDs, one IMPLEMENT

**CapabilityGapType:** style_qualifier_with_predicates (multi-gap)

**Request:** "Iterative sprint — implement the dashboard."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing + dashboard_scope
RegisteredGapIds: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]
ActivePredicates: ["IMPLEMENT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
Note: "dashboard" is never UI lexicon; always gap-vibe-coding-dashboard-001
```

---

## B32-gap-028-004 — "iterative" vocabulary summary

**Teaching:** "iterative" maps to gap-vibe-coding-style-001.

It is a process-style qualifier. No predicate is embedded in "iterative" itself.

Co-occurrence rule is identical to all other Round 3 qualifiers: action verbs fire their predicates; gap warning attaches.

---
