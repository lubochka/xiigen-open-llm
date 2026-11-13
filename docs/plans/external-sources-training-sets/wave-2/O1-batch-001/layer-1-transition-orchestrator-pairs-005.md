# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-005-001 — SECURITY+TEST sequential → SecurityVerification34Flow → UnitTestingFlow51 (TEST waits)

Request: "Build the JWT validation service and write unit tests for all token edge cases."
Predicates: ["SECURITY", "TEST"] | Orchestration: SecurityVerification34Flow → UnitTestDesign51Flow
TransitionType: feature_addition
Branch 1: SecurityVerification34Flow (3.4) | FactoryState: trusted_learned
Branch 2: generic nine-node (5.1) | FactoryState: probationary_rejected (fallback)
Execution mode: → sequential (TEST branch waits for SECURITY branch to complete)
Sequencing rule: when IMPLEMENT or SECURITY precedes TEST, TEST execution is deferred until
  the implementation/security branch completes. TEST verifies the output of prior branches.

---

## TO-005-002 — AUTHORIZATION+TEST sequential

Request: "Create the role management service and assert all RBAC (Role-Based Access Control) boundary conditions."
Predicates: ["AUTHORIZATION", "TEST"] | Orchestration: SecurityVerification34Flow → generic-5.1
TransitionType: feature_addition
Branch 1: SecurityVerification34Flow (3.4) | sequential-first
Branch 2: generic nine-node (5.1) | sequential-second (waits for Branch 1)
RBAC expansion: RBAC = Role-Based Access Control

---

## TO-005-003 — SECURITY+AUTHORIZATION+TEST sequential

Request: "Implement tenant JWT enforcement and write tests for permission isolation."
Predicates: ["SECURITY", "AUTHORIZATION", "TEST"] | Orchestration: SecurityVerification34Flow → generic-5.1
TransitionType: feature_addition
Branch 1: SecurityVerification34Flow (3.4) | EvidencePredicates: ["SECURITY", "AUTHORIZATION"] | sequential-first
Branch 2: generic nine-node (5.1) | sequential-second

---

## TO-005-004 — API_CONTRACT+TEST → parallel or sequential (depends on TEST referencing API output)

Request: "Create the token exchange API contract and write tests for the schema validation."
Predicates: ["SECURITY", "API_CONTRACT", "TEST"] | Orchestration: SecurityVerification34Flow ‖ ApiContract33Flow → generic-5.1
TransitionType: feature_addition
Branch 1: SecurityVerification34Flow (3.4) | parallel with Branch 2
Branch 2: ApiContract33Flow (3.3) | parallel with Branch 1
Branch 3: generic nine-node (5.1) | sequential — waits for Branch 1 and Branch 2
Note: TEST branch always waits for preceding branches when it is testing their output.

---

## TO-005-005 — DOCUMENT+TEST parallel (both are output-only, no dependency)

Request: "Write the JWT security spec and create unit tests for the auth flow."
Predicates: ["SECURITY", "DOCUMENT", "TEST"] | Orchestration: SecurityVerification34Flow ‖ RequirementsAuthoringFlow21 ‖ generic-5.1
TransitionType: documentation (no IMPLEMENT fires)
All branches: ‖ parallel
Note: When TEST and DOCUMENT both fire without IMPLEMENT or SECURITY implementation work
  to depend on, the branches are parallel. In this case SECURITY also fires (for spec context)
  but the intent is documentation — no new implementation is produced that TEST must wait for.
