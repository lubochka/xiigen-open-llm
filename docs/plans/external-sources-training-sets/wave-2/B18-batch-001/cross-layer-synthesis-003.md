# Cross-Layer Synthesis (003)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: cross-layer-synthesis

## B18-CLS-003 — Full Seven-Layer Trace: MultiStepMigration (IMPLEMENT+ARCHITECTURE+TEST)

**Request:** "Write the ProductCatalogMigration to move 3 million product records to the normalized catalog schema, design the target schema structure, and write integration tests to verify all product records transferred correctly."

---

### Layer 0-A: Compiler

**Input tokens:** write migration, product records, normalized catalog schema, design, structure, write integration tests

**Predicate firing:**
- "write migration" → IMPLEMENT fires (Rule 2)
- "design" → ARCHITECTURE fires (Rule 11 — "design" as verb)
- "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "write integration tests" → TEST fires (Rule 5)
- No refactoring verbs → feature_addition
- No SECURITY vocabulary → SECURITY does not fire
- No "document"/"annotate"/"spec out" → DOCUMENT does not fire

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "TEST"]
- TransitionType: feature_addition
- DomainId: software-development
- GapHandler: none

**Note:** ARCHITECTURE fires twice independently ("design" + "structure") — both are separate triggers. The output ARCHITECTURE predicate appears once regardless of how many triggers fired.

---

### Layer 0-B: Router

**Input:** predicates ["IMPLEMENT", "ARCHITECTURE", "TEST"], vocabulary: product records, migration, catalog schema

**Routing decisions:**
- IMPLEMENT + data vocabulary (product records, migration) → DataMigrationFlow44 (4.4)
- ARCHITECTURE → ArchitectureFlow31 (3.1)
- TEST + migration context → UnitTestingFlow51 (5.x)
- TransitionType: feature_addition → confirms DataMigrationFlow44 (not BackendServiceFlow41)

**Critical routing decision for TEST:**
- "integration tests to verify all product records transferred" → the test purpose is migration verification
- TEST routes to UnitTestingFlow51 regardless of test type (unit vs integration — both use UnitTestingFlow51 in B18)
- TEST sub-phase: 5.x (testing flows)

**Output:**
- IMPLEMENT → DataMigrationFlow44 (sub-phase 4.4)
- ARCHITECTURE → ArchitectureFlow31 (sub-phase 3.1)
- TEST → UnitTestingFlow51 (sub-phase 5.x)
- Flow ordering: ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51

---

### Layer 0-C: Knowledge Edge

**Active edges for this request:**
- "write migration" → IMPLEMENT + feature_addition + 4.4 (Rule 2 + Rule 6)
- "design" (verb) → ARCHITECTURE (Rule 11)
- "structure" → ARCHITECTURE (Rule 3, absolute)
- "write integration tests" → TEST (Rule 5)
- "product records" → 4.4 data vocabulary
- "normalized catalog schema" → "schema" = contextual; "structure" fires absolutely (separate mention)
- No "refactor" → no Rule 9 override

**Ordering edge (critical):**
- TEST depends on IMPLEMENT (integration tests verify migration results)
- UnitTestingFlow51 HARD dependency on DataMigrationFlow44
- TEST cannot precede IMPLEMENT — this is a structural dependency, not just ordering preference

---

### Layer 0-D: Flow Factory

**Factory mode determination:**
- ProductCatalogMigration — scenario previously registered with TEST
- Factory mode: probationary_accepted (three-predicate combination is less common)
- Gate B types: row_count_assertion (DataMigrationFlow44) + test_pass_assertion (UnitTestingFlow51)

**Factory output:**
```
FlowFactoryEntry {
  ScenarioId: "ProductCatalogMigration",
  Flows: [ArchitectureFlow31, DataMigrationFlow44, UnitTestingFlow51],
  TransitionType: feature_addition,
  FactoryMode: probationary_accepted,
  GateBSet: [
    {
      Flow: DataMigrationFlow44,
      Type: row_count_assertion,
      Condition: "migrated_product_count == source_product_count"
    },
    {
      Flow: UnitTestingFlow51,
      Type: test_pass_assertion,
      Condition: "all_integration_tests_pass"
    }
  ]
}
```

---

### Layer 1: Transition Orchestrator

**Ordering input:** [ArchitectureFlow31, DataMigrationFlow44, UnitTestingFlow51]

**Orchestration rules applied:**
- Rule: ARCHITECTURE before IMPLEMENT (schema design before migration)
- Rule: TEST after IMPLEMENT (tests verify migration — HARD dependency)
- No QA → QA last rule not applicable
- No DOCUMENT → no parallel scheduling needed

**Execution schedule:**
1. ArchitectureFlow31 — design normalized catalog schema; output: schema specification
2. DataMigrationFlow44 — migrate product records per schema spec; Gate B: row_count_assertion
3. UnitTestingFlow51 — run integration tests verifying product record counts; Gate B: test_pass_assertion

**Ordering violation check:** UnitTestingFlow51 CANNOT be scheduled before DataMigrationFlow44. If ordering is violated → orchestrator error; reject plan.

---

### Layer 2: Nine-Node Trace (DataMigrationFlow44 — primary flow)

**N01 — Input:** "move 3 million product records to normalized catalog schema; tests verify transfer"

**N02 — Scope:** ProductCatalogMigration; source: legacy products table (flat); target: catalog_products_v2 (normalized); 3M records

**N03 — Analysis:** record volume assessment (3M → batch processing required); schema diff; NULL/invalid field identification

**N04 — Specification:**
- ProductCatalogMigration.cs (main migration)
- ProductMigrationBatcher.cs (batch processing — 3M records)
- ProductMigrationValidator.cs (field validation)
- ProductMigrationRollback.cs (idempotency)
- ProductCatalogIntegrationTests.cs (TEST flow)
- CatalogSchemaSpec.md (ArchitectureFlow31 output)

**N05 — Generation:** batch migration with checkpoint; schema spec from ArchitectureFlow31 used as input

**N06 — Execution:** run in batches; checkpoint after each batch; capture counts

**N07 — Pre-Gate:** regression check; TransitionType: feature_addition confirmed

**N08 — Gate B (DataMigrationFlow44):**
- Type: row_count_assertion
- Condition: `migrated_product_count == 3_000_000`
- Status: PASS

**N08 — Gate B (UnitTestingFlow51):**
- Type: test_pass_assertion
- Condition: `all_integration_tests_pass == true`
- Status: PASS

**N09 — Output:** ProductCatalogMigration delivered; schema spec confirmed; integration tests pass; all Gate B evaluations PASS.

---

### Layer 3: Teaching Point

**Core lesson from MultiStepMigration:**

TEST always waits on IMPLEMENT in migration scenarios. This is not a soft ordering preference — it is a HARD dependency. Integration tests that verify migrated record counts cannot run before the migration runs.

**Three-predicate combination [IMPLEMENT, ARCHITECTURE, TEST] ordering:**
1. ARCHITECTURE first (schema design is prerequisite for migration)
2. IMPLEMENT second (migration uses schema spec)
3. TEST third (tests verify migration results)

This sequential order is invariant for DataMigrationFlow44 scenarios. Any other order is an orchestration violation.

**Test question:** Can ArchitectureFlow31 and UnitTestingFlow51 run in parallel?
- Answer: NO. UnitTestingFlow51 depends on DataMigrationFlow44, which depends on ArchitectureFlow31. All three must be sequential.

---

### Cross-Layer Invariants Verified

| Layer | Input | Output | Consistent? |
|---|---|---|---|
| 0-A Compiler | "write migration" + "design" + "structure" + "write tests" | ["IMPLEMENT", "ARCHITECTURE", "TEST"], feature_addition | YES |
| 0-B Router | three predicates | 4.4 + 3.1 + 5.x; ARCH→IMPL→TEST order | YES |
| 0-C Knowledge | TEST depends on IMPLEMENT | hard dependency edge active | YES |
| 0-D Factory | three flows | probationary_accepted; two Gate B entries | YES |
| L1 Orchestrator | sequential ordering | ARCH(1) → IMPL(2) → TEST(3) | YES |
| L2 Nine-Node | two Gate B evaluations | both PASS | YES |
| L3 Teaching | TEST after IMPLEMENT | hard ordering constraint taught | YES |
