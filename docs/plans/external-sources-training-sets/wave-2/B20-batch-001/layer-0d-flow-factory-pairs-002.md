# Layer 0-D — Flow-Factory Pairs (002)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F002-001 — Single TEST → UnitTestingFlow51

Input: "Write unit tests for the DiscountCalculationService."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  FlowClass: UnitTestingFlow51 | Domain: DiscountCalculationService

---

## F002-002 — TEST+IMPLEMENT → [UnitTestingFlow51, FeatureImplementationFlow41]

Input: "Add assertions to the existing CouponService tests and extend the service with a bulk-apply method."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  ExecutionMode: parallel (independent flows)

---

## F002-003 — TEST+SECURITY → [UnitTestingFlow51, SecurityTestingFlow52]

Input: "Write tests for the AuthTokenService and verify token forgery is rejected."
  CompiledPredicateIds: ["TEST", "SECURITY"] | SubPhases: [5.1, 5.2]
  Factory: FlowFactory.CreateMulti(["TEST","SECURITY"]) → [UnitTestingFlow51, SecurityTestingFlow52]
  Routing note: TEST context is unit-level → 5.1; SECURITY test context → 5.2

---

## F002-004 — TEST+DOCUMENT → [UnitTestingFlow51, DocumentationFlow62]

Input: "Spec out the test cases for the TaxEngine and document the expected rounding behaviour."
  CompiledPredicateIds: ["TEST", "DOCUMENT"] | SubPhases: [5.1, 6.2]
  Factory: FlowFactory.CreateMulti(["TEST","DOCUMENT"]) → [UnitTestingFlow51, DocumentationFlow62]
  TransitionType: documentation (dominant verb: "document")

---

## F002-005 — Zero predicates → GapHandler + SCHEDULE_CAPABILITY_LEARNING

Input: "Make sure the CheckoutService behaves correctly after the refactor."
  CompiledPredicateIds: [] | Result: knowledge_edge
  Factory: GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development")
  Teaching: "make sure" + "behaves correctly" are gap. No predicate fires on either phrase.
