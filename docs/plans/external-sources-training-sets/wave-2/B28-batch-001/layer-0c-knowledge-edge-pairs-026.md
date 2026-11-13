# Layer 0-C — Knowledge Edge Pairs (026) — SanityCheckSuite
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E026-001 — SanityCheckSuite trigger edge

A: "run SanityCheckSuite" → TEST | "perform sanity check" → TEST | "execute sanity check suite" → TEST
  Teaching: Any execution verb + SanityCheckSuite or sanity check noun fires TEST.

---

## E026-002 — "validate" fires TEST in sanity check context

A: "validate the sanity check results" → TEST
  Teaching: "validate" fires TEST in any context including sanity check. Always TEST, not gap.

---

## E026-003 — "schedule the sanity check" is gap

A: "schedule the sanity check run after the rollout" → [] → knowledge_edge
  Teaching: "schedule" alone is execution planning vocabulary; gap regardless of what follows.

---

## E026-004 — "design sanity check criteria" fires ARCHITECTURE + DOCUMENT

A: "design" → ARCHITECTURE | "criteria" → DOCUMENT | Two predicates fire independently
  Teaching: "design" fires ARCHITECTURE (design verb). "criteria" fires DOCUMENT (criteria rule). Independent triggers.

---

## E026-005 — "sanity check results" alone (no verb) is gap

A: "sanity check results for the deployment" → [] → knowledge_edge
  Teaching: Noun-only phrases without an action verb do not fire TEST. Execution vocabulary ("deployment") confirms gap.
