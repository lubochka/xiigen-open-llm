# Layer 0-D — Flow Factory Pairs (022)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-022-001 — "rough architecture sketch" → ArchitectureFactory

**Predicates:** `["ARCHITECTURE"]`

**Gap context:** none (rough alone produces no gap ID)

**Flow factory decision:**

- ARCHITECTURE → ArchitectureFactory → ArchitectureFlow31

**Output:**
```
FlowFactory: ArchitectureFactory
FactorySelected: ArchitectureFlow31
RouteTarget: ArchitectureFlow31
DomainId: "software-development"
GapWarnings: []
```

---

## B32-0D-022-002 — "scrappy architecture design" → ArchitectureFactory + gap warning

**Predicates:** `["ARCHITECTURE"]`

**Gap context:** gap-vibe-coding-style-001

**Output:**
```
FlowFactory: ArchitectureFactory
FactorySelected: ArchitectureFlow31
RouteTarget: ArchitectureFlow31
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0D-022-003 — "quick and dirty architecture + API" → ArchitectureFactory + APIFactory

**Predicates:** `["ARCHITECTURE", "API_CONTRACT"]`

**Gap context:** gap-vibe-coding-quick-dirty-001

**Flow factory decision:**

- ARCHITECTURE → ArchitectureFactory
- API_CONTRACT → APIContractFactory

**Output:**
```
FlowFactory: [ArchitectureFactory, APIContractFactory]
FactorySelected: [ArchitectureFlow31, APIContractFlow]
RouteTarget: [ArchitectureFlow31, APIContractFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-quick-dirty-001]
```

---

## B32-0D-022-004 — "hack together + design + build" → ArchitectureFactory + ImplementationFactory

**Predicates:** `["ARCHITECTURE", "IMPLEMENT"]`

**Gap context:** gap-vibe-coding-hack-001, gap-vibe-coding-style-001

**Output:**
```
FlowFactory: [ArchitectureFactory, ImplementationFactory]
FactorySelected: [ArchitectureFlow31, ImplementationFlow]
RouteTarget: [ArchitectureFlow31, ImplementationFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-style-001]
```

---
