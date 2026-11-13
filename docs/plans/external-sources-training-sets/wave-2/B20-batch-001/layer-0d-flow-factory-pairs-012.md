# Layer 0-D — Flow-Factory Pairs (012)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F012-001 — TEST only via "assert"; performance domain does not add predicate

Input: "Assert that the SearchService responds within 200ms for catalog queries up to 10,000 products."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  Teaching: Performance threshold ("200ms", "10,000 products") is domain gap. TEST fires on "assert".

---

## F012-002 — TEST+IMPLEMENT via "build" + "test data builder"

Input: "Build a test data builder for the CheckoutService covering domestic and international order scenarios."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  Evidence: "build" → IMPLEMENT; "test data builder" → TEST via "test data"

---

## F012-003 — TEST+QA via "test suite" + "QA gate"

Input: "Run the test suite for the PaymentModule and confirm QA sign-off before the hotfix deploy."
  CompiledPredicateIds: ["TEST", "QA"] | SubPhases: [5.1, 7.3]
  Factory: FlowFactory.CreateMulti(["TEST","QA"]) → [UnitTestingFlow51, QAGatingFlow73]
  Evidence: "test suite" → TEST; "QA" → QA abbreviation fires; "confirm" is gap; "deploy" is gap

---

## F012-004 — TEST+IMPLEMENT+SECURITY via JWT test + implement validator + verify no bypass

Input: "Write tests for the JwtRefreshService, implement the token rotation logic, and verify refresh tokens cannot be replayed."
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "SECURITY"] | SubPhases: [5.1, 4.1, 5.2]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT","SECURITY"]) → [UnitTestingFlow51, FeatureImplementationFlow41, SecurityTestingFlow52]
  Evidence: "write tests" → TEST+IMPLEMENT; "verify" → TEST; "replayed" → SECURITY (replay attack)

---

## F012-005 — Gap: "make all tests pass" with no action verb

Input: "All tests should be passing for the InvoiceService before the code review."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  Teaching: "tests" fires TEST. "should be passing" is a state phrase — gap on its own but doesn't suppress TEST on "tests".
