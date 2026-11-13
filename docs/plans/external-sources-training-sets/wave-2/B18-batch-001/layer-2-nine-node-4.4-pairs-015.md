# Layer 2 — Nine-Node 4.4 Pairs (015)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: nine-node-4.4

## B18-N9-015-001 — SearchIndexRebuild: N01 input

**Scenario:** Rebuild product search index by seeding all active products into search_index table.

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- ConnectorName: SearchIndexRebuild
- DomainId: "software-development"
- Flow: DataMigrationFlow44
- Sub-phase: 4.4
- SelectedLogicalTransition: IMPLEMENT alone → DataMigrationFlow44, feature_addition

---

## B18-N9-015-002 — N02-N04: SearchIndexRebuild scope

**N02 — Scope:**
- Source: products table (95,000 active products)
- Target: search_index (product_id, title, description_tokens, category, price, in_stock, indexed_at)
- Operation: seed (insert all active products; idempotent re-run)
- Batch size: 1,000 products

**N03 — Source Analysis:**
- products table: 95,000 rows (status = 'active')
- fields to index: title, description (tokenized), category_path, price_range, stock_status

**N04 — Target Specification:**
- search_index: (search_id UUID, product_id FK, title_tokens TEXT[], description_tokens TEXT[], category VARCHAR, price_cents INT, in_stock BOOLEAN, indexed_at TIMESTAMP)
- Token generation: title split on whitespace + punctuation; description stemmed

---

## B18-N9-015-003 — N05-N07: SearchIndexRebuild seed script generation

**N05 — Script Generation:**
1. SearchIndexSeed.cs — tokenize product titles and descriptions; insert to search_index
2. SearchIndexSeed_idempotency.sql — ON CONFLICT DO UPDATE (upsert; handles re-seeding)
3. SearchIndexSeed_verify.sql — count verification

**N06 — Execution:** 95 batches of 1,000; upsert prevents duplicates on re-run

**N07 — Pre-Gate:** verify count(products where status='active') == 95,000; pre-check token generation

---

## B18-N9-015-004 — N08: Gate B idempotency check

**N08 — Gate B:**
- Type: idempotency_check
- Condition 1: count(search_index) == 95,000 (all active products indexed) ✓
- Condition 2: re-run produces no new rows (upsert updates; no duplicates) ✓

**N08 verdict: PASS**

---

## B18-N9-015-005 — N09: SearchIndexRebuild output

**N09 output:**
```
{
  "connector_name": "SearchIndexRebuild",
  "flow": "DataMigrationFlow44",
  "transition_type": "feature_addition",
  "gate_b_type": "idempotency_check",
  "gate_b_condition": "count(search_index) == 95,000 AND idempotent",
  "gate_b_status": "PASS",
  "files_generated": 3,
  "artifacts": [
    "SearchIndexSeed.cs",
    "SearchIndexSeed_idempotency.sql",
    "SearchIndexSeed_verify.sql"
  ],
  "n08_verdict": "PASS",
  "completion_message": "SearchIndexRebuild complete; 95,000 products indexed"
}
```

---

## B18-N9-015-006 — Seeding Gate B vs Migration Gate B: the key distinction

**Migration Gate B (row_count_assertion):**
- Source has N rows → target must have N rows after migration
- One-time operation (data moved from source)

**Seeding Gate B (idempotency_check):**
- Source table is NOT depleted (products remain in products table)
- search_index is populated from products (seeding, not moving)
- Re-run must not duplicate rows (upsert or idempotency guard)
- Gate B: count correct AND re-run stable
