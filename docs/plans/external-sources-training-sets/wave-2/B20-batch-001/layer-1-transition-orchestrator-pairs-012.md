# Layer 1 — Transition-Orchestrator Pairs (012)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T012-001 — TransitionType: feature_addition | TEST via "flaky test" fix context

Request: "Investigate the flaky tests in the CartService and stabilize them."
  TransitionType: feature_addition
  Evidence: "flaky tests" → TEST via "tests"; "investigate" is gap; "stabilize" is gap
  CompiledPredicateIds: ["TEST"]
  OrchestratorDecision: UnitTestingFlow51 | Single predicate; gap verbs don't suppress TEST on object noun

---

## T012-002 — TransitionType: feature_addition | TEST+IMPLEMENT via "develop"+"test-driven"

Request: "Develop the SubscriptionRenewalService test-driven with failure scenario coverage."
  TransitionType: feature_addition
  Evidence: "develop" → IMPLEMENT (feature_addition); "test-driven" → TEST via "test"
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41

---

## T012-003 — TransitionType: feature_addition | TEST+IMPLEMENT+API_CONTRACT via "spec"+"REST"

Request: "Write unit tests for the OrdersController, implement the PATCH handler, and spec the request/response schema."
  TransitionType: feature_addition
  Evidence: "write unit tests" → TEST+IMPLEMENT; "implement" → IMPLEMENT; "spec the schema" → API_CONTRACT via "spec"+"schema"
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "API_CONTRACT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + ContractTestingFlow33

---

## T012-004 — TransitionType: documentation | ARCHITECTURE+TEST+DOCUMENT

Request: "Diagram the unit testing approach for the Payments subdomain and document the isolation strategy."
  TransitionType: documentation
  Evidence: "diagram" → ARCHITECTURE (documentation transitionType); "unit testing" → TEST; "document" → DOCUMENT
  CompiledPredicateIds: ["ARCHITECTURE", "TEST", "DOCUMENT"]
  OrchestratorDecision: ArchitectureFlow31 + UnitTestingFlow51 + DocumentationFlow62

---

## T012-005 — Gap: pure BDD Given/When/Then + no "test" word

Request: "Given the LoyaltyService has 1000 points, When redeem is called for 500, Then balance should be 500."
  TransitionType: N/A (gap)
  CompiledPredicateIds: []
  OrchestratorDecision: GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development")
  Teaching: Pure BDD syntax without "test" fires zero predicates. No TransitionType assigned.
