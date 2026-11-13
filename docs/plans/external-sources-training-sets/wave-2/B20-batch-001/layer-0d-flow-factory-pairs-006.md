# Layer 0-D — Flow-Factory Pairs (006)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F006-001 — TEST+IMPLEMENT via "write test" + "implement"

Input: "Write a failing test for the missing loyalty points expiry logic and implement the expiry handler."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  ExecutionMode: sequential (TEST first = failing test; IMPLEMENT second = make green)

---

## F006-002 — TEST only via "verify" + "assert"; no duplicate predicate

Input: "Verify and assert that the CartService correctly applies bulk discounts."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  Teaching: Multiple TEST triggers ("verify", "assert") produce ONE TEST predicate — no duplication.

---

## F006-003 — Gap: "double-check" + "sanity check" → GapHandler

Input: "Sanity check the OrderTotal calculation and double-check the rounding logic."
  CompiledPredicateIds: [] | Result: knowledge_edge
  Factory: GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development")
  Teaching: "sanity check" and "double-check" are both gap. Zero predicates fire.

---

## F006-004 — TEST+API_CONTRACT via "spec" + REST assertion

Input: "Spec the OrdersController endpoints and assert the 400 response for malformed payloads."
  CompiledPredicateIds: ["TEST", "API_CONTRACT"] | SubPhases: [5.1, 3.3]
  Factory: FlowFactory.CreateMulti(["TEST","API_CONTRACT"]) → [UnitTestingFlow51, ContractTestingFlow33]
  Evidence: "spec" → TEST; "assert" → TEST; REST response assertion → API_CONTRACT

---

## F006-005 — TEST+IMPLEMENT+API_CONTRACT via "write test" + "build mock" + "contract"

Input: "Write unit tests for the PaymentGatewayClient, build the mock contract, and implement the retry logic."
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "API_CONTRACT"] | SubPhases: [5.1, 4.1, 3.3]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT","API_CONTRACT"]) → [UnitTestingFlow51, FeatureImplementationFlow41, ContractTestingFlow33]
  Evidence: "write unit tests" → TEST; "build" → IMPLEMENT; "mock contract" → API_CONTRACT via "contract"
