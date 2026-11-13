# Layer 0-C — Knowledge Edge Pairs (006)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E006-001 — "sign-off" fires QA only with "QA" compound

A: "Get sign-off from the team before release." → []
  vs "Get QA sign-off before release." → ["QA"]
  Teaching: "sign-off" alone is gap. "QA sign-off" fires QA.

---

## E006-002 — "approval" fires QA only with "QA" compound

A: "Get approval from the manager before shipping." → []
  vs "Get QA approval before shipping." → ["QA"]
  Teaching: "approval" alone is gap. "QA approval" fires QA.

---

## E006-003 — "QA" in "QA testing" fires QA (gerund variant)

A: "QA testing is required before the Tuesday release." → ["QA"]
  Teaching: "QA testing" as a compound noun fires QA via QA abbreviation.

---

## E006-004 — "integration testing" fires TEST (not QA)

A: "Write integration tests for the PaymentGateway before QA sign-off." → ["TEST", "QA"]
  "write integration tests" → TEST | "QA sign-off" → QA
  Teaching: "integration testing" fires TEST. QA requires explicit "QA" abbreviation.

---

## E006-005 — "QA process" fires QA

A: "Document the QA process for the release pipeline." → ["QA", "DOCUMENT"]
  "QA process" → QA | "document" → DOCUMENT
  Teaching: "QA process", "QA workflow", "QA procedure" — all fire QA via compound noun.

