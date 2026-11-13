# Layer 0-B — Router Pairs (042)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-042-001 — BackupPolicy: routing for ARCHITECTURE → 3.5

"Architect the backup retention and recovery topology."
  ARCHITECTURE → 3.5 | TransitionType: feature_addition | TrustState: deterministic_generic

---

## B14-router-042-002 — BackupPolicy: routing for IMPLEMENT + SECURITY

"Implement the Velero backup manifest and enforce encryption at rest."
  IMPLEMENT → 4.1 | SECURITY → 6.3 | Sequential: 6.3 → 4.1
  TransitionType: feature_addition | AllSubPhaseIds: [4.1, 6.3]

---

## B14-router-042-003 — BackupPolicy: routing for DOCUMENT alone

"Document the backup policy ADR and RTO/RPO (Recovery Time Objective/Recovery Point Objective) targets."
  DOCUMENT → 6.2 | TransitionType: documentation | TrustState: deterministic_generic

---

## B14-router-042-004 — BackupPolicy: routing for gap operational verbs

"Restore the database from the last backup."
  Zero predicates → GapHandler | "restore" is operational gap
  CapabilityGapWarnings: gap-devops-cloud-001

"Run the nightly backup job."
  Zero predicates → GapHandler | "run" is operational gap

"Trigger backup for the production namespace."
  Zero predicates → GapHandler | "trigger" is operational gap
