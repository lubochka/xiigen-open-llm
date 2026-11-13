# Layer 0-D — Flow Factory Pairs (032)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: flow-factory

## B18-0D-032-001 — FullTextSearchIndex factory entry

**Scenario:** "Create a GIN full-text search index on the products description column."

```
FlowFactoryEntry {
  ScenarioId: "FullTextSearchIndexCreation",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "gin_index_exists(products_description_fts_idx) AND query_plan_uses_fts == true"}
  ]
}
```

---

## B18-0D-032-002 — GeoIndex factory entry

**Scenario:** "Add a GiST spatial index on the locations table geometry column."

```
FlowFactoryEntry {
  ScenarioId: "GeoIndexCreation",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "gist_index_exists(locations_geometry_idx) AND geo_queries_use_index == true"}
  ]
}
```

---

## B18-0D-032-003 — CompositeIndex factory entry

**Scenario:** "Create a composite index on (tenant_id, created_at, status) for the orders table."

```
FlowFactoryEntry {
  ScenarioId: "CompositeIndexCreation",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "index_exists(orders_tenant_created_status_idx) AND query_plan_uses_index == true"}
  ]
}
```

---

## B18-0D-032-004 — CoveringIndex factory entry

**Scenario:** "Add a covering index on orders(customer_id) INCLUDE (total_amount, created_at)."

```
FlowFactoryEntry {
  ScenarioId: "CoveringIndexCreation",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "covering_index_exists(orders_customer_id_covering_idx) AND heap_fetches == 0"}
  ]
}
```

---

## B18-0D-032-005 — PartialIndex factory entry

**Scenario:** "Create a partial index on orders WHERE status = 'active'."

```
FlowFactoryEntry {
  ScenarioId: "PartialIndexCreation",
  Flows: ["DataMigrationFlow44"],
  TransitionType: "feature_addition",
  FactoryMode: "probationary_accepted",
  GateBSet: [
    {flow: "DataMigrationFlow44", type: "schema_change_verification",
     condition: "partial_index_exists(orders_active_status_idx) AND filtered_query_uses_index == true"}
  ]
}
```
