# Layer 0-D — Flow-Factory Pairs (004)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F004-001 — TEST+SECURITY+AUTHORIZATION → [UnitTestingFlow51, SecurityTestingFlow52, AuthorizationTestingFlow52]

Input: "Write tests that assert the JwtValidator rejects expired tokens and denies access to protected routes."
  CompiledPredicateIds: ["TEST", "SECURITY", "AUTHORIZATION"] | SubPhases: [5.1, 5.2, 5.2]
  Factory: FlowFactory.CreateMulti(["TEST","SECURITY","AUTHORIZATION"]) → [UnitTestingFlow51, SecurityTestingFlow52, AuthorizationTestingFlow52]
  Note: SECURITY and AUTHORIZATION both route to 5.2; factory emits two sub-flows at same tier

---

## F004-002 — IMPLEMENT only (no TEST) — refactoring TransitionType

Input: "Refactor the LegacyPaymentProcessor to make it unit testable."
  CompiledPredicateIds: ["IMPLEMENT"] | SubPhase: 4.1 | TransitionType: refactoring
  Factory: FlowFactory.Create("IMPLEMENT", subPhase: "4.1", transitionType: "refactoring") → FeatureImplementationFlow41
  Teaching: "testable" adjective does NOT fire TEST. IMPLEMENT fires on "refactor".

---

## F004-003 — TEST fires via "spec"; IMPLEMENT fires via "create"

Input: "Create a spec for the ProductCatalogService covering all CRUD operations."
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]
  Factory: FlowFactory.CreateMulti(["IMPLEMENT","TEST"]) → [FeatureImplementationFlow41, UnitTestingFlow51]
  Evidence: "create" → IMPLEMENT; "spec" → TEST

---

## F004-004 — TEST fires via "test cases"; IMPLEMENT fires via "generate"

Input: "Generate test cases for the PromotionEngine boundary conditions."
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]
  Factory: FlowFactory.CreateMulti(["IMPLEMENT","TEST"]) → [FeatureImplementationFlow41, UnitTestingFlow51]
  Evidence: "generate" → IMPLEMENT; "test cases" → TEST

---

## F004-005 — Four-predicate: TEST+IMPLEMENT+SECURITY+DOCUMENT

Input: "Write unit tests for the EncryptionService, implement the AES-256 mode, document the API, and verify no plaintext leaks."
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "SECURITY", "DOCUMENT"] | SubPhases: [5.1, 4.1, 5.2, 6.2]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT","SECURITY","DOCUMENT"]) → 4 flows
  ExecutionMode: sequential (IMPLEMENT → TEST → SECURITY validation → DOCUMENT)
