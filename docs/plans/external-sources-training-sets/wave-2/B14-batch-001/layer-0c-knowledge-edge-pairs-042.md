# Layer 0-C — Knowledge Edge Pairs (042)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-042-001 — BackupPolicy: gap vocabulary — backup lifecycle operations

Confirmed gap vocabulary for backup lifecycle operations (zero predicates):
  Action verbs: restore, recover, run (backup job), trigger, execute, verify (backup), test restore, purge, expire, age out, archive (move to cold storage)
  Backup runtime nouns: backup job status, recovery point, restore point, backup window, backup vault (AWS Backup vault — operational context, not Vault secrets manager)

---

## B14-edge-042-002 — BackupPolicy: disambiguation — "Vault" vs "backup vault"

"Vault" (HashiCorp Vault / secrets management) → SECURITY ("vault" fires SECURITY directly)
"backup vault" (AWS Backup Vault operational resource) → gap (operational backup infrastructure)
Rule: "vault" as a standalone noun always fires SECURITY; "backup vault" in AWS Backup operational context is gap.

---

## B14-edge-042-003 — BackupPolicy: SECURITY boundary — backup encryption compound phrases

SECURITY fires for:
  "encryption at rest for backup archives", "rotate backup encryption keys", "backup encryption key rotation", "KMS-encrypted backup"
  These fire from the encryption/key rotation vocabulary present.

---

## B14-edge-042-004 — BackupPolicy: IMPLEMENT boundary — backup IaC artifacts

IMPLEMENT fires for:
  Velero backup schedule manifest, backup policy Terraform module, database backup CronJob configuration, backup rule file, backup schedule configuration
  "implement backup policy" → IMPLEMENT | "write Velero backup manifest" → IMPLEMENT
  "run the backup job" → gap | "trigger backup" → gap
