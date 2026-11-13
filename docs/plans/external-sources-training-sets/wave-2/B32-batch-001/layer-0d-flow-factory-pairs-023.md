# Layer 0-D — Flow Factory Pairs (023)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-023-001 — "dashboard build" → ImplementationFactory + gap warning; UIFactory NOT invoked

**Predicates:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-dashboard-001

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory
- UI NOT present → UIFactory NOT invoked

**Output:**
```
FlowFactory: ImplementationFactory
FactorySelected: ImplementationFlow
RouteTarget: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-dashboard-001]
```

**Corruption check:** UIFactory must NOT be invoked when only "dashboard" appears (no UI lexicon term present).

---

## B32-0D-023-002 — "screen build" → ImplementationFactory + UIFactory; no gap warning

**Predicates:** `["IMPLEMENT", "UI"]`

**Gap context:** none

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory
- UI (from "screen") → UIFactory

**Output:**
```
FlowFactory: [ImplementationFactory, UIFactory]
FactorySelected: [ImplementationFlow, UIFlow]
RouteTarget: [ImplementationFlow, UIFlow]
DomainId: "software-development"
GapWarnings: []
```

---

## B32-0D-023-003 — "dashboard screen build" → ImplementationFactory + UIFactory + gap warning

**Predicates:** `["IMPLEMENT", "UI"]`

**Gap context:** gap-vibe-coding-dashboard-001

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory
- UI (from "screen") → UIFactory
- gap-dashboard-001 logged separately

**Output:**
```
FlowFactory: [ImplementationFactory, UIFactory]
FactorySelected: [ImplementationFlow, UIFlow]
RouteTarget: [ImplementationFlow, UIFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-dashboard-001]
```

---

## B32-0D-023-004 — "panel build" → ImplementationFactory only; no gap, no UI factory

**Predicates:** `["IMPLEMENT"]`

**Gap context:** none

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory
- "panel" is not a UI trigger → UIFactory NOT invoked
- "panel" produces no gap ID

**Output:**
```
FlowFactory: ImplementationFactory
FactorySelected: ImplementationFlow
RouteTarget: ImplementationFlow
DomainId: "software-development"
GapWarnings: []
```

---
