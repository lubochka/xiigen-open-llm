# Layer 1 — Transition Orchestrator Pairs (052)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-052-001 — StorageClasses + BackupPolicy combined: TransitionType for shared predicates

Both StorageClasses and BackupPolicy share: ARCHITECTURE + IMPLEMENT + SECURITY → feature_addition
Sequential: 3.5 → 6.3 → 4.1 | This ordering applies regardless of the specific topic domain.

---

## B14-trans-052-002 — SnapshotSchedule + TLSCertRotation: both fire SECURITY from different compound phrases

SnapshotSchedule: SECURITY fires from "rotate snapshot encryption keys" (key rotation)
TLSCertRotation: SECURITY fires from "TLS certificate rotation" (key rotation compound)
Both route to SecurityAuditFlow63 (6.3) — same sub-phase regardless of which compound phrase triggered.
TransitionType for both + IMPLEMENT: feature_addition; Sequential: 6.3 → 4.1

---

## B14-trans-052-003 — MessageQueue + EventBusConfig + DatabaseCluster: API_CONTRACT fires from all three

All three topics can produce API_CONTRACT:
  MessageQueue: "define message schema contract" → API_CONTRACT
  EventBusConfig: "specify CloudEvents schema" → API_CONTRACT
  DatabaseCluster: "define database connection API contract" → API_CONTRACT
  All route to APIContractFlow21 (2.1).

---

## B14-trans-052-004 — NetworkPolicy + PodSecurityPolicy + AdmissionControllers: all produce SECURITY + IMPLEMENT combo

Common pattern across security-infrastructure topics:
  "Implement [security manifest] and enforce [security constraint]" → IMPLEMENT + SECURITY
  Sequential: 6.3 → 4.1 | feature_addition | PrimarySubPhaseId: 4.1 (when ARCHITECTURE absent)
  Consistent ordering across all three topic domains.
