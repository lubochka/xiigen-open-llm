# Layer 0-B — Router Pairs (035)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-035-001 — PoC + IMPLEMENT + TEST + QA: full flow chain

**Input predicates:** ["IMPLEMENT", "TEST", "QA"]
**GapWarnings:** [gap-vibe-coding-prototype-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
TEST → TestFactory → TestFlow
QA → QAFactory → QAFlow (last)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-035-002 — "good enough" + IMPLEMENT + QA: ordered execution

**Input predicates:** ["IMPLEMENT", "QA"]
**GapWarnings:** [gap-vibe-coding-style-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
QA → QAFactory → QAFlow (last)
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-035-003 — "minimum viable" + all 7 predicates: QA last

**Input predicates:** ["IMPLEMENT", "UI", "ARCHITECTURE", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT", "QA"]
**GapWarnings:** [gap-vibe-coding-style-001]

```
Router decision:
All 8 predicates → 8 factories invoked
QAFlow always last in ordered sequence
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-035-004 — gap + SECURITY: SecurityFlow dispatched

**Input predicates:** ["SECURITY"]
**GapWarnings:** [gap-vibe-coding-hack-001]

```
Router decision:
SECURITY → SecurityFactory → SecurityFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-035-005 — gap + API_CONTRACT: APIContractFlow dispatched

**Input predicates:** ["API_CONTRACT"]
**GapWarnings:** [gap-vibe-coding-style-001]

```
Router decision:
API_CONTRACT → APIContractFactory → APIContractFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---
