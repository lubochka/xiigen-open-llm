# Layer 0-D — Flow Factory Pairs (055) — UnitConverter: factory selection for unit conversion domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F055-001 — TEST+IMPLEMENT: "implement UnitConverter and write test harness" → two instances

Request: "Implement the UnitConverter and write a test harness for metric to imperial length conversions."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F055-002 — TEST alone: "validate" on UnitConverter unsupported unit type → one UnitTestingFlow51, documentation

Request: "Validate that the UnitConverter throws UnsupportedConversionException for incompatible unit types."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F055-003 — Zero predicate: "convert metres to feet" → empty factory

Request: "Convert the measurement from metres to feet using the UnitConverter."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
