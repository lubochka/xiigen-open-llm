# Layer 1 — Transition-Orchestrator Pairs (005)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T005-001 — TransitionType: feature_addition | IMPLEMENT+TEST via "build"+"test data"

Request: "Build test data fixtures for the CustomerService covering edge cases in address parsing."
  TransitionType: feature_addition
  Evidence: "build" → IMPLEMENT (feature_addition); "test data" → TEST; "edge cases" is gap alone but doesn't suppress
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41

---

## T005-002 — TransitionType: feature_addition | TEST+IMPLEMENT+SECURITY via JWT expiry

Request: "Write tests asserting the AuthService rejects expired JWTs (JSON Web Tokens) and implement the expiry check."
  TransitionType: feature_addition
  Evidence: "write" → IMPLEMENT; "tests asserting" → TEST+IMPLEMENT; "rejects expired JWTs" → SECURITY
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "SECURITY"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + SecurityTestingFlow52

---

## T005-003 — TransitionType: documentation | TEST+DOCUMENT via "test plan"+"document"

Request: "Document a test plan for the CheckoutService covering all payment method combinations."
  TransitionType: documentation
  Evidence: "document" → DOCUMENT (documentation transitionType); "test plan" → TEST via "test"
  CompiledPredicateIds: ["TEST", "DOCUMENT"]
  OrchestratorDecision: UnitTestingFlow51 + DocumentationFlow62 | TransitionType: documentation

---

## T005-004 — TransitionType: refactoring | IMPLEMENT+TEST via "extract"+"test"

Request: "Extract the pricing rules from the CartService and write unit tests for the extracted PricingRulesEngine."
  TransitionType: refactoring
  Evidence: "extract" → refactoring; "write unit tests" → IMPLEMENT+TEST
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  OrchestratorDecision: FeatureImplementationFlow41 (refactoring) + UnitTestingFlow51

---

## T005-005 — Gap: "expected behaviour" fires ARCHITECTURE not TEST

Request: "Define the expected behaviour of the ShippingService when carrier API is unreachable."
  TransitionType: documentation
  Evidence: "define" → ARCHITECTURE (documentation transitionType); "expected behaviour" is gap for TEST
  CompiledPredicateIds: ["ARCHITECTURE"]
  OrchestratorDecision: ArchitectureFlow31 | Single predicate; no TEST
