# Layer 0-C — Knowledge Edge Pairs (004)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: knowledge_edge

## KE-004-001 — TransitionType: refactoring overrides feature_addition
"refactor" verb present → TransitionType: refactoring (highest priority)
"build + refactor" in same request → refactoring (refactor verb wins)
"refactor + add feature" → refactoring (refactor verb still wins)

## KE-004-002 — TEST routing: 5.1 vs 5.3 in B15 context
"unit tests" + algorithm/calculator/utility/pure function → UnitTestingFlow51 (5.1)
"integration tests" + API/service → E2ETestingFlow53 (5.3)
"E2E tests" + user flow/purchase journey → E2ETestingFlow53 (5.3)

## KE-004-003 — QA sub-phase: 7.1 vs 7.3 in B15 context
Release/staging/deployment vocabulary → ReleaseGateFlow71 (7.1)
Feature acceptance/gate/sign-off → QAGatingFlow73 (7.3, default)

## KE-004-004 — ARCHITECTURE sub-phase: 3.1 vs 3.5 in B15
"order service architecture / domain model / bounded context" → 3.1 (application)
"infrastructure for order processing cluster / cloud deployment" → 3.5 (infrastructure)
