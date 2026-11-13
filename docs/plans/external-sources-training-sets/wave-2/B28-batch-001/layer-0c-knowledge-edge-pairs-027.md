# Layer 0-C — Knowledge Edge Pairs (027) — RegressionGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E027-001 — RegressionGate trigger edge

A: "pass RegressionGate" → TEST | "run regression tests" → TEST | "regression test suite" + execution verb → TEST
  Teaching: RegressionGate and regression tests fire TEST.

---

## E027-002 — "write regression tests" fires TEST

A: "write regression tests for the bug" → TEST
  Teaching: "write" + test noun fires TEST (write = test-creation verb).

---

## E027-003 — "trigger RegressionGate" is gap

A: "trigger the RegressionGate pipeline run automatically" → [] → knowledge_edge
  Teaching: "trigger" + "automatically" = execution vocabulary; gap.

---

## E027-004 — "implementing" a regression fix fires IMPLEMENT

A: "implement the regression fix in the payment service" → IMPLEMENT (implement verb + service noun)
  Teaching: "implement" fires IMPLEMENT; "service" fires IMPLEMENT (deduplicated).

---

## E027-005 — "promoting to staging" is gap

A: "promoting the release to staging after regression gate passes" → [] → knowledge_edge
  Teaching: "promoting", "staging" are CI/CD pipeline execution vocabulary; pure gap.
