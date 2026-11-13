# Layer 0-D — Flow Factory Pairs (018)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-018-001 — "throwaway" alone → factory bypassed

**Predicates:** `[]`

**Gap warnings:** gap-vibe-coding-throwaway-001

**Flow factory decision:**

- Predicates empty → factory bypassed

**Output:**
```
FlowFactory: none
FactorySelected: none
RouteTarget: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-throwaway-001]
```

---

## B32-0D-018-002 — "throwaway + interface build" → ImplementationFactory + UIFactory

**Predicates:** `["IMPLEMENT", "UI"]`

**Gap warnings:** gap-vibe-coding-throwaway-001

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory
- UI → UIFactory (from "interface")

**Output:**
```
FlowFactory: [ImplementationFactory, UIFactory]
FactorySelected: [ImplementationFlow, UIFlow]
RouteTarget: [ImplementationFlow, UIFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-throwaway-001]
```

---

## B32-0D-018-003 — "throwaway + triple gap" → all gap; factory bypassed

**Predicates:** `[]`

**Gap warnings:** gap-vibe-coding-throwaway-001, gap-vibe-coding-prototype-001, gap-vibe-coding-demo-001

**Flow factory decision:**

- Predicates empty → factory bypassed
- Three gap IDs; one SCHEDULE_CAPABILITY_LEARNING

**Output:**
```
FlowFactory: none
FactorySelected: none
RouteTarget: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-throwaway-001, gap-vibe-coding-prototype-001, gap-vibe-coding-demo-001]
```

---

## B32-0D-018-004 — "throwaway write" → ImplementationFactory

**Predicates:** `["IMPLEMENT"]`

**Gap warnings:** gap-vibe-coding-throwaway-001

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory

**Output:**
```
FlowFactory: ImplementationFactory
FactorySelected: ImplementationFlow
RouteTarget: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-throwaway-001]
```

---
