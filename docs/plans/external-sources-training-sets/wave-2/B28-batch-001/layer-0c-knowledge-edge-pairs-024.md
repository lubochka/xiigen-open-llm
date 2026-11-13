# Layer 0-C — Knowledge Edge Pairs (024) — Complete Knowledge Edge Reference for 7.1
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E024-001 — Master gap vocabulary list for 7.1 (ReleaseGateFlow71)

Release/deployment gap terms: "go-live", "cutover", "rollout", "runbook", "deployment" alone, "provisioning", "canary" alone, "blue-green" alone, "hotfix" alone, "failover", "reroute", "trigger deployment", "enable flag", "toggle feature", "schedule audit", "A/B configuration", "DR activation"
  Teaching: None of these fire any predicate. All are gap → knowledge_edge (never SCHEDULE_CAPABILITY_LEARNING for pure gap).

---

## E024-002 — "service" rule: fires IMPLEMENT always

A: "deployment service" → IMPLEMENT | "release service" → IMPLEMENT | "rollback service" → IMPLEMENT | "canary service" → IMPLEMENT
  Teaching: "service" fires IMPLEMENT regardless of what precedes it. This rule has no exceptions in 7.1.

---

## E024-003 — "criteria" rule: fires DOCUMENT always

A: "release criteria" → DOCUMENT | "acceptance criteria" → DOCUMENT | "compliance criteria" → DOCUMENT | "rollout criteria" → DOCUMENT
  Teaching: "criteria" fires DOCUMENT regardless of what precedes it. This rule has no exceptions in 7.1.

---

## E024-004 — "build" and "build provenance" both fire IMPLEMENT

A: "build the service" → IMPLEMENT | "build provenance" → IMPLEMENT (build artifact vocabulary)
  Teaching: "build" fires IMPLEMENT. "build provenance" (artifact lineage record) fires IMPLEMENT — it is not gap.

---

## E024-005 — Pure gap → knowledge_edge (NOT SCHEDULE_CAPABILITY_LEARNING or GapHandler)

A: "Plan the deployment rollout" → [] → knowledge_edge gap only
  Teaching: Pure gap requests in 7.1 produce knowledge_edge output. They do NOT produce GapHandler or SCHEDULE_CAPABILITY_LEARNING. Those are reserved for other gap categories.
