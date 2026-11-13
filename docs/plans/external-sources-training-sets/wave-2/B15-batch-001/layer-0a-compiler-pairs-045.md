# Layer 0-A — Compiler Pairs (045)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-045-001 — ImportService: IMPLEMENT from "create"

A: "Create an ImportService to ingest bulk CSV data from external systems."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"create" → IMPLEMENT.

---

## B15-compiler-045-002 — ImportService: IMPLEMENT+SECURITY on "authenticate"

A: "Create ImportService and authenticate the import source connections."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
"create" → IMPLEMENT. "authenticate" → SECURITY.

---

## B15-compiler-045-003 — ImportService: IMPLEMENT+API_CONTRACT on "wire up"

A: "Create ImportService and wire up the source system API for data ingestion."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
"create" → IMPLEMENT. "wire up" → API_CONTRACT.

---

## B15-compiler-045-004 — ImportService: IMPLEMENT+TEST on "write tests"

A: "Create ImportService and write integration tests for the CSV parsing pipeline."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
"create" → IMPLEMENT. "write integration tests" → TEST.

---

## B15-compiler-045-005 — "ingest" and "parse" are IMPLEMENT trigger verbs

"ingest" and "parse" as code artifact creation verbs fire IMPLEMENT.
"Ingest the bulk data feed" → IMPLEMENT. "Parse the incoming CSV records" → IMPLEMENT.
Both are feature_addition TransitionType when no "refactor" present.
