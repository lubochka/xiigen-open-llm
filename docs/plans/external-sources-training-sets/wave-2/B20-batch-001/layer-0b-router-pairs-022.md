# Layer 0-B — Router Pairs (022)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: router

---

## R022-001 — TEST+IMPLEMENT → 5.1 + 4.1 (PriceFormatterTests)

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the PriceFormatter verifying that currency symbols, decimal places, and locale-specific separators are rendered correctly."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'unit tests' + 'verifying' → TEST → 5.1"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'write' → IMPLEMENT → 4.1"
  TransitionType: feature_addition
  PrimarySubPhaseId: 5.1
  AllSubPhaseIds: [5.1, 4.1]

---

## R022-002 — TEST alone → 5.1 (validate rounding mode)

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Validate that the PriceFormatter wraps negative prices in parentheses for accountancy display mode."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'validate' → TEST → 5.1 (universal rule)"
  TransitionType: documentation
  SubPhaseId: 5.1
  Note: "validate" fires TEST in all contexts — formatting validation, business rules, test assertions — universally.
