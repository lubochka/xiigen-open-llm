# Held-Out and Corruption Pairs (018)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H018-001 — Held-out: QA+IMPLEMENT — migration runner

A: "Build the database migration runner and QA the schema change before the release cutover."
  Expected: ["QA", "IMPLEMENT"]
  Held-out: "build" → IMPLEMENT; "runner" → code artifact (IMPLEMENT dedup); "QA" → QA; "cutover" → gap. feature_addition.

---

## H018-002 — Held-out: TEST alone — "validate" fires TEST in migration context

A: "Run the database migration script and validate the row counts."
  Expected: ["TEST"]
  Held-out: "run migration" → gap; "validate" → TEST fires on "validate." One predicate. documentation.

---

## H018-003 — Held-out: gap — migration execution

A: "Execute the database migration against the production schema."
  Expected: []
  Held-out: "execute migration" → gap. knowledge_edge.

---

## C018-001 — Corruption: "run migration" fires IMPLEMENT

A: "Run the migration script against the production database."
  Correct: [] — gap (migration execution is gap)
  Corrupt: ["IMPLEMENT"] ← wrong ("run" treated as implementation)
  Teaching: "run migration" is gap. "build the migration runner" fires IMPLEMENT; "run migration" does not.

---

## C018-002 — Corruption: "validate" not recognized as TEST trigger

A: "Run the migration and validate the row counts."
  Correct: ["TEST"] — "validate" fires TEST
  Corrupt: [] ← wrong ("validate" suppressed as just "checking")
  Teaching: "validate" always fires TEST regardless of context.
