# Layer 0-C — Knowledge Edge Pairs (058) — SLAVerification
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E058-001 — "SLA verification" fires TEST; "SLA compliance" alone is gap

"SLA verification" → TEST | "run SLA verification" → TEST | "SLA compliance" → gap | "SLO adherence" → gap
  Teaching: "verification" fires TEST. "SLA"/"SLO" alone without verification verb is gap.

---

## E058-002 — "uptime" and "error budget" are gap

"uptime calculation" → gap | "error budget burn rate" → gap | "SLA breach" → gap
  Teaching: SLA metric vocabulary is gap.

---

## E058-003 — "SLA verification service" fires IMPLEMENT via "service" rule

"SLA verification service" → IMPLEMENT | "SLA checker service" → IMPLEMENT
  Teaching: "service" fires IMPLEMENT in SLA verification compound nouns.

---

## E058-004 — "SLA threshold tests" fires TEST via test-writing verb

"write tests for SLA threshold checks" → TEST | "add SLA compliance tests" → TEST
  Teaching: Test-writing verbs fire TEST in SLA verification context.

---

## E058-005 — "SLA compliance criteria" fires DOCUMENT via "criteria" rule

"SLA compliance criteria" → DOCUMENT | "SLA gate criteria" → DOCUMENT
  Teaching: "criteria" fires DOCUMENT in SLA compound nouns.
