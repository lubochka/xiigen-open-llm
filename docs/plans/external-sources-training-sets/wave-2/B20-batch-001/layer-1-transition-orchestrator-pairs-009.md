# Layer 1 — Transition-Orchestrator Pairs (009)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T009-001 — TransitionType: feature_addition | TEST via "assert"; performance threshold is gap

Request: "Assert that the ProductSearchService returns results within 150ms for up to 50,000 products."
  TransitionType: feature_addition
  Evidence: "assert" → TEST; "150ms" is gap (performance threshold); "50,000 products" is domain gap
  CompiledPredicateIds: ["TEST"]
  OrchestratorDecision: UnitTestingFlow51 | Performance threshold does not add predicate

---

## T009-002 — TransitionType: feature_addition | TEST+IMPLEMENT via "create"+"test cases"

Request: "Create parameterized test cases for the DiscountEngine covering all tier combinations."
  TransitionType: feature_addition
  Evidence: "create" → IMPLEMENT; "test cases" → TEST; "parameterized" is gap (test framework keyword)
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41

---

## T009-003 — TransitionType: feature_addition | TEST+IMPLEMENT+SECURITY via AES test

Request: "Implement AES-256 (Advanced Encryption Standard, 256-bit) key rotation in the DataVaultService and write tests asserting old keys are invalidated."
  TransitionType: feature_addition
  Evidence: "implement" → IMPLEMENT; "write tests" → TEST+IMPLEMENT; "AES-256 key rotation" → SECURITY
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "SECURITY"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + SecurityTestingFlow52

---

## T009-004 — TransitionType: documentation | ARCHITECTURE only

Request: "Define the testing boundaries between the OrderService and the external InventoryAdapter."
  TransitionType: documentation
  Evidence: "define" → ARCHITECTURE (documentation transitionType); "testing boundaries" — "testing" fires TEST via gerund
  CompiledPredicateIds: ["ARCHITECTURE", "TEST"]
  OrchestratorDecision: ArchitectureFlow31 + UnitTestingFlow51

---

## T009-005 — Gap path: "ensure" → zero predicates

Request: "Ensure the ShippingService is fully tested before the Q4 release."
  TransitionType: N/A (gap)
  CompiledPredicateIds: ["TEST"]
  Note: "tested" fires TEST via past-participle form of "test"; "ensure" is gap; "Q4 release" is gap
  OrchestratorDecision: UnitTestingFlow51
  Teaching: Even "ensure X is tested" fires TEST on "tested". "ensure" is still gap but doesn't suppress.
