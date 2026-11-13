# Layer 0-A — Compiler Pairs (057)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-057-001 — Advanced disambiguation: "migrate" in non-4.4 contexts

**Rule: "migrate" + data/records/tables vocabulary → sub-phase 4.4 (DataMigrationFlow44)**
**Rule: "migrate" + code/class/service vocabulary → sub-phase 4.1 (BackendServiceFlow41)**

| Request | Sub-phase | Reason |
|---|---|---|
| "migrate user records to new table" | 4.4 | "records" = data vocabulary |
| "migrate the UserService to a new module" | 4.1 | "service" = code vocabulary |
| "migrate product data from MongoDB" | 4.4 | "product data" = data vocabulary |
| "migrate the Repository pattern to interfaces" | 4.1 | "Repository pattern" = code vocabulary |
| "migrate indexes to the new schema" | 4.4 | "indexes" = DDL vocabulary |
| "migrate API endpoints to v3 format" | 4.3 | "API endpoints" = connector vocabulary |

---

## B18-0A-057-002 — Advanced disambiguation: "structure" in non-ARCHITECTURE contexts

**Rule: "structure" (noun) ALWAYS fires ARCHITECTURE regardless of context (Rule 3, absolute)**

| Request | ARCHITECTURE fires? | Reason |
|---|---|---|
| "normalize the data structure" | YES | "data structure" — "structure" is noun |
| "define the project structure" | YES | "project structure" — "structure" is noun |
| "add structure to JSON column" | YES | "structure" is noun; absolute |
| "structured data format" | NO | "structured" is adjective; not "structure" noun |
| "structured audit log" | NO | "structured" is adjective |
| "the schema is well-structured" | NO | "structured" is adjective |
| "the data has no structure" | YES | "structure" is noun |

---

## B18-0A-057-003 — Advanced disambiguation: "pattern" scope in migration requests

**Rule: "pattern" ALWAYS fires ARCHITECTURE (Rule 7, absolute)**
**Counter-examples for "pattern" that a model might miss:**

| Request | ARCHITECTURE fires? | Model failure mode |
|---|---|---|
| "the migration follows the standard pattern" | YES | "Describes existing behavior, not design" |
| "use the existing rollback pattern" | YES | "Refers to existing artifact" |
| "document the soft-delete usage pattern" | YES (but DOCUMENT also fires) | "document verb overrides" (incorrect; both fire) |
| "test the sharding migration pattern" | YES + possibly TEST | "test" verb + "pattern" noun |
| "archive the old data migration pattern docs" | YES | "archive" in non-migration sense + "pattern" |

**Absolute rule:** "pattern" (noun) fires ARCHITECTURE regardless of the surrounding verb.

---

## B18-0A-057-004 — Advanced disambiguation: multi-predicate collapse rules

**When multiple triggers of the same predicate fire, the output predicate appears exactly once:**

| Multiple triggers | Output predicates |
|---|---|
| "PII" + "GDPR" + "encrypted" | SECURITY (once) |
| "structure" + "architecture" + "pattern" | ARCHITECTURE (once) |
| "write migration" + "implement migration" + "add migration" | IMPLEMENT (once) |
| "write tests" + "add tests" + "create tests" | TEST (once) |
| "document" + "annotate" + "spec out" | DOCUMENT (once) |

**Key rule:** Predicates are a set, not a count. No matter how many times a trigger fires, the predicate appears once in the output set.

---

## B18-0A-057-005 — Advanced disambiguation: "design" as verb vs noun

**Rule: "design" as a verb fires ARCHITECTURE (Rule 11). "design" as a noun does not.**

| Request | ARCHITECTURE fires? | Reason |
|---|---|---|
| "design the schema" | YES | "design" is verb |
| "the design is complete" | NO | "design" is noun |
| "database design review" | NO | "design" is noun in noun phrase |
| "re-design the schema" | YES | "re-design" contains design verb |
| "good design practice" | NO | "design" is noun/adjective |
| "design and implement the schema" | YES | "design" is verb |
| "the schema design looks good" | NO | "schema design" is noun phrase |
