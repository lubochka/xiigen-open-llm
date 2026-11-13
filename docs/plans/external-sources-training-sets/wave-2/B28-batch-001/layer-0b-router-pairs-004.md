# Layer 0-B — Router Pairs (004)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: router

---

## B28-router-004-001 — QA+IMPLEMENT+TEST+DOCUMENT → 7.3, 4.1, 5.1, 6.2

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"] | B: 4 flows. TransitionType: feature_addition.

## B28-router-004-002 — QA+SECURITY+TEST → 7.3, 6.3, 5.1

A: ["QA", "SECURITY", "TEST"] | B: 7.3 + 6.3 + 5.1. Security QA cycle.

## B28-router-004-003 — QA: sub-phase 7.1 vs 7.3 disambiguation

A: ["QA"] | release/deploy context → 7.1 (ReleaseGateFlow71) | general QA gating → 7.3 (QAGatingFlow73)
  Teaching: QA routes to 7.1 for release/deploy pipeline gates; 7.3 for general acceptance gating.

## B28-router-004-004 — QA+IMPLEMENT+TEST+SECURITY+DOCUMENT → 5 flows

A: ["QA", "IMPLEMENT", "TEST", "SECURITY", "DOCUMENT"] | B: 7.3 + 4.1 + 5.1 + 6.3 + 6.2. TransitionType: feature_addition.

## B28-router-004-005 — v6-01 corruption: QA_AUTOMATION invented

A: ["QA"] | Corrupt: ["QA", "QA_AUTOMATION"] → v6-01 (QA_AUTOMATION is not a valid predicate)
  Correct: ["QA"] | Teaching: Only 9 valid predicates. QA_AUTOMATION does not exist.

