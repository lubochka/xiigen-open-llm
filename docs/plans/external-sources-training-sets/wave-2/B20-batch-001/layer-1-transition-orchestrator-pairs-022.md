# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T022-001 — TEST+IMPLEMENT → feature_addition (PriceFormatterTests)

A:
  compiled_text: "Write unit tests for the PriceFormatter verifying currency symbols, decimal places, and locale-specific separators."
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'write' fires IMPLEMENT"
    - "'unit tests' + 'verifying' fire TEST"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

---

## T022-002 — TEST alone → documentation (validate negative price)

A:
  compiled_text: "Validate that the PriceFormatter wraps negative prices in parentheses for accountancy display mode."
  compiled_predicate_ids: ["TEST"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: documentation
  TransitionEvidence:
    - "'validate' fires TEST (universal); no IMPLEMENT verb → documentation"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.88

---

## T022-003 — ZERO predicates → no TransitionType (run unit tests for PriceFormatter)

A:
  compiled_text: "Run unit tests for the PriceFormatter to check the German locale is now fixed."
  compiled_predicate_ids: []

B:
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: knowledge_edge
  Note: "run" + "check" → zero predicates → no TransitionType. Orchestrator terminates without classification.
