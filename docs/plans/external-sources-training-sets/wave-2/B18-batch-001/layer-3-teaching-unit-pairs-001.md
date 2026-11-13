# Layer 3 — Teaching Unit Pairs (001)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: teaching-unit

## B18-TU-001-001 — B18 teaching unit overview: what the model must learn

**B18 teaches the following skills:**

1. **Literal rule application:** "structure" always fires ARCHITECTURE (no context suppression)
2. **TransitionType determination:** refactoring verbs vs feature_addition verbs; hierarchy
3. **Sub-phase vocabulary:** 4.4 (migration/seed/ETL) vs 4.1 (service/class/refactoring)
4. **"migrate" disambiguation:** code context vs data context
5. **v6 corruption detection:** three specific corruption patterns and their corrections
6. **Flow selection:** DataMigrationFlow44 vs BackendServiceFlow41 based on TransitionType
7. **Orchestration ordering:** ARCHITECTURE first; TEST after IMPLEMENT; QA last
8. **Gate B types:** row_count (migration) vs test_regression (refactoring)

---

## B18-TU-001-002 — Teaching approach for B18

**Each teaching unit has four components:**

1. **Question:** What is the rule being taught?
2. **Example:** A concrete request demonstrating the rule
3. **Explanation:** Why this result is correct
4. **Anti-example:** What a corrupted classifier (v6) would do wrong, and why

---

## B18-TU-001-003 — Core teaching: TransitionType is determined by action verb

**Question:** How is TransitionType determined?

**Example:**
- Request A: "Refactor the OrderService to extract PaymentProcessor"
- Result A: TransitionType: refactoring (because "refactor" and "extract" are refactoring verbs)

- Request B: "Write the migration to move order history to the normalized schema"
- Result B: TransitionType: feature_addition (because "write migration" = Rule 2 new artifact)

**Explanation:** TransitionType is determined by the DOMINANT ACTION VERB. The hierarchy is: refactoring > feature_addition. If a refactoring verb is present, TransitionType is refactoring (unless "write migration" is the explicit primary task creating a migration artifact).

**Anti-example (v6-02):** Would classify "Refactor OrderService" as feature_addition because "a new PaymentProcessor class is created." Wrong: the dominant verb is "refactor" → refactoring TransitionType.

---

## B18-TU-001-004 — Core teaching: sub-phase is determined by vocabulary

**Question:** How is sub-phase (4.4 vs 4.1) determined?

**Example:**
- "seed the product database" → 4.4 (seed is 4.4 vocabulary)
- "refactor the ProductService" → 4.1 (service is 4.1 vocabulary; refactor is 4.1 refactoring)
- "migrate product records" → 4.4 (migrate + records is 4.4 vocabulary)

**Explanation:** Sub-phase routing is determined by co-occurrence vocabulary (Rule 6). Data vocabulary (migrate, seed, ETL, records, table, database) → 4.4. Service vocabulary (service, class, method, repository) → 4.1. Default in B18 is 4.4 (B18 is primarily a data migration scenario).

**Anti-example (v6-03):** Would route "seed the product database" to 4.1 (treating all IMPLEMENT as 4.1). Wrong: "seed" + "database" → 4.4.

---

## B18-TU-001-005 — Core teaching: predicates are additive

**Question:** Can multiple predicates fire at once?

**Yes. Predicates are additive — each fires independently.**

**Example:**
- "Migrate data AND design the schema structure AND write tests"
- IMPLEMENT fires (migrate)
- ARCHITECTURE fires (schema + structure — both triggers present)
- TEST fires (write tests)
- No predicate suppresses another
- Result: predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"]

**Anti-example (v6 suppression):** Would suppress ARCHITECTURE because "it's a migration context, not an architecture context." Wrong: each predicate fires independently by its own trigger. Context does not suppress predicates.

---

## B18-TU-001-006 — B18 teaching units roadmap

**Teaching units 002-012 cover:**

| File | Teaching topic |
|---|---|
| 002 | "structure" always fires ARCHITECTURE (detailed) |
| 003 | "seed"/"fixture" → IMPLEMENT → 4.4 (not 5.x) |
| 004 | ETL vocabulary: transform/load/extract → IMPLEMENT → 4.4 |
| 005 | 4.4 vs 4.1 disambiguation complete routing signal table |
| 006 | "migrate" disambiguation: code vs data context |
| 007 | TransitionType: refactoring vs feature_addition word lists |
| 008 | "rename" → IMPLEMENT, refactoring (not DOCUMENT) |
| 009 | ARCHITECTURE in unexpected places: database context |
| 010 | v6 corruption patterns (three types with examples) |
| 011 | Phase 4 sub-phase disambiguation: 4.1/4.3/4.4 vocabulary |
| 012 | Complete B18 teaching summary |
