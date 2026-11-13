# Layer 0-D — Flow Factory Pairs (050) — RegexValidator: factory selection for regex domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F050-001 — TEST+IMPLEMENT: "build RegexValidator and add test cases" → two instances

Request: "Build the RegexValidator and add test cases for email, phone, and postal code patterns."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F050-002 — TEST alone: "spec" on RegexValidator → one UnitTestingFlow51, documentation

Request: "Spec the RegexValidator behaviour for strings at the maximum allowed length."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F050-003 — Zero predicate: "apply regex rules" → empty factory

Request: "Apply the regex rules to sanitise user input before storing it."
  "apply" (standalone) → NOT in IMPLEMENT lexicon.
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
