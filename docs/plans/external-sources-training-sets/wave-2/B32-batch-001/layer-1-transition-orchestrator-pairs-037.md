# Layer 1 — Transition Orchestrator Pairs (037)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-037-001 — gap + all predicates negated: knowledge_edge orchestrator

**Request:** "Lean sprint — skip tests, skip docs, skip architecture, skip security."

```
Input: Predicates = []
TransitionType: unknown
SessionState: knowledge_edge
ContributionFlag: false
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
```

---

## B32-L1-037-002 — gap + IMPLEMENT + all other negated: feature_addition

**Request:** "Lean sprint — build the feature, skip tests, skip docs."

```
Input: Predicates = ["IMPLEMENT"]
TransitionType: feature_addition
SessionState: active
ContributionFlag: true
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-L1-037-003 — no gap vocabulary + IMPLEMENT + negated TEST: feature_addition, no gap warning

**Request:** "Build the service, no tests needed."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: []
TransitionType: feature_addition
SessionState: active
ContributionFlag: true
DomainId: "software-development"
CapabilityGapWarning: (absent; no gap vocabulary)
```

No gap vocabulary → CapabilityGapWarning absent from output.

---

## B32-L1-037-004 — gap + IMPLEMENT + negated ARCHITECTURE

**Request:** "Pragmatic — implement, don't bother with architecture."

```
Input: Predicates = ["IMPLEMENT"]
TransitionType: feature_addition
SessionState: active
ContributionFlag: true
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---
