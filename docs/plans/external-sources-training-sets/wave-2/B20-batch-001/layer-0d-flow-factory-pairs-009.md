# Layer 0-D — Flow-Factory Pairs (009)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F009-001 — TEST only via "verify"; concurrency domain gap

Input: "Verify that the InventoryService handles concurrent stock deductions without race conditions."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  Evidence: "verify" → TEST; "concurrent"/"race conditions" are domain gap terms

---

## F009-002 — TEST only via "negative test case"

Input: "Add a negative test case for the LoginService when the session token has expired."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  Evidence: "add" → IMPLEMENT; "negative test case" → TEST via "test case"

---

## F009-003 — IMPLEMENT fires via "generate"; TEST fires via "test cases"

Input: "Generate boundary-value test cases for the ShippingCostCalculator."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  Evidence: "generate" → IMPLEMENT; "test cases" → TEST

---

## F009-004 — TEST only via "testing" gerund

Input: "Testing the EmailDispatcher retry logic is blocked by the missing SMTP mock."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  Teaching: "testing" gerund fires TEST. "blocked" is state description; no additional predicate.

---

## F009-005 — Gap: "improve" + "code quality" → GapHandler

Input: "Improve the code quality of the SearchService before the release."
  CompiledPredicateIds: [] | Result: knowledge_edge
  Factory: GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development")
  Teaching: "improve" is gap; "code quality" is gap; "release" is gap. Zero predicates.
