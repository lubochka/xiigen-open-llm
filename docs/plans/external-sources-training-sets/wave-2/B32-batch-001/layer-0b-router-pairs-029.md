# Layer 0-B — Router Pairs (029)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-029-001 — gap + DOCUMENT: routes to DocumentFlow

**Input predicates:** ["DOCUMENT"]
**GapWarnings:** [gap-vibe-coding-rough-cut-001]

```
Router decision:
DOCUMENT → DocumentFactory → DocumentFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-029-002 — "minimum viable" + DOCUMENT + IMPLEMENT

**Input predicates:** ["IMPLEMENT", "DOCUMENT"]
**GapWarnings:** [gap-vibe-coding-style-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
DOCUMENT → DocumentFactory → DocumentFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-029-003 — "thin slice" + DOCUMENT only

**Input predicates:** ["DOCUMENT"]
**GapWarnings:** [gap-vibe-coding-spike-001]

```
Request context: "Thin slice: just document the happy path."

Router decision:
DOCUMENT → DocumentFactory → DocumentFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-029-004 — "rough draft" + DOCUMENT + TEST

**Input predicates:** ["DOCUMENT", "TEST"]
**GapWarnings:** [gap-vibe-coding-rough-cut-001]

```
Router decision:
DOCUMENT → DocumentFactory → DocumentFlow
TEST → TestFactory → TestFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-029-005 — DOCUMENT routing summary

**All DOCUMENT routing patterns for B32:**

| GapWarning | Predicates | Route |
|---|---|---|
| Any gap-vibe-coding-* | ["DOCUMENT"] | DocumentFlow |
| Any gap-vibe-coding-* | ["IMPLEMENT", "DOCUMENT"] | ImplementationFlow + DocumentFlow |
| Any gap-vibe-coding-* | ["DOCUMENT", "TEST"] | DocumentFlow + TestFlow |
| None | [] | knowledge_edge |

SCHEDULE_CAPABILITY_LEARNING emits only when predicates = []. DomainId always "software-development".

---
