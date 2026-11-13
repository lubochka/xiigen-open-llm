# Layer 0-D — Flow-Factory Pairs (003)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F003-001 — TEST+API_CONTRACT → [UnitTestingFlow51, ContractTestingFlow33]

Input: "Assert the OrdersController returns 404 when the order ID is unknown."
  CompiledPredicateIds: ["TEST", "API_CONTRACT"] | SubPhases: [5.1, 3.3]
  Factory: FlowFactory.CreateMulti(["TEST","API_CONTRACT"]) → [UnitTestingFlow51, ContractTestingFlow33]
  Routing note: "assert" → TEST; REST response code → API_CONTRACT

---

## F003-002 — TEST+AUTHORIZATION → [UnitTestingFlow51, AuthorizationTestingFlow52]

Input: "Verify that non-admin users cannot call the DeleteOrder endpoint."
  CompiledPredicateIds: ["TEST", "AUTHORIZATION"] | SubPhases: [5.1, 5.2]
  Factory: FlowFactory.CreateMulti(["TEST","AUTHORIZATION"]) → [UnitTestingFlow51, AuthorizationTestingFlow52]
  Routing note: "verify" → TEST; "non-admin users cannot" → AUTHORIZATION; both route to 5.x tier

---

## F003-003 — TEST+IMPLEMENT+DOCUMENT → [UnitTestingFlow51, FeatureImplementationFlow41, DocumentationFlow62]

Input: "Write a test fixture for the InvoiceService, implement the missing factory method, and document the fixture setup."
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "DOCUMENT"] | SubPhases: [5.1, 4.1, 6.2]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT","DOCUMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41, DocumentationFlow62]
  ExecutionMode: sequential (IMPLEMENT before TEST, DOCUMENT last)

---

## F003-004 — TEST+QA → [UnitTestingFlow51, QAGatingFlow73]

Input: "Run the test suite for the ShippingModule and log QA results before the deploy."
  CompiledPredicateIds: ["TEST", "QA"] | SubPhases: [5.1, 7.3]
  Factory: FlowFactory.CreateMulti(["TEST","QA"]) → [UnitTestingFlow51, QAGatingFlow73]
  Routing note: "test suite" → TEST 5.1; "QA" abbreviation → QA 7.3

---

## F003-005 — TEST+ARCHITECTURE → [UnitTestingFlow51, ArchitectureFlow31]

Input: "Define the testing strategy for the PaymentModule and write the first unit tests."
  CompiledPredicateIds: ["TEST", "ARCHITECTURE"] | SubPhases: [5.1, 3.1]
  Factory: FlowFactory.CreateMulti(["TEST","ARCHITECTURE"]) → [UnitTestingFlow51, ArchitectureFlow31]
  Routing note: "testing strategy" fires TEST via "testing" + ARCHITECTURE via "define"
