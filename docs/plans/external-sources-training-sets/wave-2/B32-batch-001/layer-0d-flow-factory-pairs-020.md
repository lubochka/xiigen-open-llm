# Layer 0-D — Flow Factory Pairs (020)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-020-001 — Gap + IMPLEMENT mixed: ImplementationFactory selected despite gap framing

**Predicates:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-just-works-001, gap-vibe-coding-nothing-fancy-001

**Source request:** "Just make it work — build the checkout flow, nothing fancy."

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory → ImplementationFlow
- Multiple gap warnings logged but do not affect factory selection

**Output:**
```
FlowFactory: ImplementationFactory
FactorySelected: ImplementationFlow
RouteTarget: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-just-works-001, gap-vibe-coding-nothing-fancy-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0D-020-002 — Gap + IMPLEMENT: "quick and dirty + implement" → ImplementationFactory

**Predicates:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-quick-dirty-001

**Output:**
```
FlowFactory: ImplementationFactory
FactorySelected: ImplementationFlow
RouteTarget: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-quick-dirty-001]
```

---

## B32-0D-020-003 — Gap + IMPLEMENT: "vibe create" → ImplementationFactory

**Predicates:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-style-001

**Output:**
```
FlowFactory: ImplementationFactory
FactorySelected: ImplementationFlow
RouteTarget: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0D-020-004 — Gap + IMPLEMENT + TEST: two factories selected

**Predicates:** `["IMPLEMENT", "TEST"]`

**Gap context:** gap-vibe-coding-style-001

**Source:** "Scrappy build — implement the feature and test it quick."

**Output:**
```
FlowFactory: [ImplementationFactory, TestFactory]
FactorySelected: [ImplementationFlow, TestFlow]
RouteTarget: [ImplementationFlow, TestFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---
