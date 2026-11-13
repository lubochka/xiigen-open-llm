# Layer 0-D — Flow Factory Pairs (053) — CurrencyFormatter: factory selection for currency domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F053-001 — TEST+IMPLEMENT: "create CurrencyFormatter and write test suite" → two instances

Request: "Create the CurrencyFormatter and write a test suite covering GBP (Great British Pound), EUR (Euro), and USD (US Dollar)."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F053-002 — TEST alone: "validate" on CurrencyFormatter negative amounts → one UnitTestingFlow51, documentation

Request: "Validate that the CurrencyFormatter correctly displays negative amounts with a leading minus sign."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F053-003 — Zero predicate: "format currency value" → empty factory

Request: "Format the currency value to two decimal places using the CurrencyFormatter."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
