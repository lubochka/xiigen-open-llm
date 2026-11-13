# Layer 2 — Nine-Node 4.4 Pairs (016)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: nine-node-4.4

## B18-N9-016-001 — ProductVariantsSeeding: N01 input

**Scenario:** Seed product_variants table with size, color, and material variants.

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- ConnectorName: ProductVariantsSeeding
- DomainId: "software-development"
- Flow: DataMigrationFlow44
- Sub-phase: 4.4
- SelectedLogicalTransition: IMPLEMENT alone → DataMigrationFlow44, feature_addition

---

## B18-N9-016-002 — N02-N04: ProductVariantsSeeding scope

**N02 — Scope:**
- Source: products table (12,000 active products) + variant_definitions (default size/color/material sets)
- Target: product_variants (product_id FK, variant_type, variant_value, sku_suffix, active)
- Expected: ~3.6 variants per product = ~43,200 variant rows
- Idempotency: ON CONFLICT (product_id, variant_type, variant_value) DO NOTHING

**N03 — Source Analysis:**
- products: 12,000 active products; 85% have size variants; 90% have color; 60% have material
- variant_definitions: 8 sizes (XS-3XL), 12 colors, 6 materials (defaults)

**N04 — Target Specification:**
- product_variants: (variant_id UUID, product_id FK, variant_type ENUM, variant_value VARCHAR, sku_suffix VARCHAR, active BOOLEAN)

---

## B18-N9-016-003 — N05-N07: ProductVariantsSeeding scripts

**N05 — Script Generation:**
1. ProductVariantsSeed.sql — cross-join products with variant_definitions; INSERT ON CONFLICT DO NOTHING
2. ProductVariantsSeed_verify.sql — count verification per variant_type
3. ProductVariantsSeed_down.sql — DELETE from product_variants where seeded_at = this_run

**N06 — Execution:** single SQL statement with cross-join; ~43,200 rows in one batch

**N07 — Pre-Gate:** count expected variants by type before insert

---

## B18-N9-016-004 — N08: ProductVariantsSeeding Gate B

**N08 — Gate B:**
- Type: idempotency_check
- Condition 1: count(product_variants) == ~43,200 ✓
- Condition 2: re-run count unchanged (ON CONFLICT DO NOTHING prevents duplicates) ✓
- Breakdown check: count per variant_type matches expected proportions ✓

**N08 verdict: PASS**

---

## B18-N9-016-005 — N09: ProductVariantsSeeding output

**N09 output:**
```
{
  "connector_name": "ProductVariantsSeeding",
  "flow": "DataMigrationFlow44",
  "transition_type": "feature_addition",
  "gate_b_type": "idempotency_check",
  "gate_b_condition": "count(product_variants) == 43,200 AND idempotent",
  "gate_b_status": "PASS",
  "files_generated": 3,
  "artifacts": [
    "ProductVariantsSeed.sql",
    "ProductVariantsSeed_verify.sql",
    "ProductVariantsSeed_down.sql"
  ],
  "n08_verdict": "PASS"
}
```

---

## B18-N9-016-006 — Key seeding pattern: ON CONFLICT DO NOTHING vs migration_log

**Two idempotency approaches in B18 seeding scenarios:**

| Approach | Used when | Mechanism |
|---|---|---|
| migration_log (ID tracking) | Row-by-row processing; need audit trail | INSERT into migration_log; skip if already logged |
| ON CONFLICT DO NOTHING | Set-based insert; simple idempotency | SQL constraint on unique key; no extra log table |

ProductVariantsSeeding uses ON CONFLICT because variants have a natural unique key (product_id + variant_type + variant_value).
