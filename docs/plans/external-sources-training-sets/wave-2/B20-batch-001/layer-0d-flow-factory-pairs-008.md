# Layer 0-D — Flow-Factory Pairs (008)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F008-001 — TEST only via "test double" (canonical TEST trigger)

Input: "Use a test double to isolate the NotificationService from the EmailProvider."
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Factory: FlowFactory.Create("TEST", subPhase: "5.1") → UnitTestingFlow51
  Evidence: "test double" fires TEST. "use" is gap (not in IMPLEMENT lexicon).

---

## F008-002 — TEST+IMPLEMENT via "implement" + "test fixture"

Input: "Implement a test fixture that bootstraps the OrderService with seeded repository data."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  Evidence: "implement" → IMPLEMENT; "test fixture" → TEST

---

## F008-003 — TEST+DOCUMENT via "test plan" + "document"

Input: "Document the test plan for the new CustomerSegmentation feature."
  CompiledPredicateIds: ["TEST", "DOCUMENT"] | SubPhases: [5.1, 6.2]
  Factory: FlowFactory.CreateMulti(["TEST","DOCUMENT"]) → [UnitTestingFlow51, DocumentationFlow62]
  Evidence: "test plan" → TEST via "test"; "document" → DOCUMENT; TransitionType: documentation

---

## F008-004 — TEST fires via "mutation testing"; IMPLEMENT fires via "apply"

Input: "Apply mutation testing to the DiscountService to strengthen the test suite."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhases: [5.1, 4.1]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51, FeatureImplementationFlow41]
  Evidence: "apply" → IMPLEMENT; "mutation testing" → TEST via "testing"

---

## F008-005 — Three-predicate TEST+IMPLEMENT+AUTHORIZATION

Input: "Write tests for the RoleService, implement the deny-by-default rule, and verify admin override is blocked for non-superusers."
  CompiledPredicateIds: ["TEST", "IMPLEMENT", "AUTHORIZATION"] | SubPhases: [5.1, 4.1, 5.2]
  Factory: FlowFactory.CreateMulti(["TEST","IMPLEMENT","AUTHORIZATION"]) → [UnitTestingFlow51, FeatureImplementationFlow41, AuthorizationTestingFlow52]
  ExecutionMode: sequential
