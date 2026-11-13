# Layer 0-B — Router Pairs (008)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: router

## B18-0B-008-001 — [IMPLEMENT, DOCUMENT] → 4.4 + 2.1

**Scenario:** OrderArchiveMigration with documentation

**Router input:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: feature_addition
- Vocabulary: "write migration", "database" → 4.4; "document" → 2.1
- Request: "Write the database migration for the payment table and document the before/after schema changes."

**Routing logic:**
1. IMPLEMENT (feature_addition, 4.4) → DataMigrationFlow44
2. DOCUMENT → DocumentationFlow21, sub-phase 2.1

**Router output:**
- Flows: [DataMigrationFlow44, DocumentationFlow21]
- Sub-phases: [4.4, 2.1]
- Execution: parallel (documentation does not depend on IMPLEMENT completion for schema documentation)

---

## B18-0B-008-002 — DocumentationFlow21 in B18

**DOCUMENT routing in B18:**
- Always routes to DocumentationFlow21
- Sub-phase: 2.1 (documentation/specification)
- Can run parallel to IMPLEMENT in most B18 cases
- Exception: post-implementation documentation (e.g., "document the as-built schema") → sequential after IMPLEMENT

**DocumentationFlow21 input:** transformation rules, schema changes, method signatures, API changes, policy descriptions

---

## B18-0B-008-003 — parallel vs sequential DOCUMENT routing

**Parallel (DOCUMENT can run alongside IMPLEMENT):**
- "Write migration and document the schema changes" (schema can be documented from design spec)
- "Refactor service and document the new interface" (interface spec exists before implementation)

**Sequential (DOCUMENT must wait on IMPLEMENT):**
- "Write migration then document the actual row counts" (counts only known after run)
- "Refactor and document the final method signatures" (final signatures only known after refactoring)

**B18 default:** parallel unless "then document" or "after migration, document" signals dependency.

---

## B18-0B-008-004 — IMPLEMENT (refactoring) + DOCUMENT routing

**Request:** "Refactor the PaymentService to consolidate validation and document the new method signatures."

**Router input:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: refactoring
- Vocabulary: "refactor", "consolidate", "service", "methods" → 4.1; "document" → 2.1

**Routing logic:**
- IMPLEMENT (refactoring, 4.1) → BackendServiceFlow41
- DOCUMENT → DocumentationFlow21, 2.1

**Router output:**
- Flows: [BackendServiceFlow41, DocumentationFlow21]
- Sub-phases: [4.1, 2.1]

---

## B18-0B-008-005 — OrderArchiveMigration: full router trace

**ConnectorName:** OrderArchiveMigration

**Input:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: feature_addition
- Sub-phases: 4.4, 2.1

**Router output:**
```
{
  "primary_flow": "DataMigrationFlow44",
  "secondary_flows": ["DocumentationFlow21"],
  "sub_phases": ["4.4", "2.1"],
  "connector_name": "OrderArchiveMigration",
  "transition_type": "feature_addition",
  "execution": "parallel"
}
```

---

## B18-0B-008-006 — DocumentationFlow21 sub-phase 2.1 vs other doc sub-phases

**2.1 routing in B18:**
- "document the migration" → 2.1 (migration documentation)
- "document the schema changes" → 2.1 (schema documentation)
- "document the transformation rules" → 2.1 (rule documentation)
- "document the method signatures" → 2.1 (interface documentation)
- "write spec for the data model" → 2.1 (specification)
- "annotate the migration steps" → 2.1 (inline documentation)

**B18 always routes DOCUMENT to 2.1.** There is no alternative documentation sub-phase relevant to B18 scenarios.
