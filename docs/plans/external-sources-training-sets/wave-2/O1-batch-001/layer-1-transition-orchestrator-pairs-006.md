# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-006-001 — SECURITY+AUTHORIZATION+API_CONTRACT+TEST full four-predicate orchestration

Request: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"]
Orchestration: (SecurityVerification34Flow ‖ ApiContract33Flow) → generic-5.1
TransitionType: feature_addition
Branch 1: SecurityVerification34Flow (3.4) | EvidencePredicates: ["SECURITY", "AUTHORIZATION"] | parallel in Group A
Branch 2: ApiContract33Flow (3.3) | EvidencePredicates: ["API_CONTRACT"] | parallel in Group A
Branch 3: generic nine-node (5.1) | EvidencePredicates: ["TEST"] | sequential — waits for Group A

Execution structure:
  Group A (parallel): Branch 1 ‖ Branch 2
  Group B (sequential after A): Branch 3
TransitionType: feature_addition (implied IMPLEMENT from "add")

---

## TO-006-002 — SECURITY+AUTHORIZATION+API_CONTRACT+TEST + feature_addition explicit

Request: "Build the tenant JWT auth module, create the OpenAPI contract, and write isolation tests."
Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"]
Orchestration: ImplementationFlow41 → (SecurityVerification34Flow ‖ ApiContract33Flow) → generic-5.1
TransitionType: feature_addition
Branch 1: ImplementationFlow41 (4.1) | sequential-first
Branch 2: SecurityVerification34Flow (3.4) | parallel in Group B
Branch 3: ApiContract33Flow (3.3) | parallel in Group B
Branch 4: generic nine-node (5.1) | sequential — waits for Group B

Execution structure:
  Phase 1 (sequential): Branch 1 (Implementation)
  Phase 2 (parallel): Branch 2 ‖ Branch 3 (Security+Auth, API Contract)
  Phase 3 (sequential after Phase 2): Branch 4 (Tests)

---

## TO-006-003 — SECURITY+TEST+DOCUMENT

Request: "Implement the JWT token blacklist and write tests and security docs."
Predicates: ["IMPLEMENT", "SECURITY", "TEST", "DOCUMENT"]
Orchestration: ImplementationFlow41 → SecurityVerification34Flow → (generic-5.1 ‖ RequirementsAuthoringFlow21)
TransitionType: feature_addition
Branch 1: ImplementationFlow41 (4.1) | sequential-first
Branch 2: SecurityVerification34Flow (3.4) | sequential-second
Branch 3: generic nine-node (5.1) | parallel in Group C
Branch 4: RequirementsAuthoringFlow21 (2.1) | parallel in Group C

---

## TO-006-004 — All seven predicates (ARCHITECTURE excluded): full orchestration

Request: "Implement JWT tenant auth, create the API contract, secure the endpoints, write isolation tests, and document the design."
Predicates: ["API_CONTRACT", "IMPLEMENT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT"]
Orchestration: ApiContract33Flow ‖ ImplementationFlow41 → SecurityVerification34Flow → (generic-5.1 ‖ RequirementsAuthoringFlow21)
TransitionType: feature_addition
Phase 1 (parallel): ApiContract33Flow (3.3) ‖ ImplementationFlow41 (4.1)
Phase 2 (sequential after Phase 1): SecurityVerification34Flow (3.4)
Phase 3 (parallel after Phase 2): generic-5.1 ‖ RequirementsAuthoringFlow21 (2.1)

---

## TO-006-005 — Orchestration summary: parallel vs sequential rules

Reference pair — rules for when branches are parallel vs sequential.

Parallel (‖): branches that have no output dependency on each other
  Examples: SECURITY+API_CONTRACT (independent work streams)
            TEST+DOCUMENT (both are outputs with no dependency)
            DOCUMENT+API_CONTRACT (documentation can proceed in parallel)

Sequential (→): branches where one must complete before another starts
  Examples: IMPLEMENT → SECURITY (security verifies what implement built)
            IMPLEMENT → TEST (tests verify the implementation)
            SECURITY → TEST (tests verify security behavior)
            Any branch → QA (QA is always last)

Mixed: implementation first, then parallel security+api, then sequential test
  This is the most common O1 orchestration pattern for feature_addition requests.
