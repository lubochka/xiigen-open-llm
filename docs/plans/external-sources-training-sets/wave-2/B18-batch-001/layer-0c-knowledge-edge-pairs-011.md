# Layer 0-C — Knowledge-Edge Pairs (011)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: knowledge-edge

## B18-0C-011-001 — Complete B18 vocabulary table: all verbs/nouns → predicate + sub-phase

**Section A: IMPLEMENT triggers (full vocabulary)**

| Vocabulary | TransitionType | Sub-phase | Flow |
|---|---|---|---|
| refactor | refactoring | 4.1 | BackendServiceFlow41 |
| extract (code) | refactoring | 4.1 | BackendServiceFlow41 |
| rename | refactoring | 4.1 | BackendServiceFlow41 |
| consolidate (code) | refactoring | 4.1 | BackendServiceFlow41 |
| restructure | refactoring | 4.1 | BackendServiceFlow41 |
| reorganize | refactoring | 4.1 | BackendServiceFlow41 |
| move (code) | refactoring | 4.1 | BackendServiceFlow41 |
| clean up | refactoring | 4.1 | BackendServiceFlow41 |
| write migration | feature_addition | 4.4 | DataMigrationFlow44 |
| create migration | feature_addition | 4.4 | DataMigrationFlow44 |
| build migration | feature_addition | 4.4 | DataMigrationFlow44 |
| seed | feature_addition | 4.4 | DataMigrationFlow44 |
| fixture | feature_addition | 4.4 | DataMigrationFlow44 |
| migrate (data) | feature_addition | 4.4 | DataMigrationFlow44 |
| transform (data) | feature_addition | 4.4 | DataMigrationFlow44 |
| ETL | feature_addition | 4.4 | DataMigrationFlow44 |
| load (data) | feature_addition | 4.4 | DataMigrationFlow44 |
| extract (data) | feature_addition | 4.4 | DataMigrationFlow44 |

---

## B18-0C-011-002 — Vocabulary table section B: ARCHITECTURE triggers

| Vocabulary | Predicate | Sub-phase | Flow |
|---|---|---|---|
| structure | ARCHITECTURE | 3.1 | ArchitectureFlow31 |
| architecture | ARCHITECTURE | 3.1 | ArchitectureFlow31 |
| pattern | ARCHITECTURE | 3.1 | ArchitectureFlow31 |
| design (structural) | ARCHITECTURE | 3.1 | ArchitectureFlow31 |
| redesign | ARCHITECTURE | 3.1 | ArchitectureFlow31 |
| schema + design | ARCHITECTURE | 3.1 | ArchitectureFlow31 |

---

## B18-0C-011-003 — Vocabulary table section C: TEST, DOCUMENT, SECURITY triggers

**TEST triggers:**
| Vocabulary | Predicate | Sub-phase | Flow |
|---|---|---|---|
| write tests | TEST | 5.x | UnitTestingFlow51 |
| create tests | TEST | 5.x | UnitTestingFlow51 |
| add tests | TEST | 5.x | UnitTestingFlow51 |
| unit tests | TEST | 5.1 | UnitTestingFlow51 |
| integration tests | TEST | 5.2 | UnitTestingFlow51 |
| verify (test context) | TEST | 5.x | UnitTestingFlow51 |

**DOCUMENT triggers:**
| Vocabulary | Predicate | Sub-phase | Flow |
|---|---|---|---|
| document | DOCUMENT | 2.1 | DocumentationFlow21 |
| spec out | DOCUMENT | 2.1 | DocumentationFlow21 |
| write spec | DOCUMENT | 2.1 | DocumentationFlow21 |
| describe | DOCUMENT | 2.1 | DocumentationFlow21 |
| annotate | DOCUMENT | 2.1 | DocumentationFlow21 |

**SECURITY triggers:**
| Vocabulary | Predicate | Sub-phase | Flow |
|---|---|---|---|
| GDPR | SECURITY | security | SecurityReviewFlow |
| encrypted/encryption | SECURITY | security | SecurityReviewFlow |
| PII | SECURITY | security | SecurityReviewFlow |
| compliance | SECURITY | security | SecurityReviewFlow |

---

## B18-0C-011-004 — Vocabulary table section D: Sub-phase signal words

**4.1 sub-phase signals (BackendServiceFlow41 context):**
service, class, method, function, module, repository, domain, rule, business logic, handler, controller, layer, interface, base class, dependency, logic

**4.4 sub-phase signals (DataMigrationFlow44 context):**
migrate, migration, seed, fixture, ETL, transform, load, data, records, table, database, schema change, batch, rollback, idempotent, CSV, flat file, warehouse, normalized, archive

---

## B18-0C-011-005 — B18 vocabulary that does NOT fire any predicate

**Words present in B18 requests that do NOT fire predicates:**
| Word | Why no predicate fires |
|---|---|
| "new" | Adjective; no predicate trigger |
| "legacy" | Adjective; marks source data only |
| "target" | Adjective; marks destination only |
| "existing" | Adjective; no trigger |
| "all" | Quantifier; no trigger |
| "correct" / "verify" | Verification context; TEST fires only if "write tests" present |
| "ensure" | Assurance language; no predicate on its own |
| "staging" | Environment noun; reinforces 4.4 but doesn't fire IMPLEMENT alone |
| "production" | Environment noun; context only |

---

## B18-0C-011-006 — B18 vocabulary interaction matrix (key conflicts)

**Top 5 vocabulary interaction conflicts in B18:**

1. **"refactor" + "write migration"**
   - Both fire IMPLEMENT; TransitionType conflict
   - Resolution: Rule 9 (refactor→refactoring) vs Rule 2 (write migration→feature_addition)
   - Winner: depends on whether migration is the primary artifact; Rule 9 usually wins

2. **"consolidate" + "write migration"**
   - consolidate → refactoring; write migration → feature_addition
   - Resolution: if migration is the artifact being created, feature_addition wins (Rule 2)

3. **"seed" + "write tests"**
   - seed → IMPLEMENT (4.4); write tests → TEST (5.x)
   - Resolution: both fire independently; no conflict

4. **"structure" + any request**
   - structure → ARCHITECTURE (Rule 3, absolute)
   - No conflict with any other predicate; ARCHITECTURE is additive

5. **"migrate" + "service vocabulary"**
   - migrate → 4.4; service → 4.1
   - Resolution: data context → 4.4; code context → 4.1; both contexts → both sub-phases
