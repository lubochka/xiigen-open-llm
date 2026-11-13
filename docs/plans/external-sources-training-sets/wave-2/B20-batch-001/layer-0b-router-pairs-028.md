# Layer 0-B — Router Pairs (028) — ROICalculator routing: "spec" and "add" predicate routing
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R028-001 — TEST alone: "spec" routes to 5.1 with TransitionType documentation

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Spec the test suite for the ROICalculator covering positive ROI, zero ROI, and negative ROI scenarios."

B:
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1
  TransitionType: documentation
  PrimarySubPhaseId: 5.1

---

## R028-002 — TEST+IMPLEMENT: "add and test" ROI routes to 5.1 + 4.1

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Add the ROICalculator to the FinancialMetricsService and test its calculation against known benchmarks."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT → sub_phase: 4.1
    - predicate: TEST → sub_phase: 5.1
  TransitionType: feature_addition
  AllSubPhaseIds: [4.1, 5.1]

---

## R028-003 — ZERO predicates: "invoke" and "calculate" produce no routing

A:
  compiled_predicate_ids: []
  compiled_text: "Invoke the ROICalculator with a £10,000 investment and £2,500 gain."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  SourceStatus: knowledge_edge
  Note: "invoke" is execution verb. "calculate" is domain verb. Both are zero-predicate.
