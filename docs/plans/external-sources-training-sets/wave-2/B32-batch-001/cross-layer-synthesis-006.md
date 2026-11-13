# Cross-Layer Synthesis (006)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

---

## B32-CLS-006-001 — "Vibe code a screen" → UI fires + IMPLEMENT fires + gap warning

**Request:** "Vibe code a screen for the user profile."

### Full Layer Trace

---

### Layer 0-A: Compiler

**Input:** "Vibe code a screen for the user profile."

```
Token scan:
"vibe" → gap-vibe-coding-style-001
"code" → root match "code" → IMPLEMENT fires
"screen" → UI fires (screen is in UI lexicon)
"user profile" → domain noun

Action verb scan:
- code: PRESENT → IMPLEMENT fires ✓

UI vocabulary scan:
- screen: PRESENT → UI fires ✓

Gap vocabulary scan:
- "vibe" → gap-vibe-coding-style-001 (warning logged)

Predicates: ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-style-001]
```

---

### Layer 0-B: Router

```
Input: Predicates = ["IMPLEMENT", "UI"]
Decision: predicates non-empty → active routing
IMPLEMENT → ImplementationFlow
UI → UIFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicates non-empty)
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

---

### Layer 0-D: Flow Factory

```
Input: Predicates = ["IMPLEMENT", "UI"]
ImplementationFactory invoked → ImplementationFlow created
UIFactory invoked → UIFlow created
FlowInstances: [ImplementationFlow, UIFlow]
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
```

---

### Layer 1: Transition Orchestrator

```
Input: [ImplementationFlow, UIFlow] active
TransitionType: feature_addition (IMPLEMENT present)
SessionState: active
DomainId: "software-development"
CapabilityAction: none
GapIds: [gap-vibe-coding-style-001]
ContributionFlag: true
CapabilityGapWarning: style_qualifier_noted
```

---

### Layer 2: Capability Gap

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: vibe_style_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["IMPLEMENT", "UI"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched (not SCHEDULE_CAPABILITY_LEARNING)
Note: predicates fired; gap is annotation only
```

---

### Final Result Summary

```
Request: "Vibe code a screen for the user profile."
Predicates: ["IMPLEMENT", "UI"]
Route: [ImplementationFlow, UIFlow]
DomainId: software-development
CapabilityAction: none
TransitionType: feature_addition
GapWarning: gap-vibe-coding-style-001 (style_qualifier_noted)
Contribution: true
```

**Key teaching:** "vibe code" is NOT pure gap. "code" root fires IMPLEMENT. "screen" fires UI. "vibe" produces gap-style-001 warning. All three signals are independent.

---
