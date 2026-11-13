# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B28-transition-011-001 — QA+IMPLEMENT+TEST+DOCUMENT+SECURITY: 5-step cycle

A: ["QA","IMPLEMENT","TEST","DOCUMENT","SECURITY"] | feature_addition
  Sequence: SECURITY → IMPLEMENT → TEST → DOCUMENT → QA gate

## B28-transition-011-002 — QA: "QA before" → sequential

A: "Implement the feature, then get QA sign-off before release." | IMPLEMENT → QA gate

## B28-transition-011-003 — QA: "QA while" → parallel

A: "Get QA sign-off while the documentation is being written." | DOCUMENT ∥ QA gate (concurrent)
  Teaching: Explicit concurrency wording ("while") → parallel execution.

## B28-transition-011-004 — QA+IMPLEMENT: "develop" → feature_addition

A: ["QA","IMPLEMENT"] | "develop the QA automation module + QA sign-off" | feature_addition ("develop" = IMPLEMENT)

## B28-transition-011-005 — QA+IMPLEMENT: "extend" → feature_addition

A: ["QA","IMPLEMENT"] | "extend the QA harness" | feature_addition ("extend" = IMPLEMENT)

