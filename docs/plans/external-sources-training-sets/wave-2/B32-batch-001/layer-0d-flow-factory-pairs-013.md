# Layer 0-D — Flow Factory Pairs (013)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-013-001 — "scrappy" alone → factory bypassed

**Predicates:** `[]`

**DomainId:** "software-development"

**Gap warnings:** gap-vibe-coding-style-001 ("scrappy")

**Flow factory decision:**

- Predicates empty → factory bypassed
- No flow instantiated
- SCHEDULE_CAPABILITY_LEARNING emitted

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

## B32-0D-013-002 — "scrappy build" → ImplementationFactory invoked

**Predicates:** `["IMPLEMENT"]`

**Gap warnings:** gap-vibe-coding-style-001

**Flow factory decision:**

- IMPLEMENT present → ImplementationFactory invoked
- Factory creates ImplementationFlow instance
- Gap warning does not affect factory selection

**Output:**
```
FlowFactory: ImplementationFactory
FactorySelected: ImplementationFlow
RouteTarget: ImplementationFlow
CapabilityAction: none
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0D-013-003 — "scrappy build + test" → ImplementationFactory + TestFactory

**Predicates:** `["IMPLEMENT", "TEST"]`

**Gap warnings:** gap-vibe-coding-style-001

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory → ImplementationFlow
- TEST → TestFactory → TestFlow
- Both factories invoked in parallel
- Gap warning logged

**Output:**
```
FlowFactory: [ImplementationFactory, TestFactory]
FactorySelected: [ImplementationFlow, TestFlow]
RouteTarget: [ImplementationFlow, TestFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0D-013-004 — Factory receives gap warnings; passes them to flow metadata

**Rule:** The flow factory passes gap warnings into the created flow instance metadata. This allows the flow to annotate its output with CapabilityGapWarning information.

**Factory invocation with gap context:**
```
ImplementationFactory.create({
  predicates: ["IMPLEMENT"],
  domainId: "software-development",
  gapWarnings: ["gap-vibe-coding-style-001"],
  capabilityGapWarning: "style_qualifier_noted"
})
```

---
