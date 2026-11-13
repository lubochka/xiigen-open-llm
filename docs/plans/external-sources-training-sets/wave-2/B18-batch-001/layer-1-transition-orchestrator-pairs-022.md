# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: transition-orchestrator

## B18-L1-022-001 — ApiKeyRotation: IMPLEMENT+SECURITY orchestration

**Scenario:** ApiKeyRotationMigration — IMPLEMENT+SECURITY, feature_addition

**Orchestration sequence:**
```
1. DataMigrationFlow44 — migrate api_credentials; hash keys with BCRYPT
2. SecurityReviewFlow — verify all keys hashed; old keys invalidated
3. N08: count match AND all_keys_hashed AND old_keys_invalidated
4. N09 output
```

---

## B18-L1-022-002 — ApiKeyRotation: three-flow orchestration (ARCH+IMPL+SEC)

**Orchestration sequence:**
```
1. ArchitectureFlow31 — design key rotation schema + versioned key management
2. DataMigrationFlow44 — migrate and hash keys per schema
3. SecurityReviewFlow — security verification
4. N08: all three Gate B conditions
5. N09 output
```

---

## B18-L1-022-003 — ApiKeyRotation: four-flow orchestration (ARCH+IMPL+SEC+DOC)

**Orchestration sequence:**
```
1. ArchitectureFlow31 — design key rotation schema
2. DataMigrationFlow44 ‖ DocumentationFlow21
   Track A: DataMigrationFlow44 — migrate and hash keys
   Track B: DocumentationFlow21 — write key rotation runbook (parallel eligible)
3. SecurityReviewFlow — post-migration security review
4. N08: all four Gate B conditions
5. N09 output
```

DocumentationFlow21 drafts the runbook while DataMigrationFlow44 runs (parallel eligible).
SecurityReviewFlow runs after DataMigrationFlow44 completes.

---

## B18-L1-022-004 — ApiKeyRotation: code refactoring orchestration

**Scenario:** ApiKeyManagerRefactoring — IMPLEMENT, refactoring, 4.1

**Orchestration sequence:**
```
1. [BackendServiceFlow41]
   → Refactor ApiKeyManager; extract KeyRotationService
   → N07 regression: all existing API key tests pass
   → N08 Gate B: all_pre_refactor_tests_pass
   → N09 output
```

---

## B18-L1-022-005 — ApiKeyRotation: orchestration output metadata

**Orchestrator metadata for four-flow scenario:**
```
OrchestratorOutput {
  execution_plan: [
    {step: 1, flow: "ArchitectureFlow31", parallel_with: null},
    {step: 2, flow: "DataMigrationFlow44", parallel_with: ["DocumentationFlow21"]},
    {step: 3, flow: "SecurityReviewFlow", parallel_with: null}
  ],
  connector_name: "ApiKeyRotationMigration",
  transition_type: "feature_addition",
  domain_id: "software-development"
}
```
