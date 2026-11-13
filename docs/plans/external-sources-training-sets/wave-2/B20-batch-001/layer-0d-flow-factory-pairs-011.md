# Layer 0-D — Flow-Factory Pairs (011)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F011-001 — TEST only via "flaky test"; "fix" does not fire IMPLEMENT

Input: "Fix the flaky test in the CartService integration test suite."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  Teaching: "fix" does NOT fire IMPLEMENT. "flaky test" fires TEST via "test".

---

## F011-002 — TEST+IMPLEMENT via "test plan" + "create"

Input: "Create a test plan for the BillingModule covering all payment edge scenarios."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  Evidence: "create" → IMPLEMENT; "test plan" → TEST via "test"

---

## F011-003 — TEST fires on "test suite"; no predicate from "review"

Input: "Review the test suite for the UserManagement module before merging."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  Teaching: "review" is gap. "test suite" fires TEST via "test".

---

## F011-004 — TEST+IMPLEMENT via "develop" (TDD) + "test-driven"

Input: "Develop the ReturnsService test-driven with full branch coverage."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  Evidence: "develop" → IMPLEMENT; "test-driven" → TEST via "test" component; "coverage" alone is gap but doesn't suppress

---

## F011-005 — Gap: "expected behaviour" + "define" → ARCHITECTURE only

Input: "Define the expected behaviour of the RefundService for partial refund scenarios."
  CompiledPredicateIds: ["ARCHITECTURE"] | SubPhase: 3.1
  Factory: FlowFactory.Create("ARCHITECTURE", subPhase: "3.1") → ArchitectureFlow31
  Teaching: "expected behaviour" does NOT fire TEST. "define" fires ARCHITECTURE. Single-predicate result.
