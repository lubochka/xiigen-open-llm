# Layer 0-C — Knowledge Edge Pairs (011)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E011-001 — "QA review" fires QA

A: "Schedule a QA review for the API changes before the next sprint release." → ["QA"]
  Teaching: "QA review" fires QA via compound noun. "schedule" is gap alone.

---

## E011-002 — "production readiness" alone is gap

A: "Check the production readiness of the system." → []
  vs "QA production readiness review." → ["QA"]
  Teaching: "production readiness" without "QA" is gap. Requires QA abbreviation compound.

---

## E011-003 — "penetration test" in QA context fires SECURITY+TEST+QA

A: "Write penetration tests, get QA sign-off on the security release." → ["SECURITY", "TEST", "QA"]
  "penetration tests" → SECURITY + TEST | "QA sign-off" → QA
  Teaching: Penetration testing QA gate fires three predicates: SECURITY, TEST, QA.

---

## E011-004 — "release" alone is gap

A: "Prepare the release for Friday." → []
  vs "QA sign-off on the Friday release." → ["QA"]
  Teaching: "release" alone is gap. "QA" compound must be present.

---

## E011-005 — "QA metrics" fires QA

A: "Review the QA metrics from the last sprint." → ["QA"]
  Teaching: "QA metrics" fires QA via compound noun. "review" does not fire SECURITY here (no security co-occurrence).

