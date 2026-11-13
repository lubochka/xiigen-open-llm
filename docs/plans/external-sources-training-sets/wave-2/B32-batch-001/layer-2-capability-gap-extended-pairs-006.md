# Layer 2 — Capability Gap Extended Pairs (006)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-L2-006-001 — "quick and dirty build": IMPLEMENT fires; style qualifiers → gap warnings only

**Request:** "Build something quick and dirty for the shopping cart — just make it work."

**Layer 0-A trace:**
```
Token: "build" → IMPLEMENT fires ✓
Token: "quick and dirty" → gap-vibe-coding-quick-dirty-001
Token: "just make it work" → gap-vibe-coding-just-works-001
Token: "shopping cart" → domain noun

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-quick-dirty-001, gap-vibe-coding-just-works-001]
```

**Layer 0-B trace:**
```
Predicates: ["IMPLEMENT"] → BackendServiceFlow41
DomainId: "software-development"
GapMetadata passed through
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
```

**Layer 1 trace:**
```
TransitionType: feature_addition
FlowOrder: [BackendServiceFlow41]
GapMetadata: [quick-dirty-001, just-works-001]
```

**Layer 2 trace:**
```
CapabilityGapType: style_qualifier_present (not a full gap — predicates fired)
GapWarningType: quality_reduction_framing
Notes: User signaling low quality bar and quick completion preference
StyleMetadata: {quickAndDirty: true, minimalViableOutput: true}
LearningSignal: false (predicates fired; no SCHEDULE_CAPABILITY_LEARNING)
```

**Final result:** [IMPLEMENT] → BackendServiceFlow41 + style metadata

---

## B32-L2-006-002 — Key insight: gap warnings without knowledge_edge

**This is the critical B32 pattern:** Gap warnings can be present WITHOUT triggering knowledge_edge.

- knowledge_edge triggers ONLY when predicates = []
- Gap warnings can co-exist with any non-empty predicate set
- The gap warnings are metadata; they do not define the routing outcome

When predicates are non-empty:
- Gap warnings → metadata attached to flow execution
- SCHEDULE_CAPABILITY_LEARNING → NOT emitted
- Session → active, productive (flows running)

---

## B32-L2-006-003 — "quick and dirty" extended gap analysis

**User intent:** The user wants a functional-but-imperfect implementation. Quality, completeness, or elegance are explicitly not priorities.

**System response:** Route to BackendServiceFlow41 (standard IMPLEMENT flow). The gap metadata informs the flow that reduced quality is acceptable, but the flow is still selected and executed.

**Learning pipeline note:** "quick and dirty" is a recognized style qualifier with an assigned gap ID. No new learning action is needed for this specific vocabulary. The learning signal is about improving style vocabulary recognition, not about routing.

---
