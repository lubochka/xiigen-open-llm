# Layer 0-D — Flow Factory Pairs (005)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-005-001 — ARCHITECTURE factory: ArchitectureFlow31 in rough-cut request

**Predicates:** `["ARCHITECTURE"]`

**Source request:** "Design a rough-cut architecture for the feature flag system — nothing fancy."

**Gap warnings:** gap-vibe-coding-rough-cut-001, gap-vibe-coding-nothing-fancy-001

**Flow factory decision:**

Step 1 — ARCHITECTURE predicate detected → invoke ArchitectureFlow31 factory
Step 2 — factory receives gap metadata
Step 3 — ArchitectureFlow31 instance created

**Output:**
```
FlowFactory: ArchitectureFlow31
FlowInstance: created
GapMetadata: {rough-cut: true, nothing-fancy: true}
TransitionType: documentation
DomainId: "software-development"
```

---

## B32-0D-005-002 — ArchitectureFlow31: same factory regardless of vibe framing

| ARCHITECTURE request type | Factory |
|---|---|
| "design the architecture" | ArchitectureFlow31 |
| "rough-cut architecture design" | ArchitectureFlow31 |
| "nothing fancy — architect the system" | ArchitectureFlow31 |
| "spike the design" | ArchitectureFlow31 |
| "vibe design the architecture" | ArchitectureFlow31 |

---

## B32-0D-005-003 — ARCHITECTURE + IMPLEMENT: ArchitectureFlow31 + BackendServiceFlow41

**Predicates:** `["ARCHITECTURE", "IMPLEMENT"]`

**Source request:** "Design and build the feature flag system — rough prototype."

**Gap warnings:** gap-vibe-coding-rough-cut-001, gap-vibe-coding-prototype-001

**Output:**
```
FlowFactory: [ArchitectureFlow31, BackendServiceFlow41]
FlowInstances: [ArchitectureFlow31Instance, BackendServiceFlow41Instance]
ParallelEligible: conditional (IMPLEMENT may depend on ARCHITECTURE output)
TransitionType: feature_addition
DomainId: "software-development"
```

**Ordering note:** ArchitectureFlow31 may need to produce output before BackendServiceFlow41 begins. Whether this is strict sequential or loose dependency depends on the specific request.

---

## B32-0D-005-004 — TransitionType for ARCHITECTURE-only factory

**Rule:** When only ARCHITECTURE fires (no IMPLEMENT):
- TransitionType: documentation
- The architecture output is documentation, not code production

When ARCHITECTURE + IMPLEMENT both fire:
- TransitionType: feature_addition (IMPLEMENT dominates)
- Architecture is produced as part of the feature addition

---
