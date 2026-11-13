# Layer 0-A — Compiler Pairs (056)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-056-001 — v6 corruption reference for 037-060 domains: v6-01

**v6-01 (ARCHITECTURE suppressed) — 037-060 examples:**

| Request fragment | Should fire ARCHITECTURE | Token missed |
|---|---|---|
| "anonymization pattern for GDPR" | YES | "pattern" (Rule 7) |
| "sharded architecture" | YES | "architecture" (Rule 7) |
| "full-text search index structure" | YES | "structure" (Rule 3) |
| "CQRS projection pattern" | YES | "pattern" (Rule 7) |
| "multi-tenant schema structure" | YES | "structure" (Rule 3) |
| "soft-delete pattern" | YES | "pattern" (Rule 7) |
| "rollback architecture" | YES | "architecture" (Rule 7) |
| "event sourcing architecture" | YES | "architecture" (Rule 7) |

**Rule 3 (structure) and Rule 7 (architecture, pattern) are absolute in all 037-060 domains. No domain vocabulary suppresses them.**

---

## B18-0A-056-002 — v6 corruption reference for 037-060 domains: v6-02 and v6-03

**v6-02 (wrong TransitionType) — 037-060 examples:**

| Request fragment | Refactoring verb | Correct TransitionType |
|---|---|---|
| "restructure table for sharding" | restructure | refactoring |
| "consolidate anonymization migrations" | consolidate | refactoring |
| "rename deleted_at to removed_at" | rename | refactoring |
| "refactor rollback scripts" | refactor | refactoring |
| "consolidate cross-database migrations" | consolidate | refactoring |

**v6-03 (wrong sub-phase) — 037-060 examples:**

| Request fragment | Correct sub-phase | v6-03 error |
|---|---|---|
| "anonymize customer data migration" | 4.4 | Routed to 4.1 |
| "create composite index" | 4.4 | Routed to 4.1 |
| "write rollback script" | 4.4 | Routed to 4.1 |
| "migrate from MongoDB to PostgreSQL" | 4.4 | Routed to 4.3 (API connector) |
| "seed test fixtures" | 4.4 | Routed to 5.x |

---

## B18-0A-056-003 — v6 new type: v6-04 (SECURITY suppressed for anonymization vocabulary)

**v6-04 (NEW) — SECURITY suppressed when anonymization/masking vocabulary present:**

**Correct behavior:** "anonymize", "mask", "PII", "GDPR" ALWAYS fire SECURITY. No context suppresses them.

**v6-04 error pattern:** v6 classifier treats "anonymize" as a data migration verb only (maps to IMPLEMENT) without firing SECURITY.

| Request fragment | Should fire | v6-04 error |
|---|---|---|
| "anonymize PII fields" | IMPLEMENT + SECURITY | Only IMPLEMENT returned |
| "write migration to mask credit cards" | IMPLEMENT + SECURITY | Only IMPLEMENT returned |
| "GDPR-compliant archival migration" | IMPLEMENT + SECURITY | GDPR token missed |
| "encrypted test data migration" | IMPLEMENT + SECURITY | encrypted token missed |

**Detection:** SECURITY absent when anonymization/masking/PII/GDPR/encrypted tokens present.
**Fix:** Always fire SECURITY on: anonymize, mask, PII, GDPR, encrypted, sensitive, credentials, access control.

---

## B18-0A-056-004 — "document" disambiguation in 049-060 domains

**When DOCUMENT fires vs when it does not:**

| Request fragment | DOCUMENT fires? | Reason |
|---|---|---|
| "document the migration runbook" | YES | "document" verb (Rule 4) |
| "document the audit trail policy" | YES | "document" verb (Rule 4) |
| "annotate the rollback steps" | YES | "annotate" = DOCUMENT trigger (Rule 4) |
| "document the soft-delete usage pattern" | YES — DOCUMENT fires | "document" verb overrides "pattern" for ARCHITECTURE |
| "write documentation for the migration" | YES | "write documentation" = DOCUMENT trigger |
| "the documentation is already written" | NO | noun form; no document verb |
| "spec out the migration steps" | YES | "spec out" = DOCUMENT trigger (Rule 4) |

---

## B18-0A-056-005 — 037-060 QA vs TEST disambiguation

**QA vs TEST in 037-060 migration scenarios:**

| Signal | Predicate | Notes |
|---|---|---|
| "write tests to verify migration" | TEST | "write tests" verb form |
| "add tests for rollback" | TEST | "add tests" verb form |
| "QA the anonymization migration" | QA (always last) | "QA" abbreviation = QA predicate |
| "quality assurance of the migration" | QA (always last) | Spelled-out form = QA |
| "verify the migration completed" | NONE | "verify" alone = neutral |
| "integration testing the migration" | NONE | Noun form = no TEST predicate |
| "the tests passed" | NONE | Past tense noun form = no TEST predicate |

**QA is always position-locked last. TEST fires only from write/add/create verb forms.**
