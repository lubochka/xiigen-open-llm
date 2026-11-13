# Layer 1 — Transition-Orchestrator Pairs (008)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T008-001 — TransitionType: feature_addition | TEST only via "test fixture"

Request: "Set up a test fixture for the ProductRepository with seeded in-memory data."
  TransitionType: feature_addition
  Evidence: "set up" is gap for IMPLEMENT; "test fixture" → TEST; no IMPLEMENT verb
  CompiledPredicateIds: ["TEST"]
  OrchestratorDecision: UnitTestingFlow51 | Teaching: "set up" gap confirmed; TEST fires on object noun

---

## T008-002 — TransitionType: feature_addition | TEST+IMPLEMENT via "write"+"unit tests"

Request: "Write unit tests for the LoyaltyRewardsService covering point calculation and tier upgrades."
  TransitionType: feature_addition
  Evidence: "write" → IMPLEMENT; "unit tests" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41

---

## T008-003 — TransitionType: feature_addition | TEST+SECURITY+AUTHORIZATION via multi-verb

Request: "Verify the AdminService blocks unauthenticated access and assert the audit log records each denial."
  TransitionType: feature_addition
  Evidence: "verify" → TEST; "blocks unauthenticated access" → AUTHORIZATION; "assert" → TEST (duplicate); "audit log" is domain gap
  CompiledPredicateIds: ["TEST", "AUTHORIZATION"]
  OrchestratorDecision: UnitTestingFlow51 + AuthorizationTestingFlow52

---

## T008-004 — TransitionType: refactoring | IMPLEMENT+TEST via "refactor"+"verify"

Request: "Refactor the InvoiceService to eliminate the static singleton and verify all existing unit tests still pass."
  TransitionType: refactoring
  Evidence: "refactor" → refactoring; "verify" → TEST; "unit tests" → TEST (duplicate)
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  OrchestratorDecision: FeatureImplementationFlow41 (refactoring) + UnitTestingFlow51

---

## T008-005 — TransitionType: feature_addition | TEST+IMPLEMENT+API_CONTRACT+SECURITY

Request: "Write API contract tests for the PaymentGatewayClient, implement the TLS (Transport Layer Security) certificate validation, and assert no plaintext traffic is emitted."
  TransitionType: feature_addition
  Evidence: "write" → IMPLEMENT; "API contract tests" → TEST+API_CONTRACT; "implement" → IMPLEMENT; "TLS certificate" → SECURITY; "assert" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "API_CONTRACT", "SECURITY"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + ContractTestingFlow33 + SecurityTestingFlow52
