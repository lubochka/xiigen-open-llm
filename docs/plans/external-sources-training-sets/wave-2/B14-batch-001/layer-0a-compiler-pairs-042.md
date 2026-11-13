# Layer 0-A — Compiler Pairs (042)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-042-001 — BackupPolicy: ARCHITECTURE fires on backup design vocabulary

"architect the backup policy framework for production databases" → ARCHITECTURE ("architect", "framework")
"design the backup retention and recovery topology" → ARCHITECTURE ("design", "topology")
"structure the 3-2-1 backup model across regions" → ARCHITECTURE ("structure", "model")
"run the backup job" → gap ("run" is operational gap)

---

## B14-compiler-042-002 — BackupPolicy: IMPLEMENT fires on backup IaC artifacts

"implement the backup policy Terraform module" → IMPLEMENT ("implement" + "module")
"write the Velero backup schedule manifest" → IMPLEMENT ("write" + "manifest")
"scaffold the database backup CronJob configuration" → IMPLEMENT ("scaffold" + "configuration")

---

## B14-compiler-042-003 — BackupPolicy: SECURITY fires on backup encryption vocabulary

"enforce encryption at rest for all backup archives" → SECURITY ("enforce", "encryption at rest")
"rotate backup encryption keys on 90-day schedule" → SECURITY ("rotate" + "key" = key rotation)
"restrict backup access credentials to backup-operator service account" → SECURITY + AUTHORIZATION

---

## B14-compiler-042-004 — BackupPolicy: DOCUMENT fires on backup documentation vocabulary

"document the backup policy ADR and RTO/RPO targets" → DOCUMENT ("document" + "ADR")
"write the backup and restore runbook" → DOCUMENT ("write" + "runbook")
"record the backup retention decision" → DOCUMENT ("record" in documentation context)

---

## B14-compiler-042-005 — BackupPolicy: gap vocabulary — backup lifecycle operations

Confirmed gap verbs for backup lifecycle (fire zero predicates):
  run (backup), trigger (backup), execute (backup), restore (operation), verify (backup), test restore, purge (old backups), expire
  "restore from backup" → gap | "trigger backup job" → gap | "purge expired backups" → gap
