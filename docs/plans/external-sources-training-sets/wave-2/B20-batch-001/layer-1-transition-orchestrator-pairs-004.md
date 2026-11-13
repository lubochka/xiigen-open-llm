# Layer 1 — Transition-Orchestrator Pairs (004)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T004-001 — TransitionType: feature_addition | TEST+IMPLEMENT via "write"+"unit test"

Request: "Write unit tests for the OrderValidationService covering all required field checks."
  TransitionType: feature_addition
  Evidence: "write" → IMPLEMENT (feature_addition); "unit tests" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41

---

## T004-002 — TransitionType: refactoring | TEST+IMPLEMENT via "refactor"

Request: "Refactor the PaymentGateway adapter and add integration tests to confirm the refactored interface."
  TransitionType: refactoring
  Evidence: "refactor" → refactoring; "add" also fires IMPLEMENT but transitionType stays refactoring (first verb wins); "integration tests" → TEST
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  OrchestratorDecision: FeatureImplementationFlow41 (refactoring) + UnitTestingFlow51

---

## T004-003 — TransitionType: feature_addition | TEST+SECURITY via "assert"+"injection"

Request: "Assert that the ProductSearchService sanitizes XSS (Cross-Site Scripting) attempts in the query string."
  TransitionType: feature_addition
  Evidence: "assert" fires TEST; "XSS"/"Cross-Site Scripting" → SECURITY
  CompiledPredicateIds: ["TEST", "SECURITY"]
  OrchestratorDecision: UnitTestingFlow51 + SecurityTestingFlow52

---

## T004-004 — TransitionType: documentation | ARCHITECTURE+TEST

Request: "Design and document the unit test architecture for the new Payments subdomain."
  TransitionType: documentation
  Evidence: "design" → documentation transitionType; "document" → documentation; "unit test architecture" → TEST via "test" + ARCHITECTURE via "architecture"
  CompiledPredicateIds: ["ARCHITECTURE", "TEST", "DOCUMENT"]
  OrchestratorDecision: ArchitectureFlow31 + UnitTestingFlow51 + DocumentationFlow62

---

## T004-005 — TransitionType: feature_addition | TEST+IMPLEMENT+QA

Request: "Create unit tests for the InvoiceService, implement the rounding fix, and log QA results."
  TransitionType: feature_addition
  Evidence: "create" → IMPLEMENT (feature_addition); "unit tests" → TEST; "QA" → QA
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "QA"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + QAGatingFlow73
