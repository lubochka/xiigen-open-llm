# Layer 1 — Transition Orchestrator Pairs (004)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-004-001 — SECURITY+API_CONTRACT fan-out → SecurityVerification34Flow ‖ generic-3.3 (parallel)

Request: "Define the JWT authentication service API endpoints with OpenAPI contract."
Predicates: ["SECURITY", "API_CONTRACT"] | Orchestration: SecurityVerification34Flow ‖ ApiContract33Flow
TransitionType: feature_addition
Branch 1: SecurityVerification34Flow (3.4) | FactoryState: trusted_learned
Branch 2: ApiContract33Flow (3.3) | FactoryState: deterministic_generic
Execution mode: ‖ parallel — both branches run concurrently
Routing decisions: 2 (different sub-phases → fan-out → parallel branches)

---

## TO-004-002 — API_CONTRACT+SECURITY (canonical order preserved in branches)

Request: "Build the secure REST endpoints for the tenant management API."
Predicates: ["API_CONTRACT", "SECURITY", "AUTHORIZATION"] | Orchestration: ApiContract33Flow ‖ SecurityVerification34Flow
TransitionType: feature_addition
Branch 1: ApiContract33Flow (3.3) — API_CONTRACT at canonical position 2
Branch 2: SecurityVerification34Flow (3.4) — SECURITY+AUTHORIZATION at canonical position 4
Execution mode: ‖ parallel
Note: API_CONTRACT appears before SECURITY in canonical ordering; Branch 1 is the API contract flow

---

## TO-004-003 — SECURITY+API_CONTRACT from webhook prompt (fan-out)

Request: "Add secure webhook delivery endpoints for tenant-scoped authentication events."
Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | Orchestration: SecurityVerification34Flow ‖ ApiContract33Flow
TransitionType: feature_addition
Branch 1: SecurityVerification34Flow (3.4) | EvidencePredicates: ["SECURITY", "AUTHORIZATION"]
Branch 2: ApiContract33Flow (3.3) | EvidencePredicates: ["API_CONTRACT"]
Execution mode: ‖ parallel (two independent branches)

---

## TO-004-004 — API_CONTRACT alone → generic-3.3 (single branch, no parallel)

Request: "Specify the request and response schema for the token exchange endpoint."
Predicates: ["SECURITY", "API_CONTRACT"] | Orchestration: SecurityVerification34Flow ‖ ApiContract33Flow
TransitionType: documentation (no IMPLEMENT fires on "specify")
Correction: Both SECURITY (token) and API_CONTRACT (endpoint, schema, request, response) fire
  Fan-out to two parallel branches; TransitionType: documentation (no IMPLEMENT)
Branch 1: SecurityVerification34Flow (3.4)
Branch 2: ApiContract33Flow (3.3)
Execution mode: ‖ parallel

---

## TO-004-005 — Large fan-out: SECURITY+AUTHORIZATION+API_CONTRACT+DOCUMENT

Request: "Document the JWT auth API contract for tenant onboarding developers."
Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "DOCUMENT"]
Orchestration: SecurityVerification34Flow ‖ ApiContract33Flow ‖ RequirementsAuthoringFlow21
TransitionType: documentation (DOCUMENT fires; no IMPLEMENT)
Branch 1: SecurityVerification34Flow (3.4) | parallel
Branch 2: ApiContract33Flow (3.3) | parallel
Branch 3: RequirementsAuthoringFlow21 (2.1) | parallel
Execution mode: ‖ all three parallel
