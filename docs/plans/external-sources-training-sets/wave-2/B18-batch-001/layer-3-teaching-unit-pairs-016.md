# Layer 3 — Teaching Unit Pairs (016)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: teaching-unit

## B18-TU-016-001 — Teaching: "consolidate" on tables → refactoring (even for data)

**Rule:** "consolidate" is always a refactoring verb (Rule 20). It fires refactoring TransitionType regardless of whether the target is code classes or database tables.

**"consolidate" + code vocabulary → refactoring + 4.1**
**"consolidate" + database vocabulary → refactoring + 4.4**

**Anti-example (v6-02):** "Consolidate product variant tables" → feature_addition (wrong: new unified table is created). Correct: "consolidate" dominant verb → refactoring.

---

## B18-TU-016-002 — Teaching: "populate" = seed (4.4 vocabulary)

**Rule:** "populate" is semantically equivalent to "seed" in B18. Both fire IMPLEMENT + 4.4 + feature_addition.

**Test:**
- "Populate product_variants with variant data" → sub-phase 4.4, feature_addition ✓
- "Seed product_variants with defaults" → sub-phase 4.4, feature_addition ✓
- Both produce the same predicate output.

---

## B18-TU-016-003 — Teaching: "data structure" fires ARCHITECTURE (critical case)

**Rule:** "data structure" contains the noun "structure" → ARCHITECTURE fires absolutely (Rule 3).

**This is one of the most frequently failed cases.** Students expect "data structure" to be data vocabulary, not an ARCHITECTURE trigger.

**Test:**
- "Document the variant data structure" → ARCHITECTURE fires? **YES.** "structure" is the trigger.
- "Define the data model for variants" → ARCHITECTURE fires? **No.** "model" is not a trigger; "data" is not a trigger.
- "Describe the variant data" → ARCHITECTURE fires? **No.** No trigger word.

---

## B18-TU-016-004 — Teaching: predicates are additive; three predicates from one request

**Rule:** Each predicate fires independently. No predicate suppresses another.

**Example:** "Seed product variants and document the variant data structure."
- "seed" → IMPLEMENT
- "document" → DOCUMENT
- "data structure" → "structure" → ARCHITECTURE

**Three predicates fire from one request. All are independent. Result: ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"].**

---

## B18-TU-016-005 — Teaching: seeding Gate B uses idempotency_check

**Seeding idempotency_check Gate B definition:**
1. count(seeded_rows) == expected_count (correct number inserted)
2. Re-run produces no additional rows (idempotent: ON CONFLICT DO NOTHING or migration_log)

**ProductVariantsSeeding Gate B:** count(product_variants) == product_count × avg_variants AND idempotent.

**Why not row_count_assertion?** Seeding derives data from source; source is unchanged. "migrated == source" does not apply when source is not depleted.
