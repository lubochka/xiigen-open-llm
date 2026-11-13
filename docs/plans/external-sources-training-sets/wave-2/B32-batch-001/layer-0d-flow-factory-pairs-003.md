# Layer 0-D — Flow Factory Pairs (003)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-003-001 — IMPLEMENT factory: BackendServiceFlow41 in vibe/exploratory context

**Predicates:** `["IMPLEMENT"]`

**Source request:** "Build something quick and dirty for the shopping cart — just make it work."

**Gap warnings:** gap-vibe-coding-quick-dirty-001, gap-vibe-coding-just-works-001

**Flow factory decision:**

Step 1 — IMPLEMENT predicate detected → invoke BackendServiceFlow41 factory
Step 2 — factory receives gap metadata: quick-dirty, just-works
Step 3 — factory creates flow instance with gap metadata attached
Step 4 — flow proceeds normally; gap metadata is available to flow steps as context

**Output:**
```
FlowFactory: BackendServiceFlow41
FlowInstance: created
GapMetadata: {quick-dirty: true, just-works: true}
TransitionType: feature_addition
DomainId: "software-development"
```

---

## B32-0D-003-002 — BackendServiceFlow41 is selected for IMPLEMENT in all B32 contexts

**Rule:** BackendServiceFlow41 is the registered factory for IMPLEMENT predicates. It is selected regardless of vibe framing. The factory receives gap metadata but uses the same construction logic.

| IMPLEMENT request type | Factory selected |
|---|---|
| "implement a service" (clean) | BackendServiceFlow41 |
| "vibe code a service" | BackendServiceFlow41 |
| "quick and dirty implement" | BackendServiceFlow41 |
| "throwaway build" | BackendServiceFlow41 |
| "just make it work — build it" | BackendServiceFlow41 |
| "exploratory coding — hack the module" | BackendServiceFlow41 |

---

## B32-0D-003-003 — Gap metadata in factory: informational only

**The gap metadata passed to BackendServiceFlow41:**

```
factory.create({
  predicate: "IMPLEMENT",
  gapMetadata: {
    present: true,
    gapIds: ["gap-vibe-coding-quick-dirty-001", "gap-vibe-coding-just-works-001"],
    vibeStyleIndicator: true
  }
})
```

The factory may use this metadata to:
- Set a "prototype mode" flag on the flow
- Adjust output verbosity
- Note in session metadata that results are provisional

But the factory MUST still be created and the flow MUST still proceed. Gap metadata is informational, not directive.

---

## B32-0D-003-004 — Factory construction: vibe framing does not change factory type

**v6-01 corruption pattern for factory layer:**

WRONG:
```
FlowFactory: VibeCodingFlow41  ← invented factory type
```

CORRECT:
```
FlowFactory: BackendServiceFlow41  ← standard factory; gap metadata attached
```

There is no special vibe-coding factory. There is only BackendServiceFlow41 with optional gap metadata.

---
