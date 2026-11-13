# Layer 2 — Nine-Node 4.4 Pairs (003)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: nine-node-4.4

## B18-N9-003-001 — ProductCatalogMigration: N01 input

**Scenario:** Migrate product catalog from CSV flat file to normalized product/category/variant schema.

**N01 input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- ConnectorName: ProductCatalogMigration
- DomainId: "software-development"
- Flows: DataMigrationFlow44 (4.4) + ArchitectureFlow31 (3.1)
- Sub-phases: 4.4 + 3.1

**Note:** ARCHITECTURE fires because "schema structure" was in the request. ArchitectureFlow31 runs first.

---

## B18-N9-003-002 — ArchitectureFlow31 phase (3.1 output used by DataMigrationFlow44)

**ArchitectureFlow31 (3.1) output — schema specification:**

```
Target schema design:
  products table: (product_id PK, name, description, base_price, sku)
  categories table: (category_id PK, name, parent_category_id FK nullable)
  product_categories: (product_id FK, category_id FK) — many-to-many join
  product_variants: (variant_id PK, product_id FK, variant_name, sku_suffix, price_delta)

Legacy CSV columns (flat):
  sku, product_name, description, price, category_1, category_2, variant_1_name, 
  variant_1_price, variant_2_name, variant_2_price
  
Field mapping:
  sku → products.sku
  product_name → products.name
  price → products.base_price
  category_1 → categories.name (top-level)
  category_2 → categories.name (sub-category, parent = category_1)
  variant_* → product_variants rows
```

---

## B18-N9-003-003 — N02-N05: ProductCatalogMigration scope and generation

**N02 — Scope:**
- Source: products.csv (15,000 rows)
- Target: products, categories, product_categories, product_variants
- Batch size: 500 rows (CSV-based; memory-efficient)
- Idempotency: migration_log by product SKU

**N03 — Source Analysis:**
- 15,000 product rows
- 250 unique category_1 values; 1,200 unique category_2 values
- Average 1.8 variants per product = ~27,000 variant rows expected

**N04 — Target Specification:**
- products: 15,000 rows
- categories: ~1,450 rows (250 top + 1200 sub)
- product_categories: ~15,000 rows (avg 1 category per product)
- product_variants: ~27,000 rows

**N05 — Script Generation:**
- ProductCatalogMigration_categories.sql (categories first, for FK reference)
- ProductCatalogMigration_products.sql
- ProductCatalogMigration_variants.sql
- ProductCatalogMigration_down.sql
- ProductCatalogMigration_idempotency.sql

---

## B18-N9-003-004 — N06-N07: execution and pre-verification

**N06 — Execution order:**
1. Insert categories (top-level first, subcategories second)
2. Insert products
3. Insert product_categories join records
4. Insert product_variants
5. Log all inserts in migration_log

**N07 — Pre-Gate Verification:**
- count(categories) check (expect ~1,450)
- count(products) == 15,000
- count(product_variants) ~= 27,000
- All category FKs valid
- Spot-check 50 random SKUs for correct category and variant mapping

---

## B18-N9-003-005 — N08: Gate B and N09: output

**N08 Gate B:**
- count(products) == 15,000 ✓
- count(categories) == expected (verified count)
- All product_category FK references valid ✓
- All product_variant FK references valid ✓
- migration_log error_count == 0 ✓

**N08 verdict: PASS**

**N09 output:**
```
{
  "connector_name": "ProductCatalogMigration",
  "flows_completed": ["ArchitectureFlow31", "DataMigrationFlow44"],
  "gate_b_status": "PASS",
  "transition_type": "feature_addition",
  "files_generated": 6,
  "artifacts": [
    "schema_design.json",
    "ProductCatalogMigration_categories.sql",
    "ProductCatalogMigration_products.sql",
    "ProductCatalogMigration_variants.sql",
    "ProductCatalogMigration_down.sql",
    "ProductCatalogMigration_idempotency.sql"
  ],
  "n08_verdict": "PASS",
  "completion_message": "ProductCatalogMigration complete; 15K products + 1.45K categories + 27K variants"
}
```

---

## B18-N9-003-006 — ProductCatalogMigration: ARCHITECTURE contribution

**What ArchitectureFlow31 contributed:**
1. **Schema design:** defined the four target tables with proper FK relationships
2. **Field mapping:** specified how each CSV column maps to target table column
3. **Ordering:** determined that categories must be inserted before products (FK dependency)
4. **Many-to-many:** designed product_categories join table (not obvious from flat CSV)
5. **Variant modeling:** designed product_variants as separate table with price_delta

**Without ArchitectureFlow31** (v6-01 corruption scenario):
- DataMigrationFlow44 might use wrong schema
- Many-to-many relationship might be missed
- Category hierarchy might be flattened incorrectly
- FK ordering in migration might be wrong → constraint violations
