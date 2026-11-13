# Layer 1 — Transition-Orchestrator Pairs (011)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T011-001 — TransitionType: feature_addition | TEST via "acceptance tests"

Request: "Write acceptance tests for the new ProductBundling feature with the QA team."
  TransitionType: feature_addition
  Evidence: "write" → IMPLEMENT; "acceptance tests" → TEST; "QA team" — "QA" fires QA predicate
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "QA"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + QAGatingFlow73

---

## T011-002 — TransitionType: feature_addition | TEST only via "mutation testing"

Request: "Apply mutation testing to the BillingService to strengthen the assertion coverage."
  TransitionType: feature_addition
  Evidence: "apply" → IMPLEMENT; "mutation testing" → TEST via "testing"
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41

---

## T011-003 — TransitionType: refactoring | IMPLEMENT+TEST via "extract"+"unit tests"

Request: "Extract the tax calculation logic and add unit tests that cover the extracted TaxRulesEngine."
  TransitionType: refactoring
  Evidence: "extract" → refactoring; "add unit tests" → TEST+IMPLEMENT
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  OrchestratorDecision: FeatureImplementationFlow41 (refactoring) + UnitTestingFlow51

---

## T011-004 — TransitionType: feature_addition | TEST+DOCUMENT via "spec"+"document"

Request: "Spec the ReturnService test cases and document the return policy edge cases."
  TransitionType: feature_addition
  Evidence: "spec" → TEST+IMPLEMENT; "document" → DOCUMENT
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "DOCUMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + DocumentationFlow62

---

## T011-005 — TransitionType: feature_addition | TEST+IMPLEMENT+SECURITY+AUTHORIZATION (4)

Request: "Implement MFA (Multi-Factor Authentication) for the AdminService, write tests for each auth step, assert the OTP (One-Time Password) expires after 5 minutes, and verify role escalation is blocked."
  TransitionType: feature_addition
  Evidence: "implement" → IMPLEMENT; "write tests" → TEST+IMPLEMENT; "assert OTP expires" → TEST+SECURITY; "role escalation is blocked" → AUTHORIZATION
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "SECURITY", "AUTHORIZATION"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + SecurityTestingFlow52 + AuthorizationTestingFlow52
