# Layer 0-C — Knowledge Edge Pairs (015)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E015-001 — Rollback execution vocabulary is gap

Rollback gap terms: "roll back", "revert deployment", "restore previous version", "restart pods", "previous state"
  Teaching: Rollback execution commands → gap. "roll back to v2.1" → gap (not IMPLEMENT, not TEST).

---

## E015-002 — "automated rollback service" fires IMPLEMENT twice

A: "Build the automated rollback service" → IMPLEMENT (via "build") and IMPLEMENT (via "service")
  Deduplication: ["IMPLEMENT"] — set, not multiset.
  Teaching: "service" alone and "build" alone both fire IMPLEMENT. Deduplication yields one IMPLEMENT.

---

## E015-003 — "verify rollback restores state" fires TEST

A: "Verify that rollback restores the previous state" → TEST fires on "verify"
  Teaching: "verify" fires TEST regardless of object. "restores previous state" is gap by itself.

---

## E015-004 — "rollback runbook" vs "document the rollback procedure"

A: "rollback runbook" alone → gap | "document the rollback procedure" → DOCUMENT (verb "document")
  Teaching: "runbook" alone is gap. The documentation verb "document" fires DOCUMENT.

---

## E015-005 — QA fires for rollback gate

A: "QA gate the rollback procedure" → QA | "QA sign-off before rollback" → QA | "QA review the rollback runbook" → QA
  Teaching: "QA" fires in rollback gate context. "rollback" and "runbook" remain gap even inside a QA compound.
