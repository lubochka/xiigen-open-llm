# Layer 3 — Teaching Unit Pairs (012)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: teaching-unit

## B18-TU-012-001 — Complete B18 teaching summary: all rules consolidated

**B18 — Development — refactoring and data migration**
**The 20 rules condensed to 7 must-know points:**

1. **"structure" → ARCHITECTURE (always, no exceptions)**
2. **"refactor" → TransitionType: refactoring (always wins)**
3. **"seed"/"fixture" → 4.4 (not 5.x)**
4. **"ETL"/"transform"/"load" (data) → 4.4 (not 4.3)**
5. **"write migration" → feature_addition, 4.4**
6. **Data migration → DataMigrationFlow44 (4.4), not BackendServiceFlow41 (4.1)**
7. **DomainId → "software-development" (always)**

---

## B18-TU-012-002 — Teaching summary: predicate triggers

**IMPLEMENT fires on:** refactor, extract, rename, consolidate, restructure, reorganize, move, clean up, migrate, write/create/build migration, seed, fixture, ETL, transform (data), load (data)

**ARCHITECTURE fires on:** structure (always!), pattern, architecture, design (structural noun)

**TEST fires on:** write/create/add tests, unit tests, integration tests

**DOCUMENT fires on:** document, spec out, annotate, describe, write spec

**SECURITY fires on:** GDPR, encrypted, PII, compliance, security (data context)

**No GapHandler in B18.**

---

## B18-TU-012-003 — Teaching summary: TransitionType determination

**TransitionType hierarchy:** refactoring > feature_addition > documentation

**Refactoring verbs:** refactor, rename, consolidate, extract (code), restructure, reorganize, move (code), clean up

**Feature_addition verbs:** write migration, seed, fixture, ETL, migrate (data), transform (data), load (data)

**Conflict resolution (when both present):**
- Rule 9: "refactor" always wins → TransitionType: refactoring
- Exception: "write migration to [refactoring action]" → feature_addition (migration is the artifact)

---

## B18-TU-012-004 — Teaching summary: sub-phase routing

**Sub-phase 4.4 vocabulary:** migrate, migration, seed, fixture, ETL, transform, load, data, records, table, database, batch, archive, CSV

**Sub-phase 4.1 vocabulary:** service, class, method, function, module, repository, domain, business logic, rule, handler

**Sub-phase 3.1 (ARCHITECTURE):** always 3.1 in B18 (application/domain architecture)

**Sub-phase 5.x (TEST):** write/add/create tests

**Sub-phase 2.1 (DOCUMENT):** document, annotate, spec out

---

## B18-TU-012-005 — Teaching summary: orchestration ordering

**B18 orchestration order:**
1. ARCHITECTURE (schema/pattern design first)
2. IMPLEMENT (primary artifact — migration or refactoring)
3. SECURITY (review after implementation)
4. TEST (verify after implementation)
5. DOCUMENT (parallel-eligible; can run concurrently with IMPLEMENT for policy/spec docs)
6. QA (always last if present; fires only on "QA" abbreviation)

**Critical dependencies:**
- TEST always waits on IMPLEMENT
- ARCHITECTURE always precedes IMPLEMENT when both present
- QA always last

---

## B18-TU-012-006 — Teaching summary: v6 corruptions to detect and correct

**Three v6 corruptions in B18:**

| Corruption | Detection | Correction |
|---|---|---|
| v6-01 | "structure" in request + ARCHITECTURE absent | Add ARCHITECTURE; add ArchitectureFlow31 |
| v6-02 | Refactoring verb in request + TransitionType: feature_addition | Set TransitionType: refactoring |
| v6-03 | Migration/data vocabulary + BackendServiceFlow41 routing | Route to DataMigrationFlow44 (4.4) |

**All three corruptions represent failures to apply literal rules:**
- v6-01: applied contextual suppression to a literal rule
- v6-02: applied outcome-based reasoning instead of verb-based classification
- v6-03: applied a default routing instead of vocabulary-based disambiguation

**Prevention:** apply all B18 rules literally and sequentially. No contextual override. No outcome-based reasoning for TransitionType or predicate determination.
