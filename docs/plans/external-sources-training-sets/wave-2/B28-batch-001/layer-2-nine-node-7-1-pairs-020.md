# Layer 2 — Nine-Node Flow Pairs (020)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ReleaseGateFlow71)

---

## B28-nine-020 — ABTestingGate — QA alone (no contribution TransitionType)

Request: "QA the A/B testing release for the SearchService ranking experiment before the go-live."

### Flow: ReleaseGateFlow71

**Node 1 (DomainSplitter)**: SelectedLogicalTransition: A = "QA alone → ReleaseGateFlow71 only | no TransitionType"
  Target: SearchService ranking A/B experiment | Predicates: ["QA"] | TransitionType: none
**Node 2 (DomainTranslator)**: Gates: variant assignment correctness, traffic split accuracy, metric collection integrity, experiment isolation.
**Node 3 (SchemePlanner)**: Gate 1 — Variant assignment: 50% control, 50% treatment verified via 1000-request sample ✓
  Gate 2 — Traffic split: no leakage between control and treatment groups ✓
  Gate 3 — Metric collection: click-through rate and session duration captured correctly ✓
  Gate 4 — Experiment isolation: SearchService experiment does not affect RecommendationService ✗ WARNING
**Node 4 (DomainProcessor)**: Gate 4 — cross-service metric leakage observed in shared event bus. Non-critical for current experiment window.
**Node 5 (Applier)**: Arbiter: ReleaseGateArbiter (A/B experiment gate)
**Node 6 (ResultTranslator)**: Coverage: assignment: PASS, split: PASS, metrics: PASS, isolation: WARNING
**Node 7 (ArbiterSelector)**: PASS_WITH_WARNINGS — isolation issue is non-blocking for this experiment
**Node 8 (Arbiter)**: QA report: SearchService-AB-QA.md | WARN-210 (cross-service metric leakage)
**Node 9 (Formatter)**: Verdict: PASS_WITH_WARNINGS — A/B gate cleared with WARN-210 filed. QA alone → no TransitionType.
