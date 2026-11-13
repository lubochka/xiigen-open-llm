# Layer 0-D — Flow Factory Pairs (031)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-031-001 — ShardingMigration factory entry

**Scenario:** "Write a migration to move the users table to a sharded architecture across 4 nodes."

```
FlowFactoryEntry {
  ScenarioId: "ShardingMigration",
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "ArchitectureFlow31", type: "schema_review_complete",
     condition: "shard_architecture_spec_approved == true"},
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "sum(count(shard_1), count(shard_2), count(shard_3), count(shard_4)) == count(source_users)"}
  ]
}
```

---

## B18-0D-031-002 — ShardingMigration refactoring factory entry

**Scenario:** "Restructure the payments table for horizontal sharding."

```
FlowFactoryEntry {
  ScenarioId: "PaymentsTableShardingRefactor",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "refactoring",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "row_count_assertion",
     condition: "count(all_shards) == count(source_payments) AND data_integrity_preserved == true"}
  ]
}
```

---

## B18-0D-031-003 — ReplicationSetup factory entry

**Scenario:** "Write a migration to configure streaming replication between primary and replica."

```
FlowFactoryEntry {
  ScenarioId: "ReplicationSetupMigration",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "replication_active == true AND replication_lag_seconds == 0"}
  ]
}
```

---

## B18-0D-031-004 — ReplicationSetup with SECURITY factory entry

**Scenario:** "Set up encrypted TLS replication between database nodes."

```
FlowFactoryEntry {
  ScenarioId: "EncryptedReplicationSetup",
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "tls_replication_active == true AND replication_lag_seconds == 0"},
    {flow: "SecurityReviewFlow", type: "security_scan_pass",
     condition: "all_replication_traffic_encrypted == true AND certificate_valid == true"}
  ]
}
```

---

## B18-0D-031-005 — ReadReplicaPromotion factory entry

**Scenario:** "Write a migration to promote the read replica to primary during failover."

```
FlowFactoryEntry {
  ScenarioId: "ReadReplicaPromotion",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "new_primary_accepts_writes == true AND old_primary_demoted == true"}
  ]
}
```
