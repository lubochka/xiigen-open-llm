# Layer 1 — Transition Orchestrator Pairs (042)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-042-001 — DomainId invariant in orchestrator output

**Rule: all orchestrator outputs carry DomainId = "software-development"**

| Scenario | DomainId |
|---|---|
| Active session, any predicates | "software-development" |
| knowledge_edge session | "software-development" |
| Multi-gap, multi-predicate | "software-development" |

---

## B32-L1-042-002 — "expedient spike + implement dashboard": orchestrator output

**Request:** "Expedient spike — implement the dashboard."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-dashboard-001]
TransitionType: feature_addition
SessionState: active
ContributionFlag: true
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
GapIds: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-dashboard-001]
```

---

## B32-L1-042-003 — knowledge_edge with multi-gap: orchestrator output

**Request:** "Expedient spike on the dashboard."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-dashboard-001]
TransitionType: unknown
SessionState: knowledge_edge
ContributionFlag: false
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING (once)
```

Three gap IDs; SCL still emitted exactly once.

---

## B32-L1-042-004 — ContributionFlag summary

**ContributionFlag rules (definitive):**
- true: predicates non-empty (any factory created any flow)
- false: predicates = [] (knowledge_edge)

ContributionFlag is not affected by gap vocabulary count, gap ID type, or DomainId.

---
