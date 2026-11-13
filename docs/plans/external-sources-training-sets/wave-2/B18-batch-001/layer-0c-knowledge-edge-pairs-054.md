# Layer 0-C — Knowledge-Edge Pairs (054)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-054-001 — Edge reference: "consolidate" in migration context

**"consolidate" fires IMPLEMENT + TransitionType: refactoring in 037-060:**

| Request | Predicates | TransitionType |
|---|---|---|
| "consolidate anonymization migrations" | IMPLEMENT | refactoring |
| "consolidate PII masking migrations" + PII | IMPLEMENT + SECURITY | refactoring |
| "consolidate into a pattern" | IMPLEMENT + ARCHITECTURE | refactoring |
| "consolidate shard key constraints" | IMPLEMENT | refactoring |
| "consolidate rollback scripts" | IMPLEMENT | refactoring |
| "consolidate seed data migrations" | IMPLEMENT | refactoring |

---

## B18-0C-054-002 — Edge reference: "rename" in migration context

**"rename" fires IMPLEMENT + TransitionType: refactoring in 037-060:**

| Request | Predicates | TransitionType |
|---|---|---|
| "rename deleted_at to removed_at" | IMPLEMENT | refactoring |
| "rename schema_version to migration_version" | IMPLEMENT | refactoring |
| "rename tenant_id to organization_id" | IMPLEMENT | refactoring |
| "rename and add PII documentation" | IMPLEMENT + SECURITY + DOCUMENT | refactoring |

---

## B18-0C-054-003 — Edge reference: "refactor" in migration context

**"refactor" fires IMPLEMENT + TransitionType: refactoring in 037-060:**

| Request | Predicates | TransitionType |
|---|---|---|
| "refactor the rollback migration scripts" | IMPLEMENT | refactoring |
| "refactor the seed data migrations" | IMPLEMENT | refactoring |
| "refactor anonymization migration + design pattern" | IMPLEMENT + ARCHITECTURE | refactoring |
| "refactor cross-database migration + PII" | IMPLEMENT + SECURITY | refactoring |

---

## B18-0C-054-004 — Edge reference: "archive" as verb vs noun

**"archive" disambiguation:**

| Usage | IMPLEMENT fires? | Notes |
|---|---|---|
| "archive the old records" (verb) | YES | "archive" = migration action verb |
| "write migration to archive records" | YES | "write migration" = Rule 2 |
| "the archive table" (noun) | NO | "archive" = noun (table name) |
| "the archive is complete" (noun) | NO | "archive" = noun |
| "archive the PII data" | YES + SECURITY | Verb + PII token |

---

## B18-0C-054-005 — Complete knowledge-edge reference: all tokens summarized by Rule

**Complete Rule-to-token map for B18 037-060:**

| Rule | What it governs | Tokens |
|---|---|---|
| Rule 1 | restructure/reorganize/move (code) | refactoring TransitionType |
| Rule 2 | new artifacts | write migration, create/add/implement DB object |
| Rule 3 | "structure" (noun) | ARCHITECTURE absolute |
| Rule 4 | documentation | document, annotate, spec out, write runbook |
| Rule 5 | data migration verbs | migrate, archive, move data |
| Rule 6 | seed vocabulary | seed, populate, fixture |
| Rule 7 | "architecture"/"pattern" nouns | ARCHITECTURE absolute |
| Rule 9 | refactoring verbs | refactor, rename, consolidate, restructure |
| Rule 11 | "design" verb | ARCHITECTURE |
| Rule 12 | TEST verb forms | write tests, add tests, create tests |
| Rule 14 | security tokens | PII, GDPR, encrypted, sensitive, credentials, auth, access control, row-level security, TLS, anonymize, mask |
| Rule 20 | consolidate | refactoring TransitionType |
