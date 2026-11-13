# Layer 0-D — Flow Factory Pairs (006)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: flow-factory

## B18-0D-006-001 — IMPLEMENT+ARCHITECTURE factory: DataMigrationFlow44 + ArchitectureFlow31

**Scenario:** ProductCatalogMigration (IMPLEMENT + ARCHITECTURE)

**Factory calls (two independent calls for two predicates):**

**Call 1 — IMPLEMENT:**
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.4",
  "transition_type": "feature_addition",
  "connector_name": "ProductCatalogMigration",
  "factory_mode": "deterministic_generic"
}
→ flow_name: "DataMigrationFlow44", sub_phase: "4.4"
```

**Call 2 — ARCHITECTURE:**
```
{
  "predicate": "ARCHITECTURE",
  "sub_phase": "3.1",
  "transition_type": "feature_addition",
  "connector_name": "ProductCatalogMigration",
  "factory_mode": "deterministic_generic"
}
→ flow_name: "ArchitectureFlow31", sub_phase: "3.1"
```

**Combined factory output:**
```
{
  "flows": [
    {"flow_name": "DataMigrationFlow44", "sub_phase": "4.4"},
    {"flow_name": "ArchitectureFlow31", "sub_phase": "3.1"}
  ],
  "connector_name": "ProductCatalogMigration",
  "transition_type": "feature_addition",
  "factory_mode": "deterministic_generic"
}
```

---

## B18-0D-006-002 — ArchitectureFlow31 configuration

**ArchitectureFlow31 configuration:**
```
{
  "flow_name": "ArchitectureFlow31",
  "scenario_type": "schema_design",
  "sub_phase": "3.1",
  "operations": [
    "schema_analysis",       // Analyze existing and target schema
    "design_generation",     // Generate schema design artifacts
    "specification_output"   // Output schema specification
  ],
  "output_artifacts": [
    "schema_diagram",
    "entity_relationship_diagram",
    "migration_specification"
  ]
}
```

---

## B18-0D-006-003 — IMPLEMENT+ARCHITECTURE ordering at factory level

**Factory-level ordering constraint:**

When factory produces both DataMigrationFlow44 and ArchitectureFlow31:
- ArchitectureFlow31 generates schema specification first
- DataMigrationFlow44 uses schema specification as input
- Factory registers this dependency: DataMigrationFlow44 depends_on ArchitectureFlow31

**Factory dependency registration:**
```
{
  "dependency": {
    "flow": "DataMigrationFlow44",
    "depends_on": "ArchitectureFlow31",
    "reason": "migration needs schema specification as input"
  }
}
```

---

## B18-0D-006-004 — IMPLEMENT+ARCHITECTURE for refactoring scenarios

**Scenario:** OrderServiceRefactoring with pattern (IMPLEMENT + ARCHITECTURE)

**Factory calls:**

Call 1 — IMPLEMENT (refactoring):
```
{
  "predicate": "IMPLEMENT",
  "sub_phase": "4.1",
  "transition_type": "refactoring",
  "connector_name": "OrderServicePatternRefactor",
  "factory_mode": "deterministic_generic"
}
→ flow_name: "BackendServiceFlow41", sub_phase: "4.1"
```

Call 2 — ARCHITECTURE:
```
{
  "predicate": "ARCHITECTURE",
  "sub_phase": "3.1",
  "transition_type": "refactoring",
  "connector_name": "OrderServicePatternRefactor",
  "factory_mode": "deterministic_generic"
}
→ flow_name: "ArchitectureFlow31", sub_phase: "3.1"
```

**Difference from migration:** BackendServiceFlow41 (not DataMigrationFlow44) for IMPLEMENT.

---

## B18-0D-006-005 — Factory pattern: same factory, different flows based on TransitionType

**Same factory input structure — different flow output based on TransitionType:**

```
IMPLEMENT + 4.4 + feature_addition → DataMigrationFlow44
IMPLEMENT + 4.1 + refactoring → BackendServiceFlow41
```

**TransitionType is the decisive parameter in the IMPLEMENT factory selection:**
- feature_addition → DataMigrationFlow44
- refactoring → BackendServiceFlow41

**ARCHITECTURE factory selection is TransitionType-independent:**
- Any TransitionType + ARCHITECTURE + 3.1 → ArchitectureFlow31

---

## B18-0D-006-006 — Flow factory table: IMPLEMENT+ARCHITECTURE combinations

| Predicate combo | TransitionType | IMPLEMENT flow | ARCHITECTURE flow |
|---|---|---|---|
| [IMPLEMENT, ARCHITECTURE] | feature_addition, 4.4 | DataMigrationFlow44 | ArchitectureFlow31 |
| [IMPLEMENT, ARCHITECTURE] | refactoring, 4.1 | BackendServiceFlow41 | ArchitectureFlow31 |
| [IMPLEMENT, ARCHITECTURE] | feature_addition, 4.1 + 4.4 (both) | BackendServiceFlow41 + DataMigrationFlow44 | ArchitectureFlow31 |

**The ARCHITECTURE flow is always ArchitectureFlow31 (3.1).** It does not change based on TransitionType or IMPLEMENT sub-phase.
