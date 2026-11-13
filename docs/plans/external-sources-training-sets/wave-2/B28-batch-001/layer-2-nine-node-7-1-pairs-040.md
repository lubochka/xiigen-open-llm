# Layer 2 — Nine-Node Flow Pairs (040) — DNSCutover Gate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ReleaseGateFlow71)

---

## B28-nine-040 — ReleaseGateFlow71: DNSCutover gate trace

### Flow: ReleaseGateFlow71 (sub-phase 7.1) | Target: PaymentService v5 DNS cutover gate

**Node 01 (DomainSplitter)**: A = SelectedLogicalTransition
  Predicate set: ["IMPLEMENT", "TEST", "QA"] | TransitionType: feature_addition

**Node 02 (DomainTranslator)**: Gate scope — DNS propagation completeness, traffic routing correctness, rollback readiness

**Node 03 (SchemePlanner)**: Gate assertions:
  ✓ DNS resolves to new endpoint from all regions within TTL window
  ✓ Traffic routed 100% to new cluster after cutover
  ✓ Rollback DNS record pre-staged and tested

**Node 04 (DomainProcessor)**: Defect classification:
  Critical: none | Non-critical: one region (ap-southeast-1) has stale cache for 45s post-TTL (non-blocking)

**Node 05 (Applier)**: DomainSplitter → DomainTranslator → SchemePlanner → DomainProcessor → Applier → ResultTranslator → ArbiterSelector → Arbiter → Formatter

**Node 06 (ResultTranslator)**: DNS propagation: PASS_WITH_WARNINGS (ap-southeast-1 stale 45s) | Traffic routing: PASS ✓ | Rollback readiness: PASS ✓

**Node 07 (ArbiterSelector)**: Verdict: PASS_WITH_WARNINGS

**Node 08 (Arbiter)**: QA report: W-4001 (ap-southeast-1 DNS cache stale 45s post-TTL — expected propagation delay for region)

**Node 09 (Formatter)**: Verdict: PASS_WITH_WARNINGS | TransitionType: feature_addition
  W-4001: ap-southeast-1 propagation delay 45s — expected for region. No action required.
