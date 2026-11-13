# Layer 0-B — Router Pairs (027)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-027-001 — "spike" alone → knowledge_edge

**Input predicates:** []
**GapWarnings:** [gap-vibe-coding-spike-001]

```
Router decision:
Predicates = [] → knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (DomainId: software-development)
GapMetadata: {gapId: gap-vibe-coding-spike-001, type: investigation_framing}
```

---

## B32-0B-027-002 — "technical spike" alone → same routing

**Input predicates:** []
**GapWarnings:** [gap-vibe-coding-spike-001]

```
Router decision:
Predicates = [] → knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted
Note: All spike-family terms (spike, technical spike, run a spike, tracer bullet, walking skeleton, thin slice) produce identical routing when no action verb present
```

---

## B32-0B-027-003 — "spike" + IMPLEMENT → ImplementationFlow

**Input predicates:** ["IMPLEMENT"]
**GapWarnings:** [gap-vibe-coding-spike-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-027-004 — "walking skeleton" + IMPLEMENT + TEST

**Input predicates:** ["IMPLEMENT", "TEST"]
**GapWarnings:** [gap-vibe-coding-spike-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
TEST → TestFactory → TestFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-027-005 — "thin slice" + IMPLEMENT + DOCUMENT

**Input predicates:** ["IMPLEMENT", "DOCUMENT"]
**GapWarnings:** [gap-vibe-coding-spike-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
DOCUMENT → DocumentFactory → DocumentFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---
