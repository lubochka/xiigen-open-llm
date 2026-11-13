# Layer 2 — Nine-Node Flow Pairs (023)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ReleaseGateFlow71)

---

## B28-nine-023 — ProbationaryRejected — QA+TEST release testing flow REJECTED (FAIL verdict)

Request: "QA the NotificationService v2 release — validate deduplication, delivery guarantees, and retry logic."

### Flow: ReleaseGateFlow71

**Node 1 (DomainSplitter)**: SelectedLogicalTransition: A = "QA alone → ReleaseGateFlow71 | no TransitionType"
  Note: "validate" fires TEST → ["QA", "TEST"] | Predicates: ["QA", "TEST"] | TransitionType: documentation (TEST, no IMPLEMENT)
**Node 2 (DomainTranslator)**: Gates: deduplication correctness, delivery guarantee (at-least-once), retry logic, idempotency.
**Node 3 (SchemePlanner)**: Gate 1 — Deduplication: duplicate message ID blocked ✓
  Gate 2 — Delivery guarantee: message delivered at least once on transient failure ✓
  Gate 3 — Retry logic: 3 retries with exponential backoff ✗ FAIL — retry interval is fixed 1s, not exponential
  Gate 4 — Idempotency: consumer handler idempotent on duplicate delivery ✗ FAIL — double-insert on duplicate message ID
**Node 4 (DomainProcessor)**: Two critical defects: Gate 3 (fixed retry, not exponential) and Gate 4 (non-idempotent consumer).
**Node 5 (Applier)**: Arbiter: ReleaseGateArbiter
**Node 6 (ResultTranslator)**: Coverage: dedup: PASS, delivery: PASS, retry: FAIL, idempotency: FAIL
**Node 7 (ArbiterSelector)**: FAIL — two critical defects block release
**Node 8 (Arbiter)**: QA report: NotificationService-v2-QA.md | BUG-501 (fixed retry interval), BUG-502 (non-idempotent consumer)
**Node 9 (Formatter)**: Verdict: FAIL — NotificationService v2 not cleared. Fix BUG-501 and BUG-502 before re-QA cycle.
  Teaching: FAIL is a valid expected output. Models must not normalize FAIL verdicts to PASS.
