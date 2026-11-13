# Layer 0-C — Knowledge Edge Pairs (040) — ApprovalGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E040-001 — "approval" alone is gap; "QA approve" fires QA

"approval" → gap | "sign-off approval" → gap | "QA approve" → QA (QA abbreviation prefix fires)
  Teaching: The abbreviation "QA" must be present for QA to fire. "approve" alone is gap.

---

## E040-002 — "authorize" fires AUTHORIZATION; "approve" does not

"authorize the release manager role" → AUTHORIZATION | "approve the release" → gap
  Teaching: "authorize" fires AUTHORIZATION. "approve" without QA prefix is gap.

---

## E040-003 — "approval gate service" fires IMPLEMENT via "service" rule

"approval gate service" → IMPLEMENT | "gate approval service" → IMPLEMENT
  Teaching: "service" fires IMPLEMENT in approval gate compound nouns.

---

## E040-004 — "manual approval" vs "automated approval" both gap

"manual approval gate" → gap | "automated approval threshold" → gap
  Teaching: Approval type modifiers (manual/automated) do not change gap status.

---

## E040-005 — "approval gate criteria" fires DOCUMENT via "criteria" rule

"approval gate criteria" → DOCUMENT | "release approval criteria" → DOCUMENT
  Teaching: "criteria" fires DOCUMENT in approval gate compound nouns.
