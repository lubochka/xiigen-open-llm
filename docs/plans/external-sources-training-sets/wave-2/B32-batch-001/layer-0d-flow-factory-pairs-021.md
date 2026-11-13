# Layer 0-D — Flow Factory Pairs (021)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-021-001 — "vibe code a screen" → ImplementationFactory + UIFactory

**Predicates:** `["IMPLEMENT", "UI"]`

**Gap context:** gap-vibe-coding-style-001

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
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0D-021-002 — "vibe out a screen" → UIFactory only (no "code" root; IMPLEMENT not fired)

**Predicates:** `["UI"]`

**Gap context:** gap-vibe-coding-style-001

**Flow factory decision:**

- UI → UIFactory → UIFlow
- IMPLEMENT absent → ImplementationFactory NOT invoked

**Output:**
```
FlowFactory: UIFactory
FactorySelected: UIFlow
RouteTarget: UIFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0D-021-003 — "vibe code a dashboard" → ImplementationFactory only; UIFactory NOT invoked

**Predicates:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory
- UI NOT present → UIFactory NOT invoked (dashboard did not fire UI)

**Output:**
```
FlowFactory: ImplementationFactory
FactorySelected: ImplementationFlow
RouteTarget: ImplementationFlow
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]
```

---

## B32-0D-021-004 — "vibe coding a modal" → ImplementationFactory + UIFactory

**Predicates:** `["IMPLEMENT", "UI"]`

**Gap context:** gap-vibe-coding-style-001

**Flow factory decision:**

- IMPLEMENT (coding→code root) → ImplementationFactory
- UI (modal → UI lexicon) → UIFactory

**Output:**
```
FlowFactory: [ImplementationFactory, UIFactory]
FactorySelected: [ImplementationFlow, UIFlow]
RouteTarget: [ImplementationFlow, UIFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---
