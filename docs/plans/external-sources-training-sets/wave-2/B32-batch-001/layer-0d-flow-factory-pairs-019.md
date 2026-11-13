# Layer 0-D — Flow Factory Pairs (019)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-019-001 — "fast and loose approach" → factory bypassed

**Predicates:** `[]`

**Gap warnings:** gap-vibe-coding-style-001

**Flow factory decision:**

- Predicates empty → factory bypassed

**Output:**
```
FlowFactory: none
FactorySelected: none
RouteTarget: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0D-019-002 — "fast and loose + implementation" → ImplementationFactory

**Predicates:** `["IMPLEMENT"]`

**Gap warnings:** gap-vibe-coding-style-001

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory

**Output:**
```
FlowFactory: ImplementationFactory
FactorySelected: ImplementationFlow
RouteTarget: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0D-019-003 — "fast and loose + design" → ArchitectureFactory

**Predicates:** `["ARCHITECTURE"]`

**Gap warnings:** gap-vibe-coding-style-001

**Flow factory decision:**

- ARCHITECTURE → ArchitectureFactory

**Output:**
```
FlowFactory: ArchitectureFactory
FactorySelected: ArchitectureFlow31
RouteTarget: ArchitectureFlow31
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0D-019-004 — "fast and loose coding" → ImplementationFactory (coding = code root)

**Predicates:** `["IMPLEMENT"]`

**Gap warnings:** gap-vibe-coding-style-001

**Flow factory decision:**

- IMPLEMENT (from "coding" via "code" root) → ImplementationFactory

**Output:**
```
FlowFactory: ImplementationFactory
FactorySelected: ImplementationFlow
RouteTarget: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---
