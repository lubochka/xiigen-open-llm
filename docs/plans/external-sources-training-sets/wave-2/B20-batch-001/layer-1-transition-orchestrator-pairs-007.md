# Layer 1 — Transition-Orchestrator Pairs (007)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T007-001 — TransitionType: feature_addition | TEST via "validate"

Request: "Validate the output of the TaxEngine for all EU VAT rate bands."
  TransitionType: feature_addition
  Evidence: "validate" → TEST (always, any context); no IMPLEMENT verb present
  CompiledPredicateIds: ["TEST"]
  OrchestratorDecision: UnitTestingFlow51 | Single predicate

---

## T007-002 — TransitionType: feature_addition | TEST+IMPLEMENT via "add"+"assertion"

Request: "Add an assertion that the CurrencyConversionService never returns a negative exchange rate."
  TransitionType: feature_addition
  Evidence: "add" → IMPLEMENT (feature_addition); "assertion" → TEST via "assert" root
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41

---

## T007-003 — TransitionType: feature_addition | TEST+SECURITY via "assert"+"password"

Request: "Assert that the UserRegistrationService enforces minimum password complexity rules."
  TransitionType: feature_addition
  Evidence: "assert" → TEST; "password complexity" → SECURITY (authentication security context)
  CompiledPredicateIds: ["TEST", "SECURITY"]
  OrchestratorDecision: UnitTestingFlow51 + SecurityTestingFlow52

---

## T007-004 — TransitionType: refactoring | IMPLEMENT+TEST via "extract"+"tests"

Request: "Extract the order validation rules into a dedicated validator and add regression tests."
  TransitionType: refactoring
  Evidence: "extract" → refactoring; "add" → IMPLEMENT (secondary, transitionType set by "extract"); "regression tests" → TEST via "tests"
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  OrchestratorDecision: FeatureImplementationFlow41 (refactoring) + UnitTestingFlow51

---

## T007-005 — TransitionType: feature_addition | TEST+IMPLEMENT+QA+DOCUMENT

Request: "Create unit tests for the ShippingCalculator, implement the weight-bracket logic, document the algorithm, and log QA approval."
  TransitionType: feature_addition
  Evidence: "create" → IMPLEMENT (feature_addition); "unit tests" → TEST; "implement" → IMPLEMENT; "document" → DOCUMENT; "QA" → QA
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "DOCUMENT", "QA"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + DocumentationFlow62 + QAGatingFlow73
