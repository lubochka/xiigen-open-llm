# Layer 0-A — Compiler Pairs (043)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-043-001 — SnapshotSchedule: ARCHITECTURE fires on snapshot design vocabulary

"architect the snapshot schedule policy for stateful database tiers" → ARCHITECTURE ("architect", "policy" in design context)
"design the incremental snapshot topology" → ARCHITECTURE ("design", "topology")
"structure the snapshot retention hierarchy across regions" → ARCHITECTURE ("structure", "hierarchy")
"take a snapshot" → gap ("take" is operational gap)

---

## B14-compiler-043-002 — SnapshotSchedule: IMPLEMENT fires on snapshot IaC artifacts

"implement the snapshot schedule CronJob manifest" → IMPLEMENT ("implement" + "manifest")
"write the AWS Data Lifecycle Manager (DLM) policy Terraform module" → IMPLEMENT ("write" + "module")
"build the snapshot automation script" → IMPLEMENT ("build" + "script")

---

## B14-compiler-043-003 — SnapshotSchedule: SECURITY fires on snapshot credential vocabulary

"enforce snapshot encryption at rest with customer-managed KMS keys" → SECURITY ("enforce", "encryption at rest", "key")
"restrict snapshot copy permissions to backup-role only" → SECURITY + AUTHORIZATION
"rotate snapshot encryption keys on monthly schedule" → SECURITY ("rotate" + "key")

---

## B14-compiler-043-004 — SnapshotSchedule: disambiguation — "schedule" is context-dependent

"schedule a snapshot" → gap ("schedule" as operational task-scheduling verb = gap)
"design the snapshot schedule policy" → ARCHITECTURE ("design" = ARCHITECTURE trigger; "schedule" is a noun here not the trigger)
"implement the snapshot schedule" → IMPLEMENT ("implement" = trigger; "schedule" is the artifact noun)
Key rule: trigger verb determines predicate; "schedule" as a standalone verb is operational gap.

---

## B14-compiler-043-005 — SnapshotSchedule: DOCUMENT compilation

"document the snapshot schedule ADR and retention policy" → DOCUMENT ("document" + "ADR")
CompiledPredicateIds: ["DOCUMENT"]
CapabilityGapWarnings: gap-devops-cloud-001 ("DLM", "snapshot", "CRR (Cross-Region Replication)" are gap)
