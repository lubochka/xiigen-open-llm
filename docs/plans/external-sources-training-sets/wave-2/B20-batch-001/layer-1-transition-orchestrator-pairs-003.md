# Layer 1 — Transition-Orchestrator Pairs (003)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T003-001 — TransitionType: feature_addition | TEST only

Request: "Generate test cases for the DiscountEngine covering stacked promotions."
  TransitionType: feature_addition
  Evidence: "generate" → IMPLEMENT; "test cases" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41

---

## T003-002 — TransitionType: refactoring | IMPLEMENT only

Request: "Extract the CartService calculation logic into a pure function to enable isolated unit testing."
  TransitionType: refactoring
  Evidence: "extract" → refactoring; "unit testing" → TEST fires on "testing"
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  OrchestratorDecision: FeatureImplementationFlow41 (refactoring) + UnitTestingFlow51 | TransitionType: refactoring

---

## T003-003 — TransitionType: feature_addition | TEST+AUTHORIZATION

Request: "Add tests that verify the AdminPanel API denies requests without a valid bearer token."
  TransitionType: feature_addition
  Evidence: "add" → feature_addition; "tests" → TEST; "verify" → TEST (same predicate); "denies" + "bearer token" → AUTHORIZATION
  CompiledPredicateIds: ["TEST", "AUTHORIZATION"]
  OrchestratorDecision: UnitTestingFlow51 + AuthorizationTestingFlow52

---

## T003-004 — TransitionType: feature_addition | TEST+IMPLEMENT+DOCUMENT

Request: "Build a test harness for the ReportingService, implement the PDF formatter, and document the output schema."
  TransitionType: feature_addition
  Evidence: "build" → feature_addition; "test harness" → TEST via "test"; "implement" → IMPLEMENT; "document" → DOCUMENT; "schema" → borderline API_CONTRACT but "schema" alone is gap without REST/endpoint context
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "DOCUMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + DocumentationFlow62

---

## T003-005 — Gap path: zero predicates → SCHEDULE_CAPABILITY_LEARNING

Request: "Can you sanity check the new InventoryService logic before we merge?"
  TransitionType: N/A (gap)
  CompiledPredicateIds: []
  OrchestratorDecision: GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development")
  Teaching: "sanity check" is gap; "before we merge" is gap; no predicate fires.
