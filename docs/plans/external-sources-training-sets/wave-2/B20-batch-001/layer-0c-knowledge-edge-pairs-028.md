# Layer 0-C — Knowledge Edge Pairs (028) — ROICalculator: "invoke" and financial abbreviation gaps
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E028-001 — Gap: "invoke" with financial context → knowledge_edge

A: "Invoke the ROICalculator with a £10,000 investment and £2,500 gain."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["invoke", "£10,000 investment", "£2,500 gain"]
  Note: "invoke" is an execution verb. Financial values are domain context; they do not elevate "invoke".
  SCHEDULE_CAPABILITY_LEARNING: true

---

## E028-002 — Gap: "ROI" abbreviation does not fire predicates

A: "Calculate ROI for the marketing campaign investment."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["calculate ROI", "marketing campaign"]
  Note: "ROI" (Return on Investment) is a domain abbreviation; no predicate.
  "calculate" is a domain verb; no predicate.
  Full financial abbreviation gap list: ROI, NPV (Net Present Value), IRR (Internal Rate of Return),
    COGS (Cost of Goods Sold), EBITDA (Earnings Before Interest Tax Depreciation Amortisation) — all zero-predicate.

---

## E028-003 — Contrast: "ROI" (gap) vs "test ROI" (routable)

| Request | CompiledPredicateIds | Source |
|---------|---------------------|--------|
| "Calculate ROI for the campaign" | [] | knowledge_edge |
| "Assert the ROICalculator returns 25% ROI" | ["TEST"] | routable → 5.1 |
| "Build the ROICalculator" | ["IMPLEMENT"] | routable → 4.1 |

The abbreviation "ROI" is inert. Only the action verbs (assert, build) determine predicates.
