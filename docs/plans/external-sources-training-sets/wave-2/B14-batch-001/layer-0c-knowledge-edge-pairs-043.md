# Layer 0-C — Knowledge Edge Pairs (043)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-043-001 — SnapshotSchedule: gap vocabulary — snapshot runtime operations

Confirmed gap vocabulary for snapshot operations (zero predicates when used without design/creation verbs):
  Action verbs: take (a snapshot), create (operational context), trigger, run, delete (snapshot), copy (to region — operational), list, describe (describe snapshots — operational)
  Note: "create" without IaC-artifact context = operational gap; "create the snapshot schedule manifest" → IMPLEMENT (create + manifest = IaC artifact)

---

## B14-edge-043-002 — SnapshotSchedule: disambiguation — "schedule" verb vs noun

"schedule" as verb: "schedule the snapshot job" → gap (operational task-scheduling verb)
"schedule" as noun following creation verb: "implement the snapshot schedule" → IMPLEMENT ("implement" is trigger; "schedule" is artifact noun)
"schedule" as noun following design verb: "design the snapshot schedule policy" → ARCHITECTURE ("design" is trigger)
Rule: verb triggers the predicate; "schedule" alone (without a design/creation verb) is always operational gap.

---

## B14-edge-043-003 — SnapshotSchedule: SECURITY boundary — snapshot encryption

SECURITY fires for:
  "customer-managed KMS keys for snapshot encryption", "encrypt snapshots at rest", "rotate snapshot encryption keys"
  "copy encrypted snapshots to another region" → gap ("copy" is operational gap; encryption alone doesn't fire if no encryption verb)

---

## B14-edge-043-004 — SnapshotSchedule: IMPLEMENT boundary — snapshot IaC artifacts

IMPLEMENT fires for:
  snapshot schedule CronJob manifest, DLM (Data Lifecycle Manager) policy Terraform module, snapshot automation script, snapshot retention rule configuration
  "implement snapshot schedule CronJob" → IMPLEMENT | "write DLM policy module" → IMPLEMENT
  "take a snapshot" → gap | "trigger snapshot" → gap
