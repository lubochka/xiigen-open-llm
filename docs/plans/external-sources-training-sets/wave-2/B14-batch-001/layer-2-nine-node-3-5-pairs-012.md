# Layer 2 — Nine-Node Flow Pairs (012)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-012 — Backup and restore architecture

Request: "Design the backup and restore architecture for the platform — define backup frequency, retention policy, cross-region replication topology, and restoration boundary."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — Input parsing**
  Target: Platform backup and restore architecture | Scope: all stateful services
  Architecture class: data protection topology

**Node 2 — Requirements extraction**
  Functional: daily full backup + hourly incremental; 30-day retention; cross-region backup replication; tested restore path
  Non-functional: backup completion < 4 hours; restore RTO < 30 minutes for any single service

**Node 3 — Option generation**
  Backup approach A: Database native backup (pg_dump) — portable but slow for large datasets
  Backup approach B: Volume snapshot (EBS snapshot) — fast; consistent; platform-dependent
  Backup approach C: Logical replication + PITR (Point-In-Time Recovery) — most granular; complex

**Node 4 — Trade-off analysis**
  Volume snapshot: fast (< 30 min for 1TB); consistent; chosen for PostgreSQL, Redis
  PITR: enables restoration to any point; chosen for critical payment DB only
  Cross-region: S3 cross-region replication; encrypted at rest (AES-256); 15-minute replication lag acceptable

**Node 5 — ADR**
  Decision: EBS volume snapshots (hourly) + PITR for payments DB + S3 cross-region backup replication
  Rationale: Volume snapshots meet RTO < 30 min; PITR for payment data provides granular recovery
  Trade-off: Two backup mechanisms to maintain; justified by payment data criticality

**Node 6 — Component placement**
  Snapshot schedule: hourly via AWS Backup | Retention: 30-day lifecycle policy
  Cross-region: S3 CRR (Cross-Region Replication) to backup region with server-side encryption
  Restore testing: monthly automated restore drill via CI/CD (Continuous Integration/Continuous Deployment) pipeline

**Node 7 — Validation**
  Backup completion: 1TB snapshot in 22 minutes → PASS (< 4 hours)
  Restore drill: latest snapshot restored in 18 minutes → PASS (< 30 min)
  Cross-region lag: S3 CRR replication lag measured at 8 minutes → within 15-minute acceptable window → PASS
  Arbiter: ArchitectureCompletenessArbiter

**Node 8 — Output**
  ADR: Platform-ADR-012-backup-restore.md | Data protection topology diagram

**Node 9 — Verdict**
  PASS — Backup and restore architecture complete; frequency, retention, cross-region replication, and restore validation all defined

