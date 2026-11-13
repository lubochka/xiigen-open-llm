# Layer 0-A — Compiler Pairs (028) — ROICalculator: TEST predicate with financial domain vocabulary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P028-001 — TEST: "spec" fires TEST on ROI (Return on Investment) boundary

A: "Spec the test suite for the ROICalculator covering positive ROI, zero ROI, and negative ROI scenarios."
  "spec" → TEST | "test suite" → TEST (deduplicates)
  "positive ROI", "zero ROI", "negative ROI" — domain phrases; no predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P028-002 — TEST+IMPLEMENT: "add and test" ROICalculator

A: "Add the ROICalculator to the FinancialMetricsService and test its calculation against known ROI benchmarks."
  "add" → IMPLEMENT | "test" → TEST (verb form fires TEST)
  "known ROI benchmarks" — domain phrase; no predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## P028-003 — GAP: "invoke" is execution verb — zero predicates

A: "Invoke the ROICalculator with a £10,000 investment and £2,500 gain."
  "invoke" — execution verb; same family as "run". Zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P028-004 — TEST: "assert" on ROI precision

A: "Assert that the ROICalculator returns 25.0% for a £10,000 investment with £2,500 net gain."
  "assert" → TEST. Fires on any assertion regardless of domain.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P028-005 — Boundary: "ROI" abbreviation does NOT fire any predicate

A: "Calculate ROI for the marketing campaign investment."
  "ROI" — domain abbreviation; no predicate. "calculate" — domain verb; not in IMPLEMENT lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Note: Financial abbreviations (ROI, NPV (Net Present Value), IRR (Internal Rate of Return)) are domain vocabulary, not predicate-firing.
