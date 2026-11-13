# Layer 0-A — Compiler Pairs (042)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-042-001 — ARCHITECTURE+IMPLEMENT combo: design-then-build pattern

**Request:** "Design the index architecture for the inventory tracking system and write the migration to create all required indexes."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "index architecture" → "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "write the migration" → IMPLEMENT fires (Rule 2)
- "inventory tracking system" → 4.4 database vocabulary
- No security tokens; no refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-042-002 — ARCHITECTURE+IMPLEMENT combo: schema design before migration

**Request:** "Design the data migration pattern for moving analytics events to the time-series partition structure, then implement the migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "migration pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "partition structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute; reinforcement)
- "implement the migration" → IMPLEMENT fires (Rule 2)
- "analytics events" → 4.4 vocabulary
- No security or TEST tokens

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-042-003 — ARCHITECTURE+IMPLEMENT combo with SECURITY

**Request:** "Design the PII-safe schema structure for migrating customer records, then implement the anonymization migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "PII-safe schema structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "PII" → SECURITY fires (Rule 14, absolute)
- "implement the anonymization migration" → IMPLEMENT fires (Rule 2)
- "customer records" → 4.4 vocabulary

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1 + security
- DomainId: software-development

---

## B18-0A-042-004 — ARCHITECTURE+IMPLEMENT combo with TEST

**Request:** "Design the composite index pattern for the reporting tables and create the indexes; write tests to verify query performance improvements."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "composite index pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "create the indexes" → IMPLEMENT fires (Rule 2: new artifacts)
- "write tests" → TEST fires (Rule 12: verb form)
- No SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1 + 5.x
- DomainId: software-development

---

## B18-0A-042-005 — DataAnonymization trigger map: complete 037-042 reference

**DataAnonymization / PIIMasking / DataArchival / ShardingMigration trigger vocabulary (037-042):**

| Domain | IMPLEMENT trigger | SECURITY trigger | ARCHITECTURE trigger |
|---|---|---|---|
| DataAnonymization | "write/add migration", "anonymize" (verb) | "PII", "GDPR", "sensitive", "anonymize" | "anonymization pattern", "anonymization architecture" |
| PIIMasking | "write migration", "mask" (verb) | "mask", "PII", "credit card", "SSN" | "masking structure", "masking pattern" |
| DataArchival | "write migration", "archive" (verb) | "PII", "encrypted", "GDPR" | "archival structure", "archival pattern" |
| ShardingMigration | "write migration", "migrate", "restructure" | (uncommon) | "sharded architecture", "shard pattern" |
| ReplicationSetup | "write migration", "configure" | "encrypted replication", "TLS" | "replication architecture" |
| ReadReplicaPromotion | "write migration", "promote" | (uncommon) | "replica architecture" |
| FullTextSearchIndex | "create/add index" | (uncommon) | "search index structure" |
| GeoIndex | "create/add index" | (uncommon) | "geospatial index pattern" |
| CompositeIndex | "create/add index" | (uncommon) | "composite index pattern" |
| CoveringIndex | "create/add index" | (uncommon) | "covering index structure" |
| PartialIndex | "create/add index" | (uncommon) | "partial index structure" |
