# Layer 1 — Transition Orchestrator Pairs (020)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: transition-orchestrator

## B18-L1-020-001 — SessionData: single migration orchestration

**Scenario:** SessionDataMigration — IMPLEMENT only, feature_addition, 4.4

**Orchestrator sequence:**
```
1. [DataMigrationFlow44]
   → Migrate session data from Redis to PostgreSQL session_store
   → N08 Gate B: count(session_store) == count(legacy_session_data)
   → N09 output
```

---

## B18-L1-020-002 — SessionData: IMPLEMENT+SECURITY token encryption

**Orchestration sequence:**
```
1. DataMigrationFlow44 — migrate sessions; encrypt tokens in transit
2. SecurityReviewFlow — verify all session tokens encrypted at rest; no plaintext
3. N08: migrated_count == source AND all_tokens_encrypted
4. N09 output
```

---

## B18-L1-020-003 — SessionData: IMPLEMENT+ARCHITECTURE+SECURITY

**Three-flow orchestration:**
```
1. ArchitectureFlow31 — design session storage schema with encryption
2. DataMigrationFlow44 — migrate per schema spec; apply encryption
3. SecurityReviewFlow — verify encryption and token security
4. N08: all three Gate B conditions
5. N09 output
```

---

## B18-L1-020-004 — SessionData: code refactoring separate from data migration

**Two-flow sequential orchestration:**
```
1. BackendServiceFlow41 — refactor SessionManager; extract ISessionStore
2. DataMigrationFlow44 — migrate session records using new ISessionStore
3. N08: all session tests pass AND migrated_count == source
4. N09 output
```

Service refactoring precedes data migration when both present.

---

## B18-L1-020-005 — SessionData: orchestrator output metadata

**Orchestrator metadata for SessionDataMigration+SECURITY:**
```
OrchestratorOutput {
  execution_plan: [
    {step: 1, flow: "DataMigrationFlow44", parallel_with: null},
    {step: 2, flow: "SecurityReviewFlow", parallel_with: null}
  ],
  connector_name: "SessionDataMigration",
  transition_type: "feature_addition",
  domain_id: "software-development",
  gate_b_conditions: [
    "migrated_count == source_count",
    "all_session_tokens_encrypted"
  ],
  rollback_plan: ["SessionDataMigration_down.sql", "restore_session_store"]
}
```
