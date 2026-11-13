# Layer 1 — Transition-Orchestrator Pairs (002)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T002-001 — TransitionType: feature_addition | TEST+IMPLEMENT

Request: "Write unit tests for the LoyaltyPointsService and add the expiry calculation."
  TransitionType: feature_addition
  Evidence: "add" → feature_addition (new capability); "write" is IMPLEMENT-adjacent but transitionType set by primary action verb
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 | ExecutionMode: parallel

---

## T002-002 — TransitionType: refactoring | IMPLEMENT only

Request: "Refactor the OrderProcessor so its dependencies can be mocked in unit tests."
  TransitionType: refactoring
  Evidence: "refactor" → refactoring; "mocked" is gap; "unit tests" fires TEST
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  OrchestratorDecision: FeatureImplementationFlow41 (refactoring) + UnitTestingFlow51 | TransitionType: refactoring

---

## T002-003 — TransitionType: documentation | TEST+DOCUMENT+ARCHITECTURE

Request: "Design the unit-test strategy for the BillingModule and document the test boundaries."
  TransitionType: documentation
  Evidence: "design" → documentation transitionType; "test strategy" → TEST; "document" → DOCUMENT
  CompiledPredicateIds: ["ARCHITECTURE", "TEST", "DOCUMENT"]
  OrchestratorDecision: ArchitectureFlow31 + UnitTestingFlow51 + DocumentationFlow62

---

## T002-004 — TransitionType: feature_addition | TEST+SECURITY+IMPLEMENT

Request: "Implement input sanitization for the SearchService and write tests that verify SQL injection is blocked."
  TransitionType: feature_addition
  Evidence: "implement" → feature_addition; "write tests" → TEST+IMPLEMENT; "SQL injection" → SECURITY
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "SECURITY"]
  OrchestratorDecision: FeatureImplementationFlow41 + UnitTestingFlow51 + SecurityTestingFlow52

---

## T002-005 — TransitionType: feature_addition | TEST+IMPLEMENT+API_CONTRACT

Request: "Create unit tests for the PaymentController REST endpoints and implement the missing DELETE handler."
  TransitionType: feature_addition
  Evidence: "create" → feature_addition; "unit tests" → TEST; REST endpoints → API_CONTRACT; "implement" → IMPLEMENT
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "API_CONTRACT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + ContractTestingFlow33
