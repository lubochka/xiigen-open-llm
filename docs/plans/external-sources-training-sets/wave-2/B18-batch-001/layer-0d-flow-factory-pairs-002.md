# Layer 0-D — Flow Factory Pairs (002)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: flow-factory

## B18-0D-002-001 — deterministic_generic: 4.4 seeding scenario

**Scenario:** ProductInventorySeeding

**Factory input:**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "connector_name": "ProductInventorySeeding",
  "factory_mode": "deterministic_generic"
}
```

**Factory logic:**
- IMPLEMENT + feature_addition + 4.4 → select DataMigrationFlow44
- Factory mode: deterministic_generic (no probation; direct selection by rule)
- Gate B: N/A for deterministic_generic (no testing phase)

**Factory output:**
```
{
  "flow_name": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "factory_mode": "deterministic_generic",
  "gate_b_status": "PENDING",
  "connector_name": "ProductInventorySeeding"
}
```

---

## B18-0D-002-002 — deterministic_generic: why 4.4 seeding uses this mode

**ProductInventorySeeding is deterministic_generic because:**
1. The vocabulary is unambiguous: "seed" + "product inventory" + "table" → clear 4.4 signals
2. The TransitionType is unambiguous: feature_addition (new seed data)
3. The flow selection is unambiguous: DataMigrationFlow44 is the only 4.4 IMPLEMENT flow
4. No competing signals require probabilistic resolution
5. Factory applies deterministic rule: seed + 4.4 → DataMigrationFlow44

---

## B18-0D-002-003 — deterministic_generic: CategoryFixtureSeeding

**Scenario:** CategoryFixtureSeeding

**Factory input:**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "connector_name": "CategoryFixtureSeeding",
  "factory_mode": "deterministic_generic"
}
```

**Factory output:**
```
{
  "flow_name": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "factory_mode": "deterministic_generic",
  "gate_b_status": "PENDING",
  "connector_name": "CategoryFixtureSeeding"
}
```

---

## B18-0D-002-004 — deterministic_generic: ETLPipelineBuild

**Scenario:** LegacyOrderEtlPipeline

**Factory input:**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "connector_name": "LegacyOrderEtlPipeline",
  "factory_mode": "deterministic_generic"
}
```

**Factory output:**
```
{
  "flow_name": "DataMigrationFlow44",
  "sub_phase": "4.4",
  "factory_mode": "deterministic_generic",
  "gate_b_status": "PENDING",
  "connector_name": "LegacyOrderEtlPipeline"
}
```

---

## B18-0D-002-005 — deterministic_generic: multiple flows for IMPLEMENT+ARCHITECTURE

**Scenario:** ProductCatalogMigration (IMPLEMENT + ARCHITECTURE)

**Factory calls (two separate calls for two predicates):**

Call 1:
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "connector_name": "ProductCatalogMigration",
  "factory_mode": "deterministic_generic"
}
→ flow_name: "DataMigrationFlow44"
```

Call 2:
```
{
  "predicate": "ARCHITECTURE",
  "sub_phase": "3.1",
  "transition_type": "feature_addition",
  "connector_name": "ProductCatalogMigration",
  "factory_mode": "deterministic_generic"
}
→ flow_name: "ArchitectureFlow31"
```

---

## B18-0D-002-006 — deterministic_generic factory rules summary

**Deterministic factory rules for B18:**

| Predicate | Sub-phase | TransitionType | Flow selected |
|---|---|---|---|
| IMPLEMENT | 4.4 | feature_addition | DataMigrationFlow44 |
| IMPLEMENT | 4.1 | refactoring | BackendServiceFlow41 |
| ARCHITECTURE | 3.1 | any | ArchitectureFlow31 |
| TEST | 5.x | any | UnitTestingFlow51 |
| DOCUMENT | 2.1 | any | DocumentationFlow21 |
| SECURITY | security | any | SecurityReviewFlow |

**All B18 standard scenarios use deterministic_generic for initial factory invocation.**
