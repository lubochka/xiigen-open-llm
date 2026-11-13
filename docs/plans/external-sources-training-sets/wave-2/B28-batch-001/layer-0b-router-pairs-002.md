# Layer 0-B — Router Pairs (002)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: router

---

## B28-router-002-001 — QA alone → 7.3

A: ["QA"] | B: SubPhase 7.3 (QAGatingFlow73)

## B28-router-002-002 — QA+TEST → 7.3, 5.1

A: ["QA", "TEST"] | B: SubPhase 7.3 + SubPhase 5.1. Independent. TEST sub-phase depends on test type: unit/regression → 5.1; smoke/E2E → 5.3.

## B28-router-002-003 — QA+IMPLEMENT → 7.3, 4.1

A: ["QA", "IMPLEMENT"] | B: SubPhase 7.3 + SubPhase 4.1. TransitionType: feature_addition.

## B28-router-002-004 — QA+DOCUMENT → 7.3, 6.2

A: ["QA", "DOCUMENT"] | B: SubPhase 7.3 + SubPhase 6.2. TransitionType: documentation.

## B28-router-002-005 — gap → GapHandler

A: [] | B: GapHandler. SCHEDULE_CAPABILITY_LEARNING emitted. DomainId: "software-development".

