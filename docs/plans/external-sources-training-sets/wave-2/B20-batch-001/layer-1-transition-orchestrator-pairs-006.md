# Layer 1 — Transition-Orchestrator Pairs (006)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## T006-001 — TransitionType: feature_addition | TEST via "spec"

Request: "Spec the error handling behaviour of the NotificationService when the SMTP host is unavailable."
  TransitionType: feature_addition
  Evidence: "spec" → IMPLEMENT (create a spec = feature_addition); "spec" also → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41

---

## T006-002 — TransitionType: refactoring | IMPLEMENT+TEST via "refactor"+"verify"

Request: "Refactor the UserService authentication flow and verify the session token logic still works."
  TransitionType: refactoring
  Evidence: "refactor" → refactoring; "verify" → TEST
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  OrchestratorDecision: FeatureImplementationFlow41 (refactoring) + UnitTestingFlow51

---

## T006-003 — TransitionType: feature_addition | TEST+IMPLEMENT+AUTHORIZATION

Request: "Implement a permission check for the OrderDeletion endpoint and write tests asserting unauthorized calls return 403."
  TransitionType: feature_addition
  Evidence: "implement" → IMPLEMENT (feature_addition); "write tests" → TEST+IMPLEMENT; "unauthorized" → AUTHORIZATION; "403" → API_CONTRACT context but "403" alone doesn't fire API_CONTRACT without REST verb
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "AUTHORIZATION"]
  OrchestratorDecision: FeatureImplementationFlow41 + UnitTestingFlow51 + AuthorizationTestingFlow52

---

## T006-004 — TransitionType: feature_addition | IMPLEMENT+TEST+API_CONTRACT

Request: "Generate stubs for the ShippingProviderClient, write contract tests, and implement the rate-lookup method."
  TransitionType: feature_addition
  Evidence: "generate" → IMPLEMENT; "contract tests" → TEST+API_CONTRACT; "implement" → IMPLEMENT
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "API_CONTRACT"]
  OrchestratorDecision: UnitTestingFlow51 + FeatureImplementationFlow41 + ContractTestingFlow33

---

## T006-005 — TransitionType: documentation | ARCHITECTURE+TEST+DOCUMENT (4 predicates)

Request: "Design the testing architecture for the OrderModule, write the initial test specs, and document the approach."
  TransitionType: documentation
  Evidence: "design" → ARCHITECTURE (documentation transitionType); "testing architecture" → TEST+ARCHITECTURE; "write specs" → TEST+IMPLEMENT; "document" → DOCUMENT
  CompiledPredicateIds: ["ARCHITECTURE", "TEST", "IMPLEMENT", "DOCUMENT"]
  OrchestratorDecision: ArchitectureFlow31 + UnitTestingFlow51 + FeatureImplementationFlow41 + DocumentationFlow62
