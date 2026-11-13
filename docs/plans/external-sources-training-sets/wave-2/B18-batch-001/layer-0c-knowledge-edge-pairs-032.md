# Layer 0-C — Knowledge-Edge Pairs (032)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-032-001 — Ten disambiguation test questions for 025-036 domains

**Q1:** "create index on the email column" → ARCHITECTURE fires? **No.** "index" alone is not an ARCHITECTURE trigger.

**Q2:** "design the index structure" → ARCHITECTURE fires? **Yes.** "design" (verb) + "structure" (noun) both fire ARCHITECTURE.

**Q3:** "database trigger for GDPR audit" → SECURITY fires? **Yes.** GDPR fires SECURITY; "trigger" itself does not.

**Q4:** "database trigger" alone → SECURITY fires? **No.** "trigger" is a DDL object token, not a security vocabulary token.

**Q5:** "restructure table into partitions" → TransitionType? **refactoring.** "restructure" is a refactoring verb (Rule 1).

**Q6:** "migrate orders table to partitioned table structure" → predicates? **[IMPLEMENT, ARCHITECTURE].** "migrate" fires IMPLEMENT; "structure" fires ARCHITECTURE.

---

## B18-0C-032-002 — More disambiguation test questions

**Q7:** "rename enum value" → TransitionType? **refactoring.** "rename" is a refactoring verb (Rule 9).

**Q8:** "add JSONB column with PII fields" → predicates? **[IMPLEMENT, SECURITY].** "add JSONB" fires IMPLEMENT; "PII" fires SECURITY.

**Q9:** "restructure JSONB column layout" → predicates? **[IMPLEMENT].** "restructure" fires IMPLEMENT+refactoring; no "structure"/"architecture"/"pattern" token present.

**Q10:** "structured JSONB schema" → ARCHITECTURE fires? **No.** "structured" is adjective; only "structure" noun fires ARCHITECTURE.

---

## B18-0C-032-003 — Stored procedure 4.1 vs 4.4 disambiguation edges

**Why stored procedures route to 4.4 and not 4.1:**

| Vocabulary signal | Sub-phase signal | Flow |
|---|---|---|
| "stored procedure" (database object) | 4.4 (database DDL) | DataMigrationFlow44 |
| "service class method" | 4.1 (code) | BackendServiceFlow41 |
| "migrate SQL to stored procedure" | 4.4 (database migration) | DataMigrationFlow44 |
| "refactor the OrderService" | 4.1 (service code) | BackendServiceFlow41 |
| "write stored procedure for data pipeline" | 4.4 | DataMigrationFlow44 |

**Key edge:** Stored procedures are database objects (4.4). Service classes and domain handlers are 4.1. The vocabulary of the primary target determines sub-phase.

---

## B18-0C-032-004 — "replace" token: no refactoring edge

**"replace" in DDL context:**

| Phrase | TransitionType | Reason |
|---|---|---|
| "replace the old trigger with a new migration" | feature_addition | "replace" is not a refactoring verb |
| "replace deprecated trigger" | feature_addition | new artifact = feature_addition |
| "refactor the trigger" | refactoring | "refactor" verb present |
| "replace view with materialized view" | feature_addition | new artifact replaces old |

**Rule:** "replace" (swap-in vocabulary) → feature_addition. "refactor" → refactoring. Not interchangeable.

---

## B18-0C-032-005 — "convert" token: feature_addition, not refactoring

**"convert" in DDL context:**

| Phrase | TransitionType | Reason |
|---|---|---|
| "convert timestamps to timestamptz" | feature_addition | "convert" = transform, not refactoring verb |
| "convert column type to JSONB" | feature_addition | "convert" is feature_addition |
| "convert nullable to NOT NULL" | feature_addition | "convert" is not a refactoring verb |
| "rename and convert" | refactoring | "rename" refactoring verb wins |

**Rule:** "convert" alone does NOT fire refactoring. It is a transformation verb that maps to feature_addition + 4.4 in database migration context.
