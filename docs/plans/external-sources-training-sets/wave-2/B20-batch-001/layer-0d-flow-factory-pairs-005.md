# Layer 0-D — Flow-Factory Pairs (005)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F005-001 — TEST only via "validate"

Input: "Validate the output of the TaxCalculationService for EU VAT rates."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  Evidence: "validate" always fires TEST in any context

---

## F005-002 — TEST only via "assert"; domain is security but not SECURITY predicate

Input: "Assert that the PasswordHashingService never stores plaintext."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  Teaching: "assert" fires TEST. Security domain content does not fire SECURITY without a security-action verb.

---

## F005-003 — IMPLEMENT only via "develop"; no TEST from "testable"

Input: "Develop the RefundService to be fully unit testable with no external dependencies."
  CompiledPredicateIds: ["IMPLEMENT"] | SubPhase: 4.1 | TransitionType: feature_addition
  Factory: FlowFactory.Create("IMPLEMENT", subPhase: "4.1") → FeatureImplementationFlow41
  Teaching: "testable" adjective gap; "develop" fires IMPLEMENT.

---

## F005-004 — TEST fires via "test data"; IMPLEMENT fires via "build"

Input: "Build test data generators for the OrderService integration tests."
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]
  Factory: FlowFactory.CreateMulti(["IMPLEMENT","TEST"]) → [FeatureImplementationFlow41, UnitTestingFlow51]
  Evidence: "build" → IMPLEMENT; "test data" → TEST; "integration tests" → TEST (duplicate)

---

## F005-005 — ARCHITECTURE+TEST via "design testing approach"

Input: "Design the testing approach for the new EventSourcing module."
  CompiledPredicateIds: ["ARCHITECTURE", "TEST"] | SubPhases: [3.1, 5.1]
  Factory: FlowFactory.CreateMulti(["ARCHITECTURE","TEST"]) → [ArchitectureFlow31, UnitTestingFlow51]
  Evidence: "design" → ARCHITECTURE; "testing" → TEST; TransitionType: documentation
