# Layer 0-C — Knowledge-Edge Pairs (047)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: PostgreSQL connector

---

## B17-0C-047-001 — PostgreSQL: non-trigger terms

**Proper nouns — no predicate:** PostgreSQL, Postgres, PgAdmin
**Domain nouns — no predicate:** table, column, row, schema, query, transaction, index, constraint, foreign key, trigger (database), stored procedure, view
**Acronyms — no predicate:** SQL, DDL (Data Definition Language), DML (Data Manipulation Language), JDBC (Java Database Connectivity), PGDUMP

---

## B17-0C-047-002 — PostgreSQL: "query" and "migrate" are not IMPLEMENT triggers

"Query the PostgreSQL database" → no predicate.
"Migrate the schema to PostgreSQL" → no predicate ("migrate" is not in the IMPLEMENT trigger list).
Only explicit build verbs fire IMPLEMENT.

---

## B17-0C-047-003 — PostgreSQL: row-level security phrase does not fire SECURITY

"Row-level security" is a PostgreSQL feature name — it is a domain phrase, not a SECURITY trigger. SECURITY fires only on token, authenticate/authentication, or HMAC-SHA256 validation phrases.

---

## B17-0C-047-004 — PostgreSQL: API_CONTRACT + SECURITY combo teaching point

When a PostgreSQL connector input contains both "connector" and "bearer token":
- API_CONTRACT fires from "connector" (Rule 4)
- SECURITY fires from "bearer token" (Rule 6 — "token" present)
- These are two independent triggers that produce two predicates
- Both route independently: API_CONTRACT → 3.3, SECURITY → 3.4
- DecisionCount: 3 (IMPLEMENT at 4.3, API_CONTRACT at 3.3, SECURITY at 3.4)
