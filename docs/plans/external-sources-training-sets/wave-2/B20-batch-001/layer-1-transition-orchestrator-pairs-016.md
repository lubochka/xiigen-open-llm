# Layer 1 — Transition Orchestrator Pairs (016)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T016-001 — TEST+IMPLEMENT → feature_addition (TaxCalculationTests)

A:
  compiled_text: "Write unit tests for the TaxCalculationService to cover VAT rates for EU countries, US state tax, and zero-rated goods."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'write' fires IMPLEMENT — construction verb"
    - "'unit tests' fires TEST — new test suite = feature_addition"
    - "Multi-jurisdiction tax coverage is net-new (EU VAT, US state, zero-rated)"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

---

## T016-002 — TEST+IMPLEMENT → feature_addition (test harness with stubs)

A:
  compiled_text: "Create a test harness for the TaxCalculationService that stubs the tax-rate repository and validates output per region."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'create' fires IMPLEMENT — construction verb"
    - "'test harness' + 'stubs' + 'validates' fire TEST — test infrastructure = feature_addition"
    - "Harness is a new artifact being constructed"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92
  Note: Creating a test harness is feature_addition — it adds new test infrastructure. It is not refactoring (no existing code is restructured) and not documentation (a harness is executable code).
