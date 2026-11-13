# Layer 0-B — Router Pairs (012)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: router

---

## O1-router-012-001 — Complete routing table for O1

Reference pair — routing table for all predicates in O1 scenario.

| Predicate | Sub-phase | Flow | FactoryState |
|---|---|---|---|
| ARCHITECTURE | 3.x | ArchitectureFlow (scenario-specific) | deterministic_generic |
| API_CONTRACT | 3.3 | ApiContract33FlowAdapter | probationary_accepted |
| IMPLEMENT | 4.1 | ImplementationFlow41 | deterministic_generic |
| SECURITY | 3.4 | SecurityVerification34Flow | trusted_learned |
| AUTHORIZATION | 3.4 | SecurityVerification34Flow | trusted_learned |
| TEST | 5.1 | UnitTestDesign51FlowAdapter | probationary_rejected (generic fallback) |
| DOCUMENT | 2.1 | RequirementsAuthoringFlow21 | deterministic_generic |
| QA | 7.x | QAGatingFlow73 | deterministic_generic |

Deduplication: SECURITY+AUTHORIZATION share 3.4 → single routing decision when both fire.
QA ordering: always last in RoutingDecision[] output.
TransitionType selection: IMPLEMENT present → feature_addition; TEST/DOCUMENT without IMPLEMENT → documentation.

---

## O1-router-012-002 — Sub-phase routing matrix: predicate combinations

Reference pair — routing decision count for common predicate combinations.

| Predicate combination | Routing decisions | Sub-phases |
|---|---|---|
| SECURITY | 1 | 3.4 |
| AUTHORIZATION | 1 | 3.4 |
| SECURITY+AUTHORIZATION | 1 | 3.4 |
| API_CONTRACT | 1 | 3.3 |
| SECURITY+API_CONTRACT | 2 | 3.4, 3.3 |
| TEST | 1 | 5.1 |
| SECURITY+TEST | 2 | 3.4, 5.1 |
| SECURITY+AUTHORIZATION+TEST | 2 | 3.4, 5.1 |
| SECURITY+AUTHORIZATION+API_CONTRACT+TEST | 3 | 3.4, 3.3, 5.1 |
| DOCUMENT | 1 | 2.1 |
| SECURITY+DOCUMENT | 2 | 3.4, 2.1 |
| IMPLEMENT+SECURITY | 2 | 4.1, 3.4 |
| IMPLEMENT+SECURITY+AUTHORIZATION | 2 | 4.1, 3.4 |

---

## O1-router-012-003 — Knowledge edge routing rules

Reference pair — when the router produces a knowledge_edge result.

Conditions that produce KnowledgeEdge: true:
  1. No predicate fires (CompiledPredicateIds is empty)
  2. All fired predicates map to an unregistered sub-phase (unroutable-transition)

Result when KnowledgeEdge fires:
  RoutingDecisions array contains exactly one entry:
    PredictedSubPhaseId: null
    KnowledgeEdge: true
    FailureClassification: "no_predicate_match" or "unroutable_transition"
  Confidence: 0.0
  The orchestrator reads this and routes to the knowledge-edge synthetic flow

Common O1 knowledge_edge triggers:
  "login page" — "login" not in any lexicon
  "login reset" — "login", "reset" not in any lexicon
  "access denied" — "access denied" is colloquial, not "access control"
  "it stopped working" — no lexicon match

---

## O1-router-012-004 — Routing integrity rules (cross-layer)

Reference pair — router output integrity requirements.

1. Every predicate that fires must produce a routing decision to its registered sub-phase.
2. Co-predicates for the same sub-phase must produce exactly ONE routing decision.
3. QA always appears last in the RoutingDecision[] array.
4. No sub-phase appears more than once in the output (deduplication).
5. KnowledgeEdge and non-KnowledgeEdge decisions never appear in the same output array.
6. EvidencePredicates in each decision must contain only the predicates that fired
   and are assigned to that decision's sub-phase.
7. The router does NOT determine execution sequence — that is the orchestrator's concern.

---

## O1-router-012-005 — v6-03 corruption detection checklist

Reference pair — how to identify v6-03 routing corruption.

v6-03 errors to detect:
  A. TEST predicate routed to 3.4 instead of 5.1 (isolation conflated with security)
  B. SECURITY+AUTHORIZATION producing two separate 3.4 decisions instead of one
  C. QA appearing before any other decision in the output array
  D. API_CONTRACT routed to 3.4 instead of 3.3 (API conflated with security)
  E. DOCUMENT routed to 3.4 instead of 2.1 (spec conflated with security)
  F. IMPLEMENT appearing in zero routing decisions when it fired in compilation

Detection method: count routing decisions against expected deduplication result;
  verify QA is last; verify each predicate maps to its canonical sub-phase only.
