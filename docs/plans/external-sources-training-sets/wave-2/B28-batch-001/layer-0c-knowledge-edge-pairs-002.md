# Layer 0-C — Knowledge Edge Pairs (002)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E002-001 — "QA" abbreviation fires QA; "quality assurance" does not

A: "QA sign-off needed before release." → ["QA"]
  vs "Quality assurance sign-off needed before release." → []
  Teaching: Strict abbreviation gate. "QA" fires QA. "quality assurance" spelled out → gap. No exception.

---

## E002-002 — "deploy" is universally gap

A: "Deploy the OrderService to production." → []
  Teaching: "deploy", "provision", "rollout", "push to production" → gap across all predicates.

---

## E002-003 — "QA" compound nouns fire QA

A: "The QA lead must approve the release." → ["QA"]
  A: "Complete the QA checklist." → ["QA"]
  A: "QA environment setup required." → ["QA"]
  Teaching: "QA lead", "QA checklist", "QA environment", "QA team", "QA engineer", "QA gate", "QA pass" — all fire QA.

---

## E002-004 — "monitor" is gap

A: "Monitor the production deployment for 24 hours." → []
  Teaching: Observability vocabulary (monitor, alert, metrics, SLO, error rate, latency) is gap across all predicates.

---

## E002-005 — "smoke test" fires TEST (sub-phase 5.3, not 5.1)

A: "Write smoke tests for the CheckoutService before QA sign-off." → ["TEST", "QA"]
  "smoke test" → TEST (E2E testing class → sub-phase 5.3) | "QA sign-off" → QA
  Teaching: "smoke test" fires TEST at sub-phase 5.3 (E2E flow), not 5.1 (unit test flow).

