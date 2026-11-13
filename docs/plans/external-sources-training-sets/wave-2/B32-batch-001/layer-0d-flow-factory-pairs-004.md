# Layer 0-D — Flow Factory Pairs (004)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-004-001 — UI factory: UIFlow in vibe request context

**Predicates:** `["UI"]`

**Source request:** "Vibe out a settings page — nothing fancy."

**Gap warnings:** gap-vibe-coding-style-001, gap-vibe-coding-nothing-fancy-001

**Flow factory decision:**

Step 1 — UI predicate detected → invoke UIFlow factory
Step 2 — factory receives gap metadata: vibe-style, nothing-fancy
Step 3 — UIFlow instance created with gap metadata

**Output:**
```
FlowFactory: UIFlow
FlowInstance: created
GapMetadata: {vibe-style: true, nothing-fancy: true}
TransitionType: feature_addition
DomainId: "software-development"
```

---

## B32-0D-004-002 — UIFlow: same factory regardless of vibe framing

**Rule:** UIFlow is the registered factory for UI predicates. Selected identically for clean and vibe-framed UI requests.

| UI request type | Factory |
|---|---|
| "build a settings screen" | UIFlow |
| "vibe code a settings screen" | UIFlow |
| "rough-cut settings page" | UIFlow |
| "nothing fancy settings form" | UIFlow |
| "hack together a settings component" | UIFlow |

---

## B32-0D-004-003 — UI + IMPLEMENT factory: both factories created

**Predicates:** `["UI", "IMPLEMENT"]`

**Source request:** "Vibe code the user profile screen and implement the avatar upload component."

**Gap warnings:** gap-vibe-coding-style-001

**Output:**
```
FlowFactory: [UIFlow, BackendServiceFlow41]
FlowInstances: [UIFlowInstance, BackendServiceFlow41Instance]
ParallelEligible: true
GapMetadata: {vibe-style: true}
TransitionType: feature_addition
DomainId: "software-development"
```

Both factories are instantiated. They can run in parallel. Gap metadata is attached to both instances.

---

## B32-0D-004-004 — Factory pair creation in parallel-eligible scenarios

**When UI and IMPLEMENT are both present:**

```
session.createFlows([
  {factory: UIFlow, gapMetadata: gapMetadata},
  {factory: BackendServiceFlow41, gapMetadata: gapMetadata}
])
```

Both flows share the same gap metadata context. Parallel execution is permitted because UI work and backend service work are independent.

---
