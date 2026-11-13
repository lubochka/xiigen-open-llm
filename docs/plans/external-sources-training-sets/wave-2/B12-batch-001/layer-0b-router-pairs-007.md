# Layer 0-B — Router Pairs (007)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-007-001 — [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY] → 4.3 + 3.3 + 2.1 + 3.4 (four decisions)

```
source_id: xiigen-b12-0b-007-001
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  run_id: "B12-0B-007-001"
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY"]
  user_request: "Write the OpenAPI spec for the cart service and include authentication requirements."
  context:
    sub_phase_hint: "3.3"
    domain: "software-development"

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"
      routes_to: "4.3"
      sub_phase_name: "Implementation"
      flow: "ImplementationFlow43"
      trigger_terms: ["write"]
      DecisionCount: 1
    - predicate: "API_CONTRACT"
      routes_to: "3.3"
      sub_phase_name: "API Contract Design"
      flow: "APIContractFlow33"
      trigger_terms: ["openapi", "api"]
      DecisionCount: 2
    - predicate: "DOCUMENT"
      routes_to: "2.1"
      sub_phase_name: "Documentation"
      flow: "DocumentationFlow21"
      trigger_terms: ["spec", "requirements"]
      DecisionCount: 3
    - predicate: "SECURITY"
      routes_to: "3.4"
      sub_phase_name: "Security + Authorization"
      flow: "SecurityAuditFlow34"
      trigger_terms: ["authentication"]
      DecisionCount: 4
  PrimarySubPhase: "3.3"
  TotalDecisionCount: 4
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34"]

Routing note: Four predicates → four routing decisions to four unique sub-phases.
IMPLEMENT fires → TransitionType = feature_addition (new artifact being written).
SECURITY fires on "authentication" → adds 3.4 to routing.
FlowOrder: implementation planning first, then API contract, then documentation, then security review.

---

## B12-0B-007-002 — Four-decision FlowOrder rationale

```
source_id: xiigen-b12-0b-007-002
pair_kind: router_reference
```

FlowOrder for [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY]:

  1. ImplementationFlow43 (Phase 4.3) — "write" the spec (create the artifact)
  2. APIContractFlow33 (Phase 3.3) — define the API contract structure
  3. DocumentationFlow21 (Phase 2.1) — document the decisions
  4. SecurityAuditFlow34 (Phase 3.4) — review authentication/security requirements

Why this order?
  - Implementation and API contract work are parallelizable in many cases
  - Security review of the contract happens after contract is defined
  - Documentation captures the final state after all other flows complete
  FlowOrder is a guidance sequence, not strict execution order — parallel-eligible flows
  may run concurrently per the orchestrator's parallelization rules.

---

## B12-0B-007-003 — Four-decision vs three-decision comparison

```
source_id: xiigen-b12-0b-007-003
pair_kind: router_reference
```

Comparison of three-decision and four-decision routing:

Three-decision [ARCHITECTURE, API_CONTRACT, DOCUMENT]:
  Routes: 3.1 + 3.3 + 2.1
  FlowOrder: [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21]
  TransitionType: documentation
  DecisionCount: 3

Four-decision [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY]:
  Routes: 4.3 + 3.3 + 2.1 + 3.4
  FlowOrder: [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34]
  TransitionType: feature_addition
  DecisionCount: 4

The difference:
  - ARCHITECTURE → adds 3.1, keeps documentation TransitionType
  - IMPLEMENT → adds 4.3, changes TransitionType to feature_addition
  - SECURITY → adds 3.4, does not change TransitionType

---

## B12-0B-007-004 — Four-decision routing variations

```
source_id: xiigen-b12-0b-007-004
pair_kind: router_reference
```

All four-decision routing patterns in B12:

Pattern A: [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY]
  → 4.3 + 3.3 + 2.1 + 3.4
  TransitionType: feature_addition
  Trigger: "write/implement/build" + "openapi/api" + "spec/document" + "authentication/token"

Pattern B: [ARCHITECTURE, API_CONTRACT, DOCUMENT, AUTHORIZATION]
  → 3.1 + 3.3 + 2.1 + 3.4
  TransitionType: documentation
  Trigger: "design" + "api/rest" + "document" + "access control/oauth"

Pattern C: [IMPLEMENT, API_CONTRACT, DOCUMENT, AUTHORIZATION]
  → 4.3 + 3.3 + 2.1 + 3.4
  TransitionType: feature_addition
  Trigger: "implement/write" + "api" + "document" + "authorization/access control"

Pattern D: [ARCHITECTURE, API_CONTRACT, DOCUMENT, SECURITY]
  → 3.1 + 3.3 + 2.1 + 3.4
  TransitionType: documentation
  Trigger: "design" + "api/rest" + "document" + "authentication/token"
