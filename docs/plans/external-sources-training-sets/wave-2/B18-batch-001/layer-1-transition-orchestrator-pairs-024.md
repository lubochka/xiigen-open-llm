# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: transition-orchestrator

## B18-L1-024-001 — Complete orchestration ordering rules reference

**B18 orchestration ordering rules (all absolute):**

| Rule | Ordering constraint |
|---|---|
| ARCHITECTURE before IMPLEMENT | ArchitectureFlow31 must complete before DataMigrationFlow44 or BackendServiceFlow41 |
| SECURITY after IMPLEMENT | SecurityReviewFlow always after IMPLEMENT flow |
| TEST after IMPLEMENT | UnitTestingFlow51 always after IMPLEMENT flow (hard dependency) |
| DOCUMENT parallel-eligible | DocumentationFlow21 can run with IMPLEMENT for spec/policy/runbook docs |
| QA always last | QA flow (if present) runs after all other flows; never implied |
| No GapHandler | B18 has no gap-handling flow; gap vocabulary does not activate any flow |

---

## B18-L1-024-002 — Complete orchestration: parallel vs sequential matrix

**Parallel eligible combinations:**

| Flow A | Flow B | Parallel? | Condition |
|---|---|---|---|
| DataMigrationFlow44 | DocumentationFlow21 | YES | Doc is spec/runbook (not as-built) |
| SecurityReviewFlow | UnitTestingFlow51 | YES | Both run after IMPLEMENT completes |
| SecurityReviewFlow | DocumentationFlow21 | YES | Security review and doc are independent |
| ArchitectureFlow31 | DataMigrationFlow44 | NO | Schema must be complete before migration |
| DataMigrationFlow44 | UnitTestingFlow51 | NO | Tests verify migration; hard dependency |
| DataMigrationFlow44 | SecurityReviewFlow | NO | Security reviews migration output |

---

## B18-L1-024-003 — Complete orchestration: rollback plan requirement

**Every migration orchestration plan includes:**
1. Rollback trigger: "N08 Gate B FAIL"
2. Rollback action: execute {ConnectorName}_down.sql
3. Rollback verification: count(source) restored to pre-migration count
4. Rollback logging: rollback event logged with timestamp and reason
5. No auto-retry: operator must investigate before re-attempting

**Seeding rollback:**
- Delete seeded rows (where seeded_at == this_run)
- Verify table is empty (or back to pre-seed state)

---

## B18-L1-024-004 — Complete orchestration: DomainId invariant

**DomainId invariant:**
- DomainId: "software-development" is set at Layer 0-A
- Persists unchanged through all orchestration layers
- N09 output always includes DomainId: "software-development"
- This is absolute in B18; no scenario changes DomainId

---

## B18-L1-024-005 — Complete orchestration: QA placement rule

**QA placement rules (critical to memorize):**

| Signal | QA fires? | QA placement |
|---|---|---|
| "QA" abbreviation in request | YES | Always last in execution plan |
| "verify" | NO | Never implies QA |
| "integration tests" | NO | Fires TEST, not QA |
| "review" | NO | Fires SECURITY if security context; otherwise no QA |
| "quality assurance" (spelled out) | YES | QA fires on full phrase too |
| "QA testing" | YES | "QA" abbreviation present |

**QA is the only flow that is position-locked (always last). All other flows follow priority-based ordering.**
