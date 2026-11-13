# Layer 0-B — Router Pairs (028)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-028-001 — gap + TEST: router dispatches TestFlow

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

## B32-0B-028-002 — gap + TEST only (no IMPLEMENT)

**Input predicates:** ["TEST"]
**GapWarnings:** [gap-vibe-coding-style-001]

```
Request context: "Scrappy but test the critical paths."

Router decision:
TEST → TestFactory → TestFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
Note: Only TEST in predicates; only TestFlow dispatched
DomainId: software-development
```

---

## B32-0B-028-003 — PoC + TEST + QA

**Input predicates:** ["IMPLEMENT", "TEST", "QA"]
**GapWarnings:** [gap-vibe-coding-prototype-001]

```
Router decision:
IMPLEMENT → ImplementationFactory → ImplementationFlow
TEST → TestFactory → TestFlow
QA → QAFactory → QAFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
Note: QAFlow ordered last
DomainId: software-development
```

---

## B32-0B-028-004 — "tracer bullet" + TEST + ARCHITECTURE

**Input predicates:** ["ARCHITECTURE", "TEST"]
**GapWarnings:** [gap-vibe-coding-spike-001]

```
Router decision:
ARCHITECTURE → ArchitectureFactory → ArchitectureFlow31
TEST → TestFactory → TestFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-028-005 — TEST fires; SCHEDULE_CAPABILITY_LEARNING rule confirmation

**Rule confirmed:** SCHEDULE_CAPABILITY_LEARNING is gated strictly on predicates = [].

| Predicates | SCL emitted? |
|---|---|
| [] | YES |
| ["TEST"] | NO |
| ["IMPLEMENT"] | NO |
| ["TEST", "QA"] | NO |
| ["IMPLEMENT", "TEST", "QA"] | NO |

**DomainId:** Always "software-development" for B32.

---
