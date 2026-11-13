# Layer 0-D — Flow Factory Pairs (016)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-016-001 — ProductVariants: factory for seeding

**Factory input:**
```
FlowFactoryInput {
  predicate: "IMPLEMENT",
  sub_phase: "4.4",
  transition_type: "feature_addition",
  connector_name: "ProductVariantsSeeding",
  factory_mode: "probationary_accepted"
}
```

**Gate B type:** idempotency_check (seeding operation)
**Condition:** seeded variant count == expected_variant_count; no duplicates on re-seed

**Factory output:**
```
FlowFactoryOutput {
  flow_name: "DataMigrationFlow44",
  gate_b_type: "idempotency_check",
  gate_b_condition: "seeded_count == product_count * avg_variants_per_product AND idempotent"
}
```

---

## B18-0D-016-002 — ProductVariants: refactoring on data tables → factory preserves DataMigrationFlow44

**Factory input:**
- transition_type: "refactoring"
- sub_phase: "4.4"
- connector_name: "ProductVariantsTableConsolidation"

**Factory key decision:**
- TransitionType is refactoring, but sub_phase is 4.4 (database tables)
- Factory selects DataMigrationFlow44 (not BackendServiceFlow41)
- BackendServiceFlow41 requires sub_phase: 4.1; refactoring on tables → DataMigrationFlow44

**Gate B type:** row_count_assertion (consolidation: records must not be lost)
- Condition: count(unified_product_variants) == sum(count(sizes), count(colors), count(materials))

---

## B18-0D-016-003 — ProductVariants + ARCHITECTURE + DOCUMENT: three-flow factory

**Factory input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]

**Factory output:**
```
FlowFactoryEntry {
  Flows: ["ArchitectureFlow31", "DataMigrationFlow44", "DocumentationFlow21"],
  GateBSet: [
    {flow: "ArchitectureFlow31", condition: "variant_schema_approved"},
    {flow: "DataMigrationFlow44", condition: "seeded_count == expected AND idempotent"},
    {flow: "DocumentationFlow21", condition: "variant_data_structure_doc_complete"}
  ],
  parallel_eligible: ["DocumentationFlow21"]
}
```

---

## B18-0D-016-004 — ProductVariants: factory mode for fixture vs seed

**Fixture and seed use the same factory path:**
- fixture → DataMigrationFlow44 (same as seed)
- Both use idempotency_check Gate B
- factory_mode: probationary_accepted for new scenarios

**Factory does not distinguish between "fixture" and "seed" in output selection. Both use DataMigrationFlow44.**

---

## B18-0D-016-005 — ProductVariants: Gate B idempotency condition definition

**Idempotency_check Gate B definition:**
1. First run: seeded_count == expected_count (insertion succeeded)
2. Second run: seeded_count unchanged (no duplicates added)
3. On re-run: existing seed data is not replaced or duplicated; new rows not created

**Gate B PASS:** both conditions hold.
**Gate B FAIL:** either insertion failed or re-run creates duplicates.
