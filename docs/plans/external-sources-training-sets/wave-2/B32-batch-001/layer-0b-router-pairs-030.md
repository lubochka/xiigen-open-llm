# Layer 0-B — Router Pairs (030)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-030-001 — gap + QA: routes to QAFlow

**Input predicates:** ["QA"]
**GapWarnings:** [gap-vibe-coding-quick-dirty-001]

```
Router decision:
QA → QAFactory → QAFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-030-002 — "minimum viable" + IMPLEMENT + QA

**Input predicates:** ["IMPLEMENT", "QA"]
**GapWarnings:** [gap-vibe-coding-style-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
QA → QAFactory → QAFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
Note: QAFlow executes last in ordered sequence
DomainId: software-development
```

---

## B32-0B-030-003 — "PoC" + IMPLEMENT + TEST + QA

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

## B32-0B-030-004 — QA only, no other predicates

**Input predicates:** ["QA"]
**GapWarnings:** [gap-vibe-coding-style-001]

```
Request context: "Scrappy but QA the registration form before release."

Router decision:
QA → QAFactory → QAFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-030-005 — QA routing summary

**All QA routing patterns for B32:**

| GapWarning | Predicates | Route |
|---|---|---|
| Any gap-vibe-coding-* | ["QA"] | QAFlow |
| Any gap-vibe-coding-* | ["IMPLEMENT", "QA"] | ImplementationFlow → QAFlow |
| Any gap-vibe-coding-* | ["IMPLEMENT", "TEST", "QA"] | ImplementationFlow → TestFlow → QAFlow |
| None | [] | knowledge_edge |

QAFlow always executes last. DomainId always "software-development".

---
