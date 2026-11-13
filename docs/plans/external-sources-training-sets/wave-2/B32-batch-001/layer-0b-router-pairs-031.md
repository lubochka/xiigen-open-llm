# Layer 0-B — Router Pairs (031)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-031-001 — "proof-of-concept" + ARCHITECTURE

**Input predicates:** ["ARCHITECTURE"]
**GapWarnings:** [gap-vibe-coding-prototype-001]

```
Router decision:
ARCHITECTURE → ArchitectureFactory → ArchitectureFlow31
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: software-development
```

---

## B32-0B-031-002 — "walking skeleton" + ARCHITECTURE + IMPLEMENT

**Input predicates:** ["ARCHITECTURE", "IMPLEMENT"]
**GapWarnings:** [gap-vibe-coding-spike-001]

```
Router decision:
ARCHITECTURE → ArchitectureFactory → ArchitectureFlow31
IMPLEMENT → ImplementationFactory → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
Note: TransitionType = feature_addition (IMPLEMENT present → takes precedence)
DomainId: software-development
```

---

## B32-0B-031-003 — "minimum viable" + ARCHITECTURE only

**Input predicates:** ["ARCHITECTURE"]
**GapWarnings:** [gap-vibe-coding-style-001]

```
Router decision:
ARCHITECTURE → ArchitectureFactory → ArchitectureFlow31
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
Note: TransitionType = documentation (ARCHITECTURE only; no IMPLEMENT)
DomainId: software-development
```

---

## B32-0B-031-004 — "thin slice" + ARCHITECTURE + API_CONTRACT

**Input predicates:** ["ARCHITECTURE", "API_CONTRACT"]
**GapWarnings:** [gap-vibe-coding-spike-001]

```
Router decision:
ARCHITECTURE → ArchitectureFactory → ArchitectureFlow31
API_CONTRACT → APIContractFactory → APIContractFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
Note: TransitionType = documentation (ARCHITECTURE sets type; no IMPLEMENT)
DomainId: software-development
```

---

## B32-0B-031-005 — spike-family gap ID dispatch summary

**All spike-family (gap-vibe-coding-spike-001) routing patterns:**

| Context | Predicates | Route |
|---|---|---|
| spike alone | [] | knowledge_edge + SCL |
| tracer bullet alone | [] | knowledge_edge + SCL |
| walking skeleton alone | [] | knowledge_edge + SCL |
| thin slice alone | [] | knowledge_edge + SCL |
| vertical slice alone | [] | knowledge_edge + SCL |
| spike + implement | ["IMPLEMENT"] | ImplementationFlow |
| spike + architecture | ["ARCHITECTURE"] | ArchitectureFlow31 |
| spike + implement + test | ["IMPLEMENT", "TEST"] | ImplementationFlow + TestFlow |

DomainId always "software-development".

---
