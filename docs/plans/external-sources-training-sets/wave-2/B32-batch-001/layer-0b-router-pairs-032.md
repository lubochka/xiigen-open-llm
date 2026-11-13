# Layer 0-B — Router Pairs (032)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-032-001 — multiple gap IDs; predicates non-empty; one CapabilityGapWarning

**Input predicates:** ["UI"]
**GapWarnings:** [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]

```
Request context: "Good enough paper prototype for the signup screen."

Router decision:
UI → UIFactory → UIFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicates non-empty)
CapabilityGapWarning: style_qualifier_noted (one warning, not two)
Note: Two gap IDs but only one CapabilityGapWarning field value emitted
DomainId: software-development
```

---

## B32-0B-032-002 — multiple gap IDs; predicates empty; one SCHEDULE_CAPABILITY_LEARNING

**Input predicates:** []
**GapWarnings:** [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]

```
Request context: "Good enough PoC."

Router decision:
Predicates = [] → knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted ONCE (not twice despite two gap IDs)
GapMetadata: {gapIds: [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]}
DomainId: software-development
```

---

## B32-0B-032-003 — three gap IDs; predicates empty; one SCHEDULE_CAPABILITY_LEARNING

**Input predicates:** []
**GapWarnings:** [gap-vibe-coding-spike-001, gap-vibe-coding-prototype-001, gap-vibe-coding-style-001]

```
Request context: "Technical spike PoC — minimum viable."

Router decision:
Predicates = [] → knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted ONCE (Rule 6 confirmed: emits exactly once)
DomainId: software-development
```

---

## B32-0B-032-004 — SCHEDULE_CAPABILITY_LEARNING emission count rule

**Rule 6 (definitive):** SCHEDULE_CAPABILITY_LEARNING emits exactly once per request when predicates = [].

| Gap IDs | Predicates | SCL count |
|---|---|---|
| 1 | [] | 1 |
| 2 | [] | 1 |
| 3 | [] | 1 |
| 5 | [] | 1 |
| 1 | ["IMPLEMENT"] | 0 |
| 3 | ["IMPLEMENT"] | 0 |

---

## B32-0B-032-005 — CapabilityGapWarning field value rule

**CapabilityGapWarning field:** Always "style_qualifier_noted" regardless of how many gap IDs are present.

| Gap IDs | Predicates non-empty | CapabilityGapWarning value |
|---|---|---|
| 1 | YES | "style_qualifier_noted" |
| 2 | YES | "style_qualifier_noted" |
| 3 | YES | "style_qualifier_noted" |
| Any | NO (predicates empty) | not present in output |

DomainId always "software-development" for B32.

---
