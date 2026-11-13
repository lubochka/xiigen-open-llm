# Layer 0-A — Compiler Pairs (019)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: compiler

---

## P019-001 — TEST+IMPLEMENT: "write unit tests" for FraudScore

A: "Write unit tests for the FraudScoreCalculator and implement the test doubles for the external risk-scoring API (Application Programming Interface)."
  "write" → IMPLEMENT; "unit tests" → TEST; "implement" → IMPLEMENT (deduplicates); "test doubles" → TEST via "test"
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1
  Note: "test doubles" is test-infrastructure vocabulary that fires TEST. IMPLEMENT fires from both "write" and "implement".

---

## P019-002 — TEST: "assert" high-risk threshold

A: "Assert that the FraudScoreCalculator classifies a transaction as high-risk when the computed score exceeds 80."
  "assert" → TEST; "high-risk" — domain classification; "80" — domain threshold value
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P019-003 — TEST alone: "verify" velocity check

A: "Verify that the FraudScoreCalculator increases the score for accounts that place more than 5 orders within 10 minutes."
  "verify" → TEST; "velocity check" — domain security concept; "5 orders within 10 minutes" — domain rule
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P019-004 — TEST+IMPLEMENT: "develop tests" with mock

A: "Develop unit tests for FraudScoreCalculator with a mock device-fingerprint service to isolate the scoring logic."
  "develop" → IMPLEMENT; "unit tests" → TEST; "mock" → TEST (mock is test vocabulary); "isolate" — gap
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1

---

## P019-005 — TEST+IMPLEMENT: "create regression tests"

A: "Create regression tests for the FraudScoreCalculator to ensure that score adjustments introduced in v2.3 do not break existing thresholds."
  "create" → IMPLEMENT; "regression tests" → TEST via "tests" and "regression"; "regression" alone — gap, but "regression tests" compound fires TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1
  Boundary: "regression" alone is gap. "regression tests" as a compound fires TEST via "tests".
