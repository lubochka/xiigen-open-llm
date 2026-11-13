# Layer 0-B — Router Pairs (010)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: router

---

## B12-0B-010-001 — Five-predicate routing: IMPLEMENT+API_CONTRACT+DOCUMENT+SECURITY+AUTHORIZATION

```
source_id: xiigen-b12-0b-010-001
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: router
```

A:
  run_id: "B12-0B-010-001"
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]
  user_request: "Write the OpenAPI specification for the authentication service, implement the OAuth 2.0 token endpoint, and document the security requirements."
  context:
    sub_phase_hint: "3.3"
    domain: "software-development"

B:
  RoutingDecisions:
    - predicate: "IMPLEMENT"
      routes_to: "4.3"
      sub_phase_name: "Implementation"
      flow: "ImplementationFlow43"
      trigger_terms: ["write", "implement"]
      DecisionCount: 1
    - predicate: "API_CONTRACT"
      routes_to: "3.3"
      sub_phase_name: "API Contract Design"
      flow: "APIContractFlow33"
      trigger_terms: ["openapi", "endpoint", "api"]
      DecisionCount: 2
    - predicate: "DOCUMENT"
      routes_to: "2.1"
      sub_phase_name: "Documentation"
      flow: "DocumentationFlow21"
      trigger_terms: ["specification", "document"]
      DecisionCount: 3
    - predicate: "SECURITY"
      routes_to: "3.4"
      sub_phase_name: "Security + Authorization"
      flow: "SecurityAuditFlow34"
      trigger_terms: ["authentication", "token", "oauth"]
      DecisionCount: 4
    - predicate: "AUTHORIZATION"
      routes_to: "3.4"
      sub_phase_name: "Security + Authorization"
      flow: "AuthorizationFlow34"
      trigger_terms: ["oauth"]
      DecisionCount: 5
  PrimarySubPhase: "3.3"
  TotalDecisionCount: 5
  UniqueSubPhaseCount: 4
  TransitionType: "feature_addition"
  FlowOrder: ["ImplementationFlow43", "APIContractFlow33", "DocumentationFlow21", "SecurityAuditFlow34", "AuthorizationFlow34"]

Routing note: Five predicates → five routing decisions. SECURITY and AUTHORIZATION both route
to 3.4 but are counted as separate decisions (Core Rule 23: DecisionCount = predicate count,
not unique sub-phase count). DecisionCount = 5, UniqueSubPhaseCount = 4 (4.3, 3.3, 2.1, 3.4).

---

## B12-0B-010-002 — SECURITY + AUTHORIZATION both to 3.4: two entries, one destination

```
source_id: xiigen-b12-0b-010-002
pair_kind: router_reference
```

When both SECURITY and AUTHORIZATION fire, they each produce a routing decision to 3.4:

  SECURITY → 3.4 (SecurityAuditFlow34)     ← Decision #4
  AUTHORIZATION → 3.4 (AuthorizationFlow34) ← Decision #5

These are TWO separate routing decisions even though they point to the same sub-phase.
This is because:
  1. Each predicate represents a different concern at 3.4:
     - SECURITY = authentication, token management, credential handling
     - AUTHORIZATION = access control, permission scopes, OAuth flows
  2. Two different flows handle them at 3.4:
     - SecurityAuditFlow34 handles SECURITY concerns
     - AuthorizationFlow34 handles AUTHORIZATION concerns

DecisionCount rule (Core Rule 23):
  DecisionCount = COUNT(predicates in CompiledPredicateIds)
  Five predicates = DecisionCount: 5
  NOT: count of unique sub-phases (would give 4)

---

## B12-0B-010-003 — Five-decision routing: FlowOrder for full OAuth API spec

```
source_id: xiigen-b12-0b-010-003
pair_kind: router_reference
```

FlowOrder for [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]:

  1. ImplementationFlow43 — creates the token endpoint implementation
  2. APIContractFlow33 — defines the OpenAPI specification and contract
  3. DocumentationFlow21 — documents the specification and security requirements
  4. SecurityAuditFlow34 — reviews authentication and token security
  5. AuthorizationFlow34 — reviews OAuth scopes and authorization flows

Parallel-eligible pairs (may run concurrently):
  - ImplementationFlow43 and APIContractFlow33 can run in parallel
    (implementation and spec writing can happen simultaneously)
  - SecurityAuditFlow34 and AuthorizationFlow34 can run in parallel
    (both are 3.4 reviews and don't depend on each other)

Sequential dependencies:
  - DocumentationFlow21 should run after APIContractFlow33
    (documentation captures the finalized contract)

---

## B12-0B-010-004 — Five-predicate DecisionCount vs UniqueSubPhaseCount explained

```
source_id: xiigen-b12-0b-010-004
pair_kind: router_reference
```

Worked example for DecisionCount vs UniqueSubPhaseCount:

Request: "Write the OpenAPI specification for the authentication service, implement the OAuth 2.0
token endpoint, and document the security requirements."

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]

DecisionCount calculation (Core Rule 23):
  COUNT(["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "SECURITY", "AUTHORIZATION"]) = 5
  DecisionCount: 5

UniqueSubPhaseCount calculation (for informational purposes, NOT DecisionCount):
  4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 2.1 (DOCUMENT), 3.4 (SECURITY), 3.4 (AUTHORIZATION)
  Unique sub-phases: {4.3, 3.3, 2.1, 3.4} = 4 unique destinations
  UniqueSubPhaseCount: 4 (informational only)

Core Rule 23 explicitly requires:
  ✅ DecisionCount = 5 (predicate count)
  ❌ DecisionCount = 4 (unique sub-phase count — WRONG)
