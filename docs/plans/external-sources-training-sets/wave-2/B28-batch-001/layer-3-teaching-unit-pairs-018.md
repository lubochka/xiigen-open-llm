# Layer 3 — Teaching Unit Pairs (018)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU018-001 — QA+IMPLEMENT: migration runner

A: "Build the database migration runner and QA the schema change."
B: ["IMPLEMENT", "QA"] — "build" → IMPLEMENT; "migration runner" → IMPLEMENT via "runner" code artifact; "QA" → QA.
Reason: Two predicates. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## TU018-002 — TEST fires from "validate" in migration context

A: "Run the database migration script and validate row counts."
B: ["TEST"] — "run migration script" → gap; "validate" → TEST fires on "validate."
Reason: Migration execution is gap. "validate" fires TEST regardless of context. One predicate. TransitionType: documentation.
CompiledPredicateIds: ["TEST"]

---

## TU018-003 — Gap: migration execution alone

A: "Execute the migration script against the production database."
B: [] — "execute migration" → gap; "production database" → gap.
Reason: Migration execution vocabulary → gap. knowledge_edge.
CompiledPredicateIds: []

---

## TU018-004 — QA+DOCUMENT: migration release criteria

A: "Document the database migration release criteria and QA review before cutover."
B: ["DOCUMENT", "QA"] — "document" → DOCUMENT; "criteria" → DOCUMENT (dedup); "QA review" → QA; "cutover" → gap.
Reason: Two predicates. TransitionType: documentation.
CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## TU018-005 — QA+IMPLEMENT+TEST+DOCUMENT: full migration lifecycle

A: "Build the migration runner, write rollback tests, document the cutover runbook, and QA gate."
B: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"] — four predicates. TransitionType: feature_addition.
Reason: "build" → IMPLEMENT; "write tests" → TEST; "document" → DOCUMENT; "QA gate" → QA. QA last.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"]
