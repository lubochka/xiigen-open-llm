# Cross-Layer Synthesis (008)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

---

## B32-CLS-008-001 — "Rough architecture, build it scrappy, no tests needed" → ARCHITECTURE + IMPLEMENT fires + gap warning

**Request:** "Rough architecture, build it scrappy, no tests needed."

### Full Layer Trace

---

### Layer 0-A: Compiler

**Input:** "Rough architecture, build it scrappy, no tests needed."

```
Token scan:
"rough" → general style adjective; no registered gap ID (not "rough cut")
"architecture" → ARCHITECTURE fires ✓
"build" → IMPLEMENT fires ✓
"scrappy" → gap-vibe-coding-style-001 (gap warning)
"no tests needed" → negation of TEST; TEST does NOT fire

Action verb scan:
- architecture: PRESENT → ARCHITECTURE fires ✓
- build: PRESENT → IMPLEMENT fires ✓
- test: appears in "no tests needed" → NEGATED; does not fire

Gap vocabulary scan:
- "scrappy" → gap-vibe-coding-style-001

Predicates: ["ARCHITECTURE", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
```

---

### Layer 0-B: Router

```
Input: Predicates = ["ARCHITECTURE", "IMPLEMENT"]
ARCHITECTURE → ArchitectureFlow31
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

---

### Layer 0-D: Flow Factory

```
ArchitectureFactory invoked → ArchitectureFlow31 created
ImplementationFactory invoked → ImplementationFlow created
FlowInstances: [ArchitectureFlow31, ImplementationFlow]
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
```

---

### Layer 1: Transition Orchestrator

```
TransitionType: feature_addition (IMPLEMENT present → takes precedence over ARCHITECTURE for transition type)
SessionState: active
DomainId: "software-development"
GapIds: [gap-vibe-coding-style-001]
ContributionFlag: true
CapabilityGapWarning: style_qualifier_noted
```

---

### Layer 2: Capability Gap

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["ARCHITECTURE", "IMPLEMENT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
NegationNote: "no tests needed" negates TEST; TEST correctly absent from predicates
```

---

### Final Result Summary

```
Request: "Rough architecture, build it scrappy, no tests needed."
Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Route: [ArchitectureFlow31, ImplementationFlow]
DomainId: software-development
CapabilityAction: none
TransitionType: feature_addition
GapWarning: gap-vibe-coding-style-001 (style_qualifier_noted)
Contribution: true
```

**Key teachings:**
1. "scrappy" is a gap warning that does not suppress ARCHITECTURE or IMPLEMENT
2. "no tests needed" is a negation — TEST does NOT fire from negated mentions
3. "rough" alone (without "cut") produces no registered gap ID
4. IMPLEMENT takes precedence over ARCHITECTURE for TransitionType when both are present

---
