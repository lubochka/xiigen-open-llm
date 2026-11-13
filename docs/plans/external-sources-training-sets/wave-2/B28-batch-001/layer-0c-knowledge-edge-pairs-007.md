# Layer 0-C — Knowledge Edge Pairs (007)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E007-001 — "QA owner" fires QA

A: "Assign a QA owner to the sprint release." → ["QA"]
  Teaching: "QA owner" fires QA via compound noun with QA abbreviation.

---

## E007-002 — "staging" alone is gap

A: "Test the feature in the staging environment." → ["TEST"]
  "test" → TEST | "staging environment" → gap
  Teaching: "staging", "staging environment", "staging server" → gap alone. TEST fires on "test" verb.

---

## E007-003 — "penetration testing" fires SECURITY+TEST

A: "Write penetration tests for the AuthService and get QA sign-off." → ["SECURITY", "TEST", "QA"]
  "penetration tests" → SECURITY + TEST | "QA sign-off" → QA
  Teaching: "penetration testing" fires SECURITY (attack simulation) and TEST (writing test artifacts). Both independent.

---

## E007-004 — "QA" in sprint ceremonies is still QA

A: "In the sprint review, QA sign-off is required before the demo." → ["QA"]
  Teaching: "QA" in agile ceremony contexts (sprint review, sprint demo, sprint retrospective) still fires QA.

---

## E007-005 — Multiple "QA" instances → one QA predicate

A: "QA sign-off: QA lead must approve. QA testing must pass. QA report must be filed." → ["QA"]
  Teaching: QA is a set predicate. Multiple QA triggers → one QA predicate (deduplication).

