# Layer 0-D — Flow-Factory Pairs (010)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F010-001 — TEST via "acceptance tests"; IMPLEMENT via "write"

Input: "Write acceptance tests for the OrderPlacement feature with the product owner."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  Evidence: "write" → IMPLEMENT; "acceptance tests" → TEST; "product owner" is project gap

---

## F010-002 — TEST+IMPLEMENT via "extend tests" + "implement handler"

Input: "Extend the DiscountService tests to cover promo stacking and implement the combined-discount handler."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  Evidence: "extend" → IMPLEMENT (extend = build on); "tests" → TEST

---

## F010-003 — TEST+SECURITY via "assert" + "encryption"

Input: "Assert that the UserProfileService encrypts PII before persisting to the database."
  CompiledPredicateIds: ["TEST", "SECURITY"] | SubPhases: [5.1, 5.2]
  Factory: FlowFactory.CreateMulti(["TEST","SECURITY"]) → [UnitTestingFlow51, SecurityTestingFlow52]
  Evidence: "assert" → TEST; "encrypts PII" → SECURITY (data encryption security action)

---

## F010-004 — TEST only via "test data"; IMPLEMENT does not fire on "set up"

Input: "Set up test data for the InvoiceService to cover zero-amount and negative-balance scenarios."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  Evidence: "set up" is gap; "test data" → TEST. IMPLEMENT does not fire on "set up".

---

## F010-005 — TEST+IMPLEMENT+DOCUMENT+API_CONTRACT — 4-predicate flow

Input: "Write integration tests for the ExternalPaymentGateway, implement the retry wrapper, document the contract, and spec the error response schema."
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "DOCUMENT", "API_CONTRACT"] | SubPhases: [5.1, 4.1, 6.2, 3.3]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT","DOCUMENT","API_CONTRACT"]) → 4 flows
  Evidence: "write tests" → TEST+IMPLEMENT; "document" → DOCUMENT; "spec the error response schema" → API_CONTRACT via "spec"+"schema"
