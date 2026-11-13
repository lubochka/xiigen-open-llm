# Layer 0-B — Router Pairs (026)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-026-001 — "proof-of-concept" alone → knowledge_edge

**Input predicates:** []
**GapWarnings:** [gap-vibe-coding-prototype-001]

```
Router decision:
Predicates = [] → knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (DomainId: software-development)
GapMetadata: {gapId: gap-vibe-coding-prototype-001, type: artifact_type_framing}
```

---

## B32-0B-026-002 — "PoC" alone → same as proof-of-concept routing

**Input predicates:** []
**GapWarnings:** [gap-vibe-coding-prototype-001]

```
Router decision:
Predicates = [] → knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (DomainId: software-development)
Note: PoC and proof-of-concept produce identical routing outcomes
```

---

## B32-0B-026-003 — "PoC" + IMPLEMENT → ImplementationFlow + gap warning

**Input predicates:** ["IMPLEMENT"]
**GapWarnings:** [gap-vibe-coding-prototype-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-026-004 — "paper prototype" + UI → UIFlow

**Input predicates:** ["UI"]
**GapWarnings:** [gap-vibe-coding-prototype-001]

```
Router decision:
UI → UIFactory → UIFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-026-005 — "paper prototype" + "good enough" → gap-style-001 + gap-prototype-001, UI fires

**Input predicates:** ["UI"]
**GapWarnings:** [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]

```
Router decision:
UI → UIFactory → UIFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicates non-empty)
CapabilityGapWarning: style_qualifier_noted
Note: Two gap IDs present; router forwards both; factory stores in GapMetadata
DomainId: software-development
```

---
