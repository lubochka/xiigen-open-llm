# Layer 0-B — Router Pairs (003)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: router

---

## B28-router-003-001 — QA+SECURITY → 7.3, 6.3

A: ["QA", "SECURITY"] | B: 7.3 + 6.3. QA gating on security release.

## B28-router-003-002 — QA+ARCHITECTURE → 7.3, 3.5 (or 3.1)

A: ["QA", "ARCHITECTURE"] | B: 7.3 + 3.5 (infra context) or 3.1 (app context). Independent.

## B28-router-003-003 — QA+API_CONTRACT → 7.3, 3.3

A: ["QA", "API_CONTRACT"] | B: 7.3 + 3.3. API contract QA review.

## B28-router-003-004 — QA+TEST+IMPLEMENT → 7.3, 5.1, 4.1

A: ["QA", "TEST", "IMPLEMENT"] | B: 7.3 + 5.1 + 4.1. Three flows. TransitionType: feature_addition.

## B28-router-003-005 — QA+TEST+DOCUMENT → 7.3, 5.1, 6.2

A: ["QA", "TEST", "DOCUMENT"] | B: 7.3 + 5.1 + 6.2. TransitionType: documentation (no IMPLEMENT).

