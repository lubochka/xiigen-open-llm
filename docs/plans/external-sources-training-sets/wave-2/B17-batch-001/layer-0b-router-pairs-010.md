# Layer 0-B — Router Pairs (010)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: router

---

## B17-0B-010-001 — [IMPLEMENT, API_CONTRACT, DOCUMENT, QA] → 4.3 + 3.3 + 2.1 + 7.x

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "QA"]
**ConnectorName:** DocuSignConnector

**Routing decisions:**

| # | Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|---|
| 1 | IMPLEMENT | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| 2 | API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |
| 3 | DOCUMENT | 2.1 | Documentation and Specification | DocumentationFlow21 |
| 4 | QA | 7.x | QA Gating | QAGatingFlow73 |

**DecisionCount:** 4
**TransitionType:** feature_addition
**QA position:** always last (Rule 21: QA is absolute last in ordering)

---

## B17-0B-010-002 — QA routes to sub-phase 7.x (QAGatingFlow73)

**Rule:** QA → sub-phase 7.x, FlowId: QAGatingFlow73
**Sub-phase 7.x name:** QA Gating and Validation

**QA routing does not interact with other sub-phases.** QA always gets its own separate sub-phase 7.x regardless of what other predicates are present.

**QA position in flow:** QA is always executed last. Regardless of predicate ordering, QAGatingFlow73 always runs after all other flows complete. This is an ordering constraint, not a routing constraint.

---

## B17-0B-010-003 — QA routing output

```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "QA"]
Routing: [
  { predicate: "IMPLEMENT", subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { predicate: "API_CONTRACT", subPhase: "3.3", flow: "APIContractFlow33" },
  { predicate: "DOCUMENT", subPhase: "2.1", flow: "DocumentationFlow21" },
  { predicate: "QA", subPhase: "7.x", flow: "QAGatingFlow73" }
]
DecisionCount: 4
QAPosition: last (absolute)
TransitionType: feature_addition
```

---

## B17-0B-010-004 — QA alone routing: no contribution

**Input:** "QA the DocuSign signing flow end-to-end."
**CompiledPredicateIds:** ["QA"]
**Routing:** [{ predicate: "QA", subPhase: "7.x", flow: "QAGatingFlow73" }]
**DecisionCount:** 1
**TransitionType:** no contribution (QA alone → no contribution, Rule 21)

**Contrast with IMPLEMENT+QA:**
- IMPLEMENT+QA → TransitionType: feature_addition (IMPLEMENT dominates)
- QA alone → TransitionType: no contribution

**Teaching point:** QA alone never produces a positive TransitionType. QA gates/validates work but doesn't itself constitute a feature or documentation contribution.
