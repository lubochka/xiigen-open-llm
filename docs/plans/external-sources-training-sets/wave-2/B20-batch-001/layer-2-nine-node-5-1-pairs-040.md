# Layer 2 — Nine-Node Flow Pairs (040)
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N040 — UnitTestingFlow51: TimeZoneConverter

Request: "Write unit tests for the TimeZoneConverter covering UTC (Coordinated Universal Time) offset accuracy and DST (Daylight Saving Time) transitions."

### Node 01 — SelectedLogicalTransition
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1 | TransitionType: feature_addition

### Node 02 — Context Assembly
  Service: TimeZoneConverter | Domain: utility/datetime
  Dependencies identified: ITimeZoneProvider (injectable for DST (Daylight Saving Time) data)
  Testable units: Convert(DateTime utcTime, string targetZone), GetOffset(string zone, DateTime at)

### Node 03 — Plan Generation
  3.1 Convert UTC (Coordinated Universal Time) to GMT (Greenwich Mean Time) -5 (New York) → expect 5-hour offset
  3.2 Convert UTC (Coordinated Universal Time) during DST (Daylight Saving Time) period → expect 4-hour offset (not 5)
  3.3 Convert UTC (Coordinated Universal Time) to UTC (Coordinated Universal Time) → expect unchanged
  3.4 Invalid timezone ID → expect UnknownTimeZoneException
  3.5 Convert DateTime.MinValue edge case → expect no overflow exception

### Node 04 — Dependency Resolution
  Mock: MockTimeZoneProvider (controls DST (Daylight Saving Time) boundary). Test framework: xUnit.

### Node 05 — Implementation (Test Code)
  Tests produced: 9 | Framework: xUnit
  Coverage: offset calculations (3 cases), DST (Daylight Saving Time) transitions (2 cases), edge cases (4 cases)

### Node 06 — Verification
  All 9 tests pass | No skipped tests

### Node 07 — Quality Gate
  Status: PASS | Coverage: 88% statement coverage

### Node 08 — Output Assembly
  OutputType: TestSuite | Files: TimeZoneConverterTests.cs

### Node 09 — Result
  Result: PASS | Tests: 9 | Status: green
