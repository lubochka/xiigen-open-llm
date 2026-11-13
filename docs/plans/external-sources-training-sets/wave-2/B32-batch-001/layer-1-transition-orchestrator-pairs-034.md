# Layer 1 — Transition Orchestrator Pairs (034)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-034-001 — "just get it working" alone: knowledge_edge

**Request:** "Just get it working."

```
Input: Predicates = []
TransitionType: unknown
SessionState: knowledge_edge
ContributionFlag: false
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
```

---

## B32-L1-034-002 — "just get it working" + IMPLEMENT: feature_addition

**Request:** "Just get it working — implement the login flow."

```
Input: Predicates = ["IMPLEMENT"]
TransitionType: feature_addition
SessionState: active
ContributionFlag: true
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-L1-034-003 — "clean up later" + IMPLEMENT: feature_addition

**Request:** "Clean up later — implement the core now."

```
Input: Predicates = ["IMPLEMENT"]
TransitionType: feature_addition
SessionState: active
ContributionFlag: true
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-L1-034-004 — gap + IMPLEMENT + negated QA: feature_addition (QA absent)

**Request:** "Pragmatic push — implement feature, skip the QA."

```
Input: Predicates = ["IMPLEMENT"]
TransitionType: feature_addition
SessionState: active
ContributionFlag: true
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

QA correctly absent from predicates.

---
