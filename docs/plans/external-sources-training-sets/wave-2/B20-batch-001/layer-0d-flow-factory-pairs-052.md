# Layer 0-D — Flow Factory Pairs (052) — TimeZoneConverter: factory selection for timezone domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F052-001 — TEST+IMPLEMENT: "extend TimeZoneConverter and add regression tests" → two instances

Request: "Extend the TimeZoneConverter to support IANA (Internet Assigned Numbers Authority) timezone IDs and add regression tests."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F052-002 — TEST alone: "assert" on TimeZoneConverter DST (Daylight Saving Time) → one UnitTestingFlow51, documentation

Request: "Assert that the TimeZoneConverter handles DST (Daylight Saving Time) transitions without a 1-hour offset error."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F052-003 — Zero predicate: "convert timezone" → empty factory

Request: "Convert the meeting timestamp from UTC (Coordinated Universal Time) to Tokyo Standard Time."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
