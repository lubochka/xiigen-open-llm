# Layer 0-D — Flow Factory Pairs (002)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-002-001 — knowledge_edge: no factory selected (zero predicates)

**Predicates:** `[]`

**DomainId:** "software-development"

**Gap warnings:** gap-vibe-coding-spike-001

**Flow factory decision:**

Step 1 — predicates empty → no flow factory can be invoked
Step 2 — knowledge_edge path: bypass all flow factories
Step 3 — emit SCHEDULE_CAPABILITY_LEARNING with DomainId "software-development"

**Output:**
```
FlowFactory: none
FactorySelected: none
RouteTarget: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0D-002-002 — knowledge_edge: all B32 zero-predicate contexts → no factory

**Rule:** The flow factory is only invoked when predicates is non-empty. For any B32 request that produces predicates = [], regardless of the gap vocabulary present:

| Gap vocabulary | Factory selected |
|---|---|
| spike alone | none |
| prototype alone | none |
| dashboard alone | none |
| vibe + hack + throwaway | none |
| any combination of gap vocabulary, no action verbs | none |

---

## B32-0D-002-003 — Factory bypass: DomainId "software-development" in learning path

**When the factory is bypassed (knowledge_edge):**

The learning pipeline receives:
```
{
  event: "capability_gap_detected",
  domainId: "software-development",
  gapIds: [...],
  action: "SCHEDULE_CAPABILITY_LEARNING",
  sourceScenario: "B32"
}
```

This event triggers training data generation for the software-development domain capability expansion. Future training batches may add these gap terms to recognized predicate lexicons.

---

## B32-0D-002-004 — Factory bypass vs factory failure: different outcomes

**Factory bypass (knowledge_edge, no predicates):**
- No factory is called
- No flow is created
- SCHEDULE_CAPABILITY_LEARNING emitted
- This is expected, correct behavior for gap requests

**Factory failure (probationary_rejected, predicates present but factory fails Gate B):**
- Factory was called
- Factory failed internal validation
- Error logged
- This is an unexpected error state, not a gap state

B32 zero-predicate requests always take the factory bypass path, not the factory failure path.

---
