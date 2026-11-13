# Layer 0-C — Knowledge Edge Pairs (038) — ParallelStageExecution
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E038-001 — "parallel" and "concurrent" are gap in all positions

"parallel stage execution" → gap | "concurrent pipeline run" → gap | "run in parallel" → gap
  Teaching: Parallelism and concurrency vocabulary is gap universally in the release domain.

---

## E038-002 — "fan-out" and "fan-in" are gap

"fan-out to parallel stages" → gap | "fan-in after parallel completion" → gap
  Teaching: Pipeline parallelism topology vocabulary is gap.

---

## E038-003 — "parallel test suite" vs "write parallel tests"

"parallel test suite" → TEST (via "test suite" vocabulary) | "write parallel tests" → TEST (test-writing verb)
  Teaching: Both forms fire TEST. "parallel" modifies the test noun but does not change the predicate.

---

## E038-004 — "parallel service" fires IMPLEMENT via "service" rule

"parallel runner service" → IMPLEMENT | "parallel executor service" → IMPLEMENT
  Teaching: "service" fires IMPLEMENT even when "parallel" is part of the compound noun.

---

## E038-005 — "enable parallel execution" is gap despite "enable" appearing to be an action verb

"enable parallel execution" → gap | "enable concurrent stages" → gap
  Teaching: "enable" in configuration context is gap. Does not fire IMPLEMENT.
