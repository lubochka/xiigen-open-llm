# Held-Out and Corruption Pairs (009)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: held-out-corruption

## B18-HO-009-001 — HO: "write migration" → IMPLEMENT, feature_addition, 4.4

**Request:** "Write the order history migration to move 2.5 million records from the legacy flat table to the normalized orders/items/shipments schema."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Flow: DataMigrationFlow44

**Reasoning:**
- "write migration" → IMPLEMENT (Rule 2), feature_addition (Rule 2), 4.4
- "order history records" → 4.4 data vocabulary
- "legacy flat table" → 4.4 (database vocabulary)
- No "structure" (schema alone, not "schema structure") → ARCHITECTURE does NOT fire
- TransitionType: feature_addition (Rule 2 — migration is the artifact)

---

## B18-HO-009-002 — HO: "migrate data" → also IMPLEMENT, feature_addition, 4.4

**Request:** "Migrate all product data from the legacy flat files to the new normalized product catalog database."

**Expected classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Flow: DataMigrationFlow44

**Both "write migration" and "migrate data" route to the same result.** The phrasing differs but the outcome is identical.

---

## B18-HO-009-003 — COR: "migrate" → refactoring (wrong TransitionType)

**Request:** "Migrate all product data from the legacy flat files to the new normalized product catalog database."

**Corrupted classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: **refactoring** ← WRONG
- Sub-phase: 4.1 ← WRONG
- Flow: BackendServiceFlow41 ← WRONG

**Corruption type:** v6-02 type (wrong TransitionType) + v6-03 type (wrong routing)
**Corrupted reasoning:** "migrate is a verb that implies moving/reorganizing code; this is a refactoring action"

**Why wrong:**
1. "migrate" in DATA context (product data, flat files, database) → feature_addition, 4.4
2. "migrate" is NOT in the refactoring vocabulary list
3. Refactoring vocabulary: refactor, extract, rename, consolidate, restructure, reorganize, move (code), clean up
4. "migrate data" is not "move code" — different action, different routing

---

## B18-HO-009-004 — HO: "write migration" vs "migrate data" side-by-side comparison

**Both produce the same result:**

Request A: "Write the ProductCatalogMigration to migrate all product records to the new schema."
Request B: "Migrate all product records to the new schema."

**Expected classification for both:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4
- Flow: DataMigrationFlow44

**"write migration" (Rule 2) and "migrate data" (Rule 5) both produce:** IMPLEMENT, feature_addition, 4.4

---

## B18-HO-009-005 — COR: "migrate" treated as code migration (wrong)

**Request:** "Migrate the order data to the new normalized schema."

**Corrupted classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition (correct)
- Sub-phase: **4.1** ← WRONG (treating data migration as code migration)
- Flow: BackendServiceFlow41 ← WRONG

**Corruption type:** v6-03 — data migration vocabulary ignored; default to 4.1
**Corrupted reasoning:** "migrate to a new schema is like migrating the data model (a code concern) to a new version — this is a code migration task (4.1)"

**Why wrong:**
- "order data" → data vocabulary → 4.4
- "schema" (as data migration target) → 4.4 context
- "migrate data/records" → 4.4 (Rule 5)
- This is NOT a code migration; it's a data migration

---

## B18-HO-009-006 — Summary: "migrate" classification rules

| Request | TransitionType | Sub-phase | Notes |
|---|---|---|---|
| "write the migration" | feature_addition | 4.4 | Rule 2 |
| "migrate data/records" | feature_addition | 4.4 | Rule 5 |
| "migrate application from .NET 4.8 to 9" | feature_addition | 4.1 | Code migration |
| "migrate stored procedures to services" | feature_addition | 4.1 | Code migration |
| "refactor the migration service" | refactoring | 4.1 | Refactoring service code |
| "migrate + refactor" | refactoring | 4.1 + 4.4 (both) | Both sub-phases |
