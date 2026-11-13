# Layer 0-D — Flow Factory Pairs (049) — StringParser: factory selection for string-processing domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F049-001 — TEST+IMPLEMENT: "create StringParser and write unit tests" → two instances

Request: "Create the StringParser class and write unit tests for delimiter-based splitting."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F049-002 — TEST alone: "validate" on StringParser → one UnitTestingFlow51, documentation

Request: "Validate that the StringParser returns an empty collection when given an empty string."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F049-003 — Zero predicate: "parse the input string" → empty factory

Request: "Parse the input string using the configured delimiter in the StringParser."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
