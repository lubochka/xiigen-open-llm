# Cross-Layer Synthesis (007)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

---

## B32-CLS-007-001 — "Build something quick and dirty" → IMPLEMENT fires + gap warning

**Request:** "Build something quick and dirty for the shopping cart."

### Full Layer Trace

---

### Layer 0-A: Compiler

**Input:** "Build something quick and dirty for the shopping cart."

```
Token scan:
"build" → IMPLEMENT fires ✓
"quick and dirty" → gap-vibe-coding-quick-dirty-001 (gap warning)
"shopping cart" → domain noun

Action verb scan:
- build: PRESENT → IMPLEMENT fires ✓

Gap vocabulary scan:
- "quick and dirty" → gap-vibe-coding-quick-dirty-001

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-quick-dirty-001]
```

---

### Layer 0-B: Router

```
Input: Predicates = ["IMPLEMENT"]
Decision: IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-quick-dirty-001]
CapabilityGapWarning: style_qualifier_noted
```

---

### Layer 0-D: Flow Factory

```
Input: Predicates = ["IMPLEMENT"]
ImplementationFactory invoked → ImplementationFlow created
GapMetadata: {gapWarnings: [gap-vibe-coding-quick-dirty-001], capabilityGapWarning: style_qualifier_noted}
```

---

### Layer 1: Transition Orchestrator

```
TransitionType: feature_addition
SessionState: active
DomainId: "software-development"
GapIds: [gap-vibe-coding-quick-dirty-001]
ContributionFlag: true
CapabilityGapWarning: style_qualifier_noted
```

---

### Layer 2: Capability Gap

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: low_quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-quick-dirty-001]
ActivePredicates: ["IMPLEMENT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
```

---

### Final Result Summary

```
Request: "Build something quick and dirty for the shopping cart."
Predicates: ["IMPLEMENT"]
Route: ImplementationFlow
DomainId: software-development
CapabilityAction: none
TransitionType: feature_addition
GapWarning: gap-vibe-coding-quick-dirty-001 (style_qualifier_noted)
Contribution: true
```

**Key teaching:** "quick and dirty" is gap vocabulary (style qualifier). "build" fires IMPLEMENT unconditionally. The style qualifier is a warning annotation only — it does not suppress the predicate and does not change the route.

---
