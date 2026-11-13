# Layer 0-D — Flow Factory Pairs (017)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-017-001 — "low-fi" alone → factory bypassed

**Predicates:** `[]`

**Gap warnings:** gap-vibe-coding-style-001 ("low-fi")

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

## B32-0D-017-002 — "low-fi + prototype" → factory bypassed (both gap)

**Predicates:** `[]`

**Gap warnings:** gap-vibe-coding-style-001, gap-vibe-coding-prototype-001

**Flow factory decision:**

- Predicates empty → factory bypassed

**Output:**
```
FlowFactory: none
FactorySelected: none
RouteTarget: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]
```

---

## B32-0D-017-003 — "low-fi build + page" → ImplementationFactory + UIFactory

**Predicates:** `["IMPLEMENT", "UI"]`

**Gap warnings:** gap-vibe-coding-style-001

**Flow factory decision:**

- IMPLEMENT → ImplementationFactory
- UI → UIFactory

**Output:**
```
FlowFactory: [ImplementationFactory, UIFactory]
FactorySelected: [ImplementationFlow, UIFlow]
RouteTarget: [ImplementationFlow, UIFlow]
DomainId: "software-development"
GapWarnings: [gap-vibe-coding-style-001]
```

---

## B32-0D-017-004 — "lo-fi" variant processed identically to "low-fi" by factory

**Rule:** The factory receives the compiled predicates, not the raw text. By the time the factory is invoked, "lo-fi" and "low-fi" have both been translated to gap-vibe-coding-style-001 by the compiler. The factory is not aware of which variant triggered the gap.

**Factory receives:**
```
{
  predicates: [...],
  gapWarnings: ["gap-vibe-coding-style-001"],
  rawText: (not used by factory)
}
```

---
