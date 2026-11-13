# Layer 0-C — Knowledge Edge Pairs (025) — SmokeTestSuite
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E025-001 — SmokeTestSuite trigger edge

A: "run SmokeTestSuite" → TEST | "execute smoke tests" → TEST | "smoke test suite" + execution verb → TEST
  Teaching: Any execution verb + SmokeTestSuite or smoke test noun fires TEST.

---

## E025-002 — "trigger smoke tests" is gap

A: "trigger the smoke test pipeline" → [] → knowledge_edge
  Teaching: "trigger" alone is execution vocabulary; gap. Does not fire TEST even with test artifact present.

---

## E025-003 — "build smoke test harness" fires IMPLEMENT + TEST (distinct)

A: "build" → IMPLEMENT | "smoke test harness" → TEST | Two separate predicates
  Teaching: "build" fires IMPLEMENT; "smoke test harness" fires TEST. Both fire independently.

---

## E025-004 — "document the smoke test criteria" fires DOCUMENT (twice, deduplicated)

A: "document" → DOCUMENT | "criteria" → DOCUMENT | Deduplicated to one DOCUMENT
  Teaching: Two DOCUMENT triggers in one clause → one predicate. Criteria rule and document verb both fire DOCUMENT.

---

## E025-005 — "deploying" alone is gap

A: "deploying the smoke test results" → [] → knowledge_edge
  Teaching: "deploying" (execution vocabulary) is gap even when combined with test artifact nouns.
