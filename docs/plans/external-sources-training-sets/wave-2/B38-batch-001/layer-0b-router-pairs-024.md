# Layer 0-B — Router Pairs (024)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-024-001 — Complete B38 routing table: all predicate-to-phase mappings

This file is a complete routing reference for B38 sub-phase 7.1.

### Phase Routing Table
| Predicate | Target Phase | Sub-Phase | Flow | Flow Type |
|---|---|---|---|---|
| QA | 7 | 7.1 | ReleaseGateFlow71 | deterministic_generic |
| IMPLEMENT | 4 | 4.1 | BackendServiceFlow41 | deterministic_generic |
| DOCUMENT | 2 | 2.1 | DocumentationFlow | deterministic_generic |
| SECURITY | 3 | 3.4 | SecurityAuditFlow | deterministic_generic |
| ARCHITECTURE | 5 | 5.x | ArchitectureFlow | deterministic_generic |
| [] (knowledge_edge) | null | null | null | N/A — no flow invoked |

### QA Ordering Rule (absolute — rule 12)
QA is always the last routing decision in any multi-predicate set.
Canonical execution order: IMPLEMENT → DOCUMENT → SECURITY → ARCHITECTURE → QA

---

## B38-0B-024-002 — Routing scenarios by predicate count

### One predicate
| PredicateSet | Routing | TransitionType |
|---|---|---|
| ["QA"] | 7.1 | no_contribution |
| ["IMPLEMENT"] | 4.1 | feature_addition |
| ["DOCUMENT"] | 2.1 | no_contribution |
| ["SECURITY"] | 3.4 | no_contribution |
| [] | knowledge_edge | N/A |

### Two predicates
| PredicateSet | Routing | TransitionType |
|---|---|---|
| ["QA","IMPLEMENT"] | 4.1 + 7.1 | feature_addition |
| ["QA","DOCUMENT"] | 2.1 + 7.1 | no_contribution |
| ["QA","SECURITY"] | 3.4 + 7.1 | no_contribution |
| ["QA","ARCHITECTURE"] | 5.x + 7.1 | no_contribution |

### Three predicates
| PredicateSet | Routing | TransitionType |
|---|---|---|
| ["QA","IMPLEMENT","DOCUMENT"] | 4.1 + 2.1 + 7.1 | feature_addition |
| ["QA","IMPLEMENT","SECURITY"] | 4.1 + 3.4 + 7.1 | feature_addition |
| ["QA","DOCUMENT","SECURITY"] | 2.1 + 3.4 + 7.1 | no_contribution |

### Four predicates
| PredicateSet | Routing | TransitionType |
|---|---|---|
| ["QA","IMPLEMENT","DOCUMENT","SECURITY"] | 4.1 + 2.1 + 3.4 + 7.1 | feature_addition |

---

## B38-0B-024-003 — DecisionCount rules summary

DecisionCount MUST equal len(PredicateSet).
- One predicate → DecisionCount: 1
- Two predicates → DecisionCount: 2
- Three predicates → DecisionCount: 3
- Four predicates → DecisionCount: 4
- Zero predicates (knowledge_edge) → DecisionCount: 1 (knowledge_edge sentinel only)

v6-03 corruption: collapsing multi-predicate set to single decision.
Example corrupt case: ["QA","IMPLEMENT"] → DecisionCount: 1 (only 7.1 returned).
Correct: ["QA","IMPLEMENT"] → DecisionCount: 2 (4.1 and 7.1 both returned).

---

## B38-0B-024-004 — Gap warnings in routing: never a routing decision

Gap warnings (CapabilityGapWarnings) are metadata attached to routing results.
They NEVER generate routing decisions.
They NEVER suppress existing routing decisions.
They NEVER change TransitionType.

Examples of gap warnings alongside active routing:
- PredicateSet: ["QA"], CapabilityGapWarnings: ["go-live","checklist","launch"] → DecisionCount: 1 (7.1 only)
- PredicateSet: ["QA","IMPLEMENT"], CapabilityGapWarnings: ["deployment"] → DecisionCount: 2 (4.1 + 7.1)
- PredicateSet: ["QA","DOCUMENT"], CapabilityGapWarnings: ["rollout"] → DecisionCount: 2 (2.1 + 7.1)
