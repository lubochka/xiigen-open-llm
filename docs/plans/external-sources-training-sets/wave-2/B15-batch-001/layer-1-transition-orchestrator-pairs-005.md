# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

## TO-005-001 — Six-predicate orchestration
Request: "Build checkout service, integrate Stripe, secure tokens, write tests, document API, QA release."
Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT", "QA"]
Orchestration: APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → TestingFlow ‖ DocumentationFlow → QAGatingFlow73

## TO-005-002 — IMPLEMENT+API_CONTRACT+TEST three flows
Request: "Implement the carrier integration, wire up the FedEx API, and write integration tests."
Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"]
Orchestration: APIContractFlow33 → ImplementationFlow41 → E2ETestingFlow53 (5.3)

## TO-005-003 — Deduplication in orchestration
Request: "Build the order service, implement the order handler, create the order repository."
Predicates: ["IMPLEMENT"] (three triggers → one predicate)
Orchestration: ImplementationFlow41 (single flow, not three flows)

## TO-005-004 — IMPLEMENT+TEST+QA three flows
Request: "Implement the batch processor, write tests, and QA before deployment."
Predicates: ["IMPLEMENT", "TEST", "QA"]
Orchestration: ImplementationFlow41 → TestingFlow → QAGatingFlow73 | QA sub-phase: 7.1 (deployment)

## TO-005-005 — Ordering rules summary
1. ARCHITECTURE first
2. API_CONTRACT before IMPLEMENT
3. IMPLEMENT
4. SECURITY (after IMPLEMENT)
5. TEST (after IMPLEMENT, parallel with DOCUMENT)
6. DOCUMENT (parallel with TEST)
7. QA always last
