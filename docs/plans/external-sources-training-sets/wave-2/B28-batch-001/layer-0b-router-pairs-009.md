# Layer 0-B — Router Pairs (009)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: router

---

## B28-router-009-001 — QA: compound noun trigger → 7.3

A: ["QA"] | trigger: "QA checklist" | [QAGatingFlow73]

## B28-router-009-002 — QA: imperative verb trigger → 7.3

A: ["QA"] | trigger: "QA this before shipping" | [QAGatingFlow73]

## B28-router-009-003 — QA+SECURITY: security patch QA → 7.3, 6.3

A: ["QA", "SECURITY"] | security CVE fix QA | [QAGatingFlow73, SecurityAuditFlow63]

## B28-router-009-004 — QA+ARCHITECTURE+IMPLEMENT → 7.3, 3.5, 4.1

A: ["QA", "ARCHITECTURE", "IMPLEMENT"] | TransitionType: feature_addition. QA sign-off on infrastructure implementation.

## B28-router-009-005 — v6-02: gap → QA (observability request misrouted)

A: "Monitor the QA environment for errors." | Correct: ["QA"] (QA environment triggers QA; "monitor" is gap alone)
  vs "Monitor the error rate." | Correct: []
  Teaching: "QA environment" fires QA even with gap verb. "Monitor" alone (without QA compound) → gap.

