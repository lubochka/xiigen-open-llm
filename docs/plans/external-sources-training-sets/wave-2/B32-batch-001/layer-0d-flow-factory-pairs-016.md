# Layer 0-D — Flow Factory Pairs (016)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-016-001 — "hack together" alone → factory bypassed

**Predicates:** `[]`

**Gap warnings:** gap-vibe-coding-hack-001

**Flow factory decision:**

- Predicates empty → factory bypassed

**Output:**
```
FlowFactory: none
FactorySelected: none
RouteTarget: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-hack-001]
```

---

## B32-0D-016-002 — "throw together + prototype + throwaway" → all gap; factory bypassed

**Predicates:** `[]`

**Gap warnings:** gap-vibe-coding-hack-001, gap-vibe-coding-prototype-001, gap-vibe-coding-throwaway-001

**Flow factory decision:**

- Predicates empty regardless of gap count → factory bypassed
- SCHEDULE_CAPABILITY_LEARNING emitted once

**Output:**
```
FlowFactory: none
FactorySelected: none
RouteTarget: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-prototype-001, gap-vibe-coding-throwaway-001]
```

---

## B32-0D-016-003 — "hack together + build" → ImplementationFactory

**Predicates:** `["IMPLEMENT"]`

**Gap warnings:** gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-nothing-fancy-001

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory

**Output:**
```
FlowFactory: ImplementationFactory
FactorySelected: ImplementationFlow
RouteTarget: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-nothing-fancy-001]
```

---

## B32-0D-016-004 — Factory selection is independent of gap warning count

**Rule:** The factory selection algorithm considers only the predicate list. Gap warning count does not affect which factory is selected, nor does it create additional factories.

| Predicates | Gap warnings | Factories selected |
|---|---|---|
| [] | 1 gap ID | none |
| [] | 5 gap IDs | none |
| [IMPLEMENT] | 0 gap IDs | ImplementationFactory |
| [IMPLEMENT] | 3 gap IDs | ImplementationFactory only |

---
