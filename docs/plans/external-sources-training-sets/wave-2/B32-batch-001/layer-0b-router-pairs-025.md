# Layer 0-B — Router Pairs (025)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-025-001 — "good enough" alone → routes to knowledge_edge

**Input predicates:** []
**GapWarnings:** [gap-vibe-coding-style-001]

```
Router decision:
Predicates = [] → knowledge_edge (bypass all flows)
SCHEDULE_CAPABILITY_LEARNING: emitted once (DomainId: software-development)
No factory invoked
```

---

## B32-0B-025-002 — "good enough" + IMPLEMENT → routes to ImplementationFlow

**Input predicates:** ["IMPLEMENT"]
**GapWarnings:** [gap-vibe-coding-style-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
GapMetadata: {gapId: gap-vibe-coding-style-001, type: quality_bar_framing}
DomainId: software-development
```

---

## B32-0B-025-003 — "minimum viable" alone → routes to knowledge_edge

**Input predicates:** []
**GapWarnings:** [gap-vibe-coding-style-001]

```
Router decision:
Predicates = [] → knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (DomainId: software-development)
Note: "minimum viable" and "good enough" both produce gap-vibe-coding-style-001
Same routing outcome for any zero-predicate gap-vibe-coding-style-001 case
```

---

## B32-0B-025-004 — "minimum viable" + IMPLEMENT + TEST → multi-route

**Input predicates:** ["IMPLEMENT", "TEST"]
**GapWarnings:** [gap-vibe-coding-style-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
TEST → TestFactory → TestFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-025-005 — "good enough" + DOCUMENT → routes to DocumentFlow

**Input predicates:** ["DOCUMENT"]
**GapWarnings:** [gap-vibe-coding-style-001]

```
Router decision:
DOCUMENT → DocumentFactory → DocumentFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---
