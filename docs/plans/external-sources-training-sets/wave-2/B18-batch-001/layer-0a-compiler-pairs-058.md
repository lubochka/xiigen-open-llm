# Layer 0-A — Compiler Pairs (058)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-058-001 — Complete B18 predicate rule summary: 001-060

**The complete B18 predicate rule set (all 4.4 domains 001-060):**

| Predicate | Absolute fires | Does NOT fire |
|---|---|---|
| IMPLEMENT | "write migration", "add migration", "migrate data", "create table/index", "seed", "populate", "archive" (verb), "mask" (verb), "anonymize" (verb), "implement" | "the migration is complete", "migration process" (nouns) |
| ARCHITECTURE | "structure" (noun), "architecture" (noun), "pattern" (noun), "design" (verb) | "structured" (adjective), "the design" (noun), "architectural decision" (noun), "design document" (noun phrase) |
| SECURITY | "PII", "GDPR", "encrypted", "sensitive data", "credentials", "authentication", "authorization", "access control", "mask" (PII context), "anonymize", "TLS", "row-level security" | "trigger" (DB object), "sequence", "index", "partition", "archive" (without PII), "audit" (without PII) |
| TEST | "write tests", "add tests", "create tests" | "tests" (noun alone), "test data", "testing" (noun), "verify", "integration testing" (noun phrase), "the tests passed" |
| DOCUMENT | "document" (verb), "annotate", "spec out", "write documentation", "write runbook" | "the documentation" (noun), "documented schema" (adjective), "document table" (noun = DB object) |

---

## B18-0A-058-002 — Complete B18 TransitionType rule summary: 001-060

**TransitionType determination (all 4.4 domains 001-060):**

| Verb | TransitionType | Rule |
|---|---|---|
| refactor | refactoring | Rule 9 (absolute) |
| consolidate | refactoring | Rule 20 (absolute) |
| restructure | refactoring | Rule 1 (absolute) |
| rename | refactoring | Rule 9 |
| reorganize | refactoring | Rule 1 |
| clean up | refactoring | Rule 1 |
| move (code) | refactoring | Rule 1 |
| write migration | feature_addition | Rule 2 (absolute) |
| migrate (data) | feature_addition | Rule 5 |
| create (table/index) | feature_addition | Rule 2 |
| add (column/constraint) | feature_addition | Rule 2 |
| seed / populate | feature_addition | Rule 6 |
| anonymize / mask | feature_addition | Rule 2 (new artifact) |
| archive (data) | feature_addition | Rule 5 |
| promote (replica) | feature_addition | Rule 5 |

---

## B18-0A-058-003 — Complete B18 sub-phase routing rule summary: 001-060

**Sub-phase routing for all B18 IMPLEMENT scenarios:**

| Vocabulary present | Sub-phase | Flow |
|---|---|---|
| "write migration", "migrate data", "seed", "populate", "archive", "anonymize", "mask", "create table/index", "DDL" | 4.4 | DataMigrationFlow44 |
| "service", "class", "method", "repository", "module", "controller" | 4.1 | BackendServiceFlow41 |
| "API connector", "webhook", "endpoint", "API contract", "integration" | 4.3 | ApiContractFlow43 |
| "structure"/"architecture"/"pattern"/"design" | 3.1 | ArchitectureFlow31 |
| "write tests"/"add tests" | 5.x | UnitTestingFlow51 |
| "document"/"annotate"/"spec out" | 2.1 | DocumentationFlow21 |
| "PII"/"encrypted"/"GDPR"/"credentials" | security | SecurityReviewFlow |
| "QA" abbreviation | QA (last) | QA flow |

---

## B18-0A-058-004 — Complete B18 orchestration ordering rules summary

**B18 orchestration ordering rules (all absolute, all domains 001-060):**

| Rule | Constraint |
|---|---|
| ARCHITECTURE before IMPLEMENT | ArchitectureFlow31 always precedes DataMigrationFlow44 |
| SECURITY after IMPLEMENT | SecurityReviewFlow always after migration completes |
| TEST after IMPLEMENT | UnitTestingFlow51 always after migration completes |
| SECURITY ‖ TEST | SecurityReviewFlow and UnitTestingFlow51 are parallel-eligible |
| DOCUMENT parallel-eligible | DocumentationFlow21 can run alongside IMPLEMENT, SECURITY, or TEST |
| QA always last | QA flow runs after all other flows; never implied |
| No GapHandler | B18 has no gap-handling flow |

---

## B18-0A-058-005 — Complete B18 factory mode progression summary

**FlowFactory mode lifecycle for all B18 scenarios:**

| Stage | Factory mode | Triggered by |
|---|---|---|
| First run, no history | deterministic_generic | No prior execution record |
| First run PASS | probationary_accepted | Gate B PASS on first execution |
| Repeated PASS history | trusted_learned | Gate B PASS on ≥2 executions |
| Any Gate B FAIL | probationary_rejected | Gate B FAIL; rollback executed |
| After probationary_rejected fix + PASS | probationary_accepted | Fix applied; Gate B PASS |

**All new domains (037-060) start at deterministic_generic on first encounter. No domain starts at trusted_learned without execution history.**
