# Layer 0-D — Flow Factory Pairs (015)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-015-001 — "quick win" alone → factory bypassed

**Predicates:** `[]`

**Gap warnings:** gap-vibe-coding-style-001 ("quick win")

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

## B32-0D-015-002 — "quick win build" → ImplementationFactory + gap warning logged

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
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0D-015-003 — "quick win document" → DocumentFactory

**Predicates:** `["DOCUMENT"]`

**Gap warnings:** gap-vibe-coding-style-001

**Flow factory decision:**

- DOCUMENT → DocumentFactory → DocumentFlow

**Output:**
```
FlowFactory: DocumentFactory
FactorySelected: DocumentFlow
RouteTarget: DocumentFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0D-015-004 — "quick win test" → TestFactory

**Predicates:** `["TEST"]`

**Gap warnings:** gap-vibe-coding-style-001

**Flow factory decision:**

- TEST → TestFactory → TestFlow

**Output:**
```
FlowFactory: TestFactory
FactorySelected: TestFlow
RouteTarget: TestFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---
