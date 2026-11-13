# Layer 1 — Transition-Orchestrator Pairs (010)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T010-001 — TransitionType: feature_addition | TEST+IMPLEMENT via "add"+"test double"

Request: "Add a test double for the EmailProvider and write tests for the NotificationService."
  TransitionType: feature_addition
  Evidence: "add" → IMPLEMENT; "test double" → TEST; "write tests" → TEST+IMPLEMENT (duplicate)
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41

---

## T010-002 — TransitionType: refactoring | IMPLEMENT+TEST via "refactor"+"unit tests"

Request: "Refactor the CheckoutService to inject the PricingEngine as a dependency and update the unit tests."
  TransitionType: refactoring
  Evidence: "refactor" → refactoring; "update" → IMPLEMENT secondary; "unit tests" → TEST
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  OrchestratorDecision: FeatureImplementationFlow41 (refactoring) + UnitTestingFlow51

---

## T010-003 — TransitionType: feature_addition | TEST+IMPLEMENT+AUTHORIZATION+SECURITY

Request: "Write tests that assert the API Gateway blocks JWT forgery, implement the signature verification, and verify only signed tokens pass the authorization filter."
  TransitionType: feature_addition
  Evidence: "write tests" → TEST+IMPLEMENT; "JWT forgery" → SECURITY; "implement" → IMPLEMENT; "verify" → TEST; "authorization filter" → AUTHORIZATION
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "SECURITY", "AUTHORIZATION"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + SecurityTestingFlow52 + AuthorizationTestingFlow52

---

## T010-004 — TransitionType: feature_addition | TEST+QA via "test suite"+"QA"

Request: "Run the test suite for the BillingModule and submit QA approval before the patch release."
  TransitionType: feature_addition
  Evidence: "run" is gap; "test suite" → TEST; "QA" → QA; "patch release" is gap
  CompiledPredicateIds: ["TEST", "QA"]
  OrchestratorDecision: UnitTestingFlow51 + QAGatingFlow73

---

## T010-005 — Gap: "happy path" + "cover" → zero predicates

Request: "Cover the happy path for the CustomerRegistration flow."
  TransitionType: N/A (gap)
  CompiledPredicateIds: []
  OrchestratorDecision: GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development")
  Teaching: "cover" is gap; "happy path" is gap testing vocabulary. Zero predicates.
