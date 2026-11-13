# Layer 0-D — Flow Factory Pairs (006)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-006-001 — UI+IMPLEMENT factory: both flows selected in vibe context

**Predicates:** `["UI", "IMPLEMENT"]`

**Source request:** "Vibe code the user profile screen and implement the avatar upload component."

**Gap warnings:** gap-vibe-coding-style-001

**Factory selection:**

```
Factories selected: [UIFlow, BackendServiceFlow41]
Reason for UIFlow: "screen" and "component" both in UI lexicon
Reason for BackendServiceFlow41: "code" root match (from "vibe code") + "implement"
GapMetadata: {vibe-style: true}
ParallelEligible: true
TransitionType: feature_addition
```

---

## B32-0D-006-002 — UI+IMPLEMENT factory: another vibe example

**Predicates:** `["UI", "IMPLEMENT"]`

**Source request:** "In creative mode — build a sleek modal component for the notifications."

**Gap warnings:** gap-vibe-coding-style-001

**Factory selection:**

```
Factories selected: [UIFlow, BackendServiceFlow41]
Reason for UIFlow: "modal" and "component" both in UI lexicon
Reason for BackendServiceFlow41: "build" fires IMPLEMENT
GapMetadata: {creative-mode: true}
ParallelEligible: true
TransitionType: feature_addition
```

---

## B32-0D-006-003 — UI+IMPLEMENT: factory instantiation pattern

```
session.createFlows([
  {
    factory: UIFlow,
    predicateTrigger: "UI",
    gapMetadata: {"gap-vibe-coding-style-001": true},
    vibeStyleIndicator: true
  },
  {
    factory: BackendServiceFlow41,
    predicateTrigger: "IMPLEMENT",
    gapMetadata: {"gap-vibe-coding-style-001": true},
    vibeStyleIndicator: true
  }
])
```

Both factories receive identical gap metadata. Both are created at the same time. Execution order: parallel (no dependency between UI and backend service).

---

## B32-0D-006-004 — Factory independence: UI and IMPLEMENT do not wait for each other

**When UI and IMPLEMENT are parallel-eligible:**
- UIFlow produces UI artifacts (screens, components, layouts)
- BackendServiceFlow41 produces backend service code
- These are independent deliverables
- Neither blocks the other
- Gap metadata is informational to both

This is the standard parallel factory creation pattern for UI+IMPLEMENT in B32.

---
