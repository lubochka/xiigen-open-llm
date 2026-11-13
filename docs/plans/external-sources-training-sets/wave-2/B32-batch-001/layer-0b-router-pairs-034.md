# Layer 0-B — Router Pairs (034)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-034-001 — "quick and dirty" + TEST: TestFlow dispatched

**Input predicates:** ["IMPLEMENT", "TEST"]
**GapWarnings:** [gap-vibe-coding-quick-dirty-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
TEST → TestFactory → TestFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-034-002 — "scrappy" + TEST: TestFlow dispatched; no suppression

**Input predicates:** ["IMPLEMENT", "TEST"]
**GapWarnings:** [gap-vibe-coding-style-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
TEST → TestFactory → TestFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
Note: "scrappy" does NOT suppress TEST. Rule 3 confirmed.
DomainId: software-development
```

---

## B32-0B-034-003 — gap + DOCUMENT: DocumentFlow dispatched

**Input predicates:** ["DOCUMENT"]
**GapWarnings:** [gap-vibe-coding-hack-001]

```
Request context: "Hack together some docs."

Router decision:
DOCUMENT → DocumentFactory → DocumentFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-034-004 — "walking skeleton" + TEST + DOCUMENT

**Input predicates:** ["TEST", "DOCUMENT"]
**GapWarnings:** [gap-vibe-coding-spike-001]

```
Router decision:
TEST → TestFactory → TestFlow
DOCUMENT → DocumentFactory → DocumentFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-034-005 — "paper prototype" + QA only

**Input predicates:** ["QA"]
**GapWarnings:** [gap-vibe-coding-prototype-001]

```
Router decision:
QA → QAFactory → QAFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---
