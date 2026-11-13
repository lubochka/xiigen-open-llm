# Layer 0-C — Knowledge Edge Pairs (018)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E018-001 — Database migration execution vocabulary is gap

DB migration gap terms: "run migration script", "apply schema change", "execute migration", "migrate database", "restart after migration"
  Teaching: Migration execution commands → gap. "run migration" → gap. Not IMPLEMENT, not TEST.

---

## E018-002 — "validate" fires TEST in database migration context

A: "Validate row counts after migration" → TEST fires on "validate"
  Teaching: "validate" is a TEST predicate trigger. It fires TEST regardless of the object being validated.

---

## E018-003 — "build the migration runner" fires IMPLEMENT

A: "Build the database migration runner" → IMPLEMENT (verb "build" + "runner" as code artifact)
  Teaching: "runner", "service", "controller", "handler" → code artifact nouns. "build" + code artifact → IMPLEMENT.

---

## E018-004 — "migration release criteria" fires DOCUMENT via "criteria"

A: "Document the database migration release criteria" → DOCUMENT fires on "criteria"
  Teaching: "criteria" fires DOCUMENT in migration context. "migration" and "database" are modifiers.

---

## E018-005 — QA fires for migration gate

A: "QA gate the migration" → QA | "QA sign-off on the schema change" → QA | "QA review before cutover" → QA
  Teaching: "QA" fires in database migration context. "cutover" remains gap.
