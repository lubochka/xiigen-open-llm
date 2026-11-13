# Layer 0-D — Flow Factory Pairs (034) — CommissionCalculator: factory test-infrastructure noun compounds
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F034-001 — TEST alone: "test suite" → one UnitTestingFlow51, documentation

Request: "Set up a test suite for the CommissionCalculator covering base commission and override rates."
  "test suite" → TEST. "set up" → context verb; does not fire IMPLEMENT.
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F034-002 — TEST+IMPLEMENT: "extend and add test cases" → two instances

Request: "Extend the CommissionCalculator to handle multi-tier sales brackets and add test cases."
  "extend" → IMPLEMENT | "add" → IMPLEMENT (deduplicates) | "test cases" → TEST
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F034-003 — Zero predicate: "apply commission rate" → empty factory

Request: "Apply the commission rate to all sales transactions in the CommissionCalculator."
  "apply" (standalone) → NOT in IMPLEMENT lexicon; zero predicates.
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
