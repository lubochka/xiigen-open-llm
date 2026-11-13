# Layer 0-D — Flow Factory Pairs (014)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-014-001 — "rough cut" alone → factory bypassed

**Predicates:** `[]`

**Gap warnings:** gap-vibe-coding-rough-cut-001

**Flow factory decision:**

- Predicates empty → factory bypassed
- knowledge_edge path

**Output:**
```
FlowFactory: none
FactorySelected: none
RouteTarget: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-rough-cut-001]
```

---

## B32-0D-014-002 — "rough cut build" → ImplementationFactory

**Predicates:** `["IMPLEMENT"]`

**Gap warnings:** gap-vibe-coding-rough-cut-001

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory → ImplementationFlow

**Output:**
```
FlowFactory: ImplementationFactory
FactorySelected: ImplementationFlow
RouteTarget: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-rough-cut-001]
```

---

## B32-0D-014-003 — "rough cut design" → ArchitectureFactory

**Predicates:** `["ARCHITECTURE"]`

**Gap warnings:** gap-vibe-coding-rough-cut-001

**Flow factory decision:**

- ARCHITECTURE → ArchitectureFactory → ArchitectureFlow31

**Output:**
```
FlowFactory: ArchitectureFactory
FactorySelected: ArchitectureFlow31
RouteTarget: ArchitectureFlow31
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-rough-cut-001]
```

---

## B32-0D-014-004 — "rough cut + screen build" → ImplementationFactory + UIFactory

**Predicates:** `["IMPLEMENT", "UI"]`

**Gap warnings:** gap-vibe-coding-rough-cut-001

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory
- UI → UIFactory
- Both invoked in parallel

**Output:**
```
FlowFactory: [ImplementationFactory, UIFactory]
FactorySelected: [ImplementationFlow, UIFlow]
RouteTarget: [ImplementationFlow, UIFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-rough-cut-001]
```

---
