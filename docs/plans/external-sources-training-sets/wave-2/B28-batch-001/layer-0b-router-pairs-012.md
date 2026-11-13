# Layer 0-B — Router Pairs (012)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: router

---

## B28-router-012-001 — QA+IMPLEMENT+TEST+DOCUMENT+QA: deduplication + 4 flows

A: Multiple QA triggers → ["QA", "IMPLEMENT", "TEST", "DOCUMENT"] | 4 flows.

## B28-router-012-002 — QA: "QA lead approves" → 7.3

A: ["QA"] | trigger: "QA lead" compound noun | [QAGatingFlow73]

## B28-router-012-003 — QA+SECURITY+IMPLEMENT → 7.3, 6.3, 4.1

A: ["QA", "SECURITY", "IMPLEMENT"] | Security patch implementation + QA gate. 3 flows. feature_addition.

## B28-router-012-004 — QA+TEST+ARCHITECTURE → 7.3, 5.1, 3.5

A: ["QA", "TEST", "ARCHITECTURE"] | QA sign-off on infrastructure with test validation. 3 flows. documentation.

## B28-router-012-005 — QA+IMPLEMENT+TEST+DOCUMENT+SECURITY+ARCHITECTURE: full lifecycle → 6 flows

A: Full QA release lifecycle | 6 predicates → 6 flows | TransitionType: feature_addition.
  Sequential: ARCHITECTURE → SECURITY → IMPLEMENT → TEST → DOCUMENT → QA gate.

