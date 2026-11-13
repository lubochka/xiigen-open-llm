# Layer 0-A — Compiler Pairs (003)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: compiler

## B18-0A-003-001 — "migrate data" + "structure" → IMPLEMENT+ARCHITECTURE; feature_addition

**Request:** "Migrate all product records to the new normalized table structure."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT (data migration vocabulary)
- Token: "product records" → 4.4 sub-phase signal (data vocabulary)
- Token: "table" → 4.4 sub-phase signal (database vocabulary)
- Token: "structure" → ARCHITECTURE fires (literal compiler rule, Rule 3 — always fires on "structure")
- TransitionType: feature_addition (data migration creates new artifact — migration script + populated tables)
- No "refactor" verb present → refactoring TransitionType does NOT apply

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

**Critical rule:** "structure" always fires ARCHITECTURE regardless of context. "Table structure", "data structure", "normalized structure" — all fire ARCHITECTURE. The compiler is literal.

---

## B18-0A-003-002 — "normalized structure" — ARCHITECTURE fires on "structure"

**Request:** "Move the legacy payment records into the new normalized structure for PCI (Payment Card Industry) compliance."

**Compiler trace:**
- Token: "move" → IMPLEMENT (refactoring vocabulary, but data context overrides)
- Token: "legacy payment records" → 4.4 sub-phase signal (data vocabulary)
- Token: "normalized" → 4.4 sub-phase signal (database normalization vocabulary)
- Token: "structure" → ARCHITECTURE fires (Rule 3, literal — "normalized structure" still fires)
- TransitionType: feature_addition (moving data records = new artifact in target schema)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-003-003 — "data structure" — ARCHITECTURE fires on "structure"

**Request:** "Implement the new data structure for the product catalog migration."

**Compiler trace:**
- Token: "implement" → IMPLEMENT
- Token: "data" → 4.4 sub-phase signal
- Token: "structure" → ARCHITECTURE fires (Rule 3 — "data structure" contains "structure")
- Token: "migration" → 4.4 sub-phase signal
- TransitionType: feature_addition (migration creates new artifact)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

**Teaching:** The compound noun "data structure" does not suppress the "structure" trigger. The compiler parses "structure" as an independent token.

---

## B18-0A-003-004 — "schema" alone without structural noun → NOT ARCHITECTURE

**Request:** "Migrate all customer accounts to match the new schema."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT (data migration)
- Token: "customer accounts" → 4.4 sub-phase signal
- Token: "schema" → borderline; no structural noun paired ("design", "structure", "architecture") → ARCHITECTURE does NOT fire (Rule 16)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Contrast with 003-001:** "table structure" fires ARCHITECTURE because "structure" is present. "Schema" alone does not.

---

## B18-0A-003-005 — "schema" + "structure" → ARCHITECTURE fires

**Request:** "Migrate all customer accounts to match the new schema structure."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT
- Token: "schema" → borderline; but paired with "structure" → structural noun present
- Token: "structure" → ARCHITECTURE fires (Rule 3)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-003-006 — ProductCatalogMigration: full compiler trace

**Request:** "Write the ProductCatalogMigration to move 15,000 product records from the legacy CSV flat file into the normalized product/category/variant schema structure."

**Compiler trace:**
- Token: "write" + "migration" → IMPLEMENT (Rule 2 — "write migration" = feature_addition)
- Token: "move" → IMPLEMENT (also present, but "write migration" dominates)
- Token: "product records" → 4.4 sub-phase signal
- Token: "legacy" → 4.4 sub-phase signal
- Token: "CSV" → 4.4 sub-phase signal (data file vocabulary)
- Token: "schema" → borderline alone
- Token: "structure" → ARCHITECTURE fires (Rule 3)
- TransitionType: feature_addition ("write the migration" = Rule 2, new artifact)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1
ConnectorName: ProductCatalogMigration
