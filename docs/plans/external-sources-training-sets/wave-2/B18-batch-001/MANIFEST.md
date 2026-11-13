# MANIFEST — B18-batch-001
# Scenario: B18 — Customer data migration and seeding scripts
# Sub-phases: 4.4 (Data Migration and Seeding) / 4.1 (Refactoring)
# Primary predicate: IMPLEMENT
# Production date: 2026-05-06 (updated; original: 2026-05-05)
# Gate B status: ACCEPTED

## File inventory

| File | Pairs | Type |
|---|---|---|
| layer-0a-compiler-pairs-001.md | 4 | compiler (overview) |
| layer-0a-compiler-pairs-002.md | 6 | compiler |
| layer-0a-compiler-pairs-003.md | 5 | compiler |
| layer-0a-compiler-pairs-004.md | 5 | compiler |
| layer-0a-compiler-pairs-005.md | 5 | compiler |
| layer-0a-compiler-pairs-006.md | 6 | compiler |
| layer-0a-compiler-pairs-007.md | 6 | compiler |
| layer-0a-compiler-pairs-008.md | 6 | compiler |
| layer-0a-compiler-pairs-009.md | 5 | compiler |
| layer-0a-compiler-pairs-010.md | 6 | compiler |
| layer-0a-compiler-pairs-011.md | 5 | compiler |
| layer-0a-compiler-pairs-012.md | 5 | compiler |
| layer-0a-compiler-pairs-013.md | 5 | compiler — AddressBook migration |
| layer-0a-compiler-pairs-014.md | 5 | compiler — Analytics/EventLog migration |
| layer-0a-compiler-pairs-015.md | 5 | compiler — SearchIndex rebuild/seed |
| layer-0a-compiler-pairs-016.md | 5 | compiler — ProductVariants seeding |
| layer-0a-compiler-pairs-017.md | 5 | compiler — InvoiceHistory migration |
| layer-0a-compiler-pairs-018.md | 5 | compiler — TaxRecord migration |
| layer-0a-compiler-pairs-019.md | 5 | compiler — AuditLog migration |
| layer-0a-compiler-pairs-020.md | 5 | compiler — SessionData migration |
| layer-0a-compiler-pairs-021.md | 5 | compiler — UserPreferences seeding |
| layer-0a-compiler-pairs-022.md | 5 | compiler — ApiKeyRotation migration |
| layer-0a-compiler-pairs-023.md | 5 | compiler — SubscriptionBilling migration |
| layer-0a-compiler-pairs-024.md | 5 | compiler — complete predicate rule reference |
| layer-0b-router-pairs-001.md | 4 | router (overview) |
| layer-0b-router-pairs-002.md | 5 | router |
| layer-0b-router-pairs-003.md | 5 | router |
| layer-0b-router-pairs-004.md | 5 | router |
| layer-0b-router-pairs-005.md | 5 | router |
| layer-0b-router-pairs-006.md | 5 | router |
| layer-0b-router-pairs-007.md | 6 | router |
| layer-0b-router-pairs-008.md | 5 | router |
| layer-0b-router-pairs-009.md | 5 | router |
| layer-0b-router-pairs-010.md | 6 | router |
| layer-0b-router-pairs-011.md | 5 | router |
| layer-0b-router-pairs-012.md | 5 | router |
| layer-0b-router-pairs-013.md | 5 | router — AddressBook routing |
| layer-0b-router-pairs-014.md | 5 | router — EventLog routing |
| layer-0b-router-pairs-015.md | 5 | router — SearchIndex routing |
| layer-0b-router-pairs-016.md | 5 | router — ProductVariants routing |
| layer-0b-router-pairs-017.md | 5 | router — InvoiceHistory routing |
| layer-0b-router-pairs-018.md | 5 | router — TaxRecord routing |
| layer-0b-router-pairs-019.md | 5 | router — AuditLog routing |
| layer-0b-router-pairs-020.md | 5 | router — SessionData routing |
| layer-0b-router-pairs-021.md | 5 | router — UserPreferences routing |
| layer-0b-router-pairs-022.md | 5 | router — ApiKeyRotation routing |
| layer-0b-router-pairs-023.md | 5 | router — SubscriptionBilling routing |
| layer-0b-router-pairs-024.md | 5 | router — complete router reference |
| layer-0c-knowledge-edge-pairs-001.md | 4 | knowledge_edge (overview) |
| layer-0c-knowledge-edge-pairs-002.md | 5 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-003.md | 6 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-004.md | 5 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-005.md | 5 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-006.md | 5 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-007.md | 5 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-008.md | 6 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-009.md | 5 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-010.md | 5 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-011.md | 5 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-012.md | 5 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-013.md | 5 | knowledge_edge — AddressBook edges |
| layer-0c-knowledge-edge-pairs-014.md | 5 | knowledge_edge — EventLog edges |
| layer-0c-knowledge-edge-pairs-015.md | 5 | knowledge_edge — SearchIndex edges |
| layer-0c-knowledge-edge-pairs-016.md | 5 | knowledge_edge — ProductVariants edges |
| layer-0c-knowledge-edge-pairs-017.md | 5 | knowledge_edge — InvoiceHistory edges |
| layer-0c-knowledge-edge-pairs-018.md | 5 | knowledge_edge — TaxRecord edges |
| layer-0c-knowledge-edge-pairs-019.md | 5 | knowledge_edge — AuditLog edges |
| layer-0c-knowledge-edge-pairs-020.md | 5 | knowledge_edge — SessionData edges |
| layer-0c-knowledge-edge-pairs-021.md | 5 | knowledge_edge — UserPreferences edges |
| layer-0c-knowledge-edge-pairs-022.md | 5 | knowledge_edge — ApiKeyRotation edges |
| layer-0c-knowledge-edge-pairs-023.md | 5 | knowledge_edge — SubscriptionBilling edges |
| layer-0c-knowledge-edge-pairs-024.md | 5 | knowledge_edge — complete edge reference |
| layer-0d-flow-factory-pairs-001.md | 4 | factory_selection (overview) |
| layer-0d-flow-factory-pairs-002.md | 5 | factory_selection |
| layer-0d-flow-factory-pairs-003.md | 5 | factory_selection |
| layer-0d-flow-factory-pairs-004.md | 5 | factory_selection |
| layer-0d-flow-factory-pairs-005.md | 5 | factory_selection |
| layer-0d-flow-factory-pairs-006.md | 5 | factory_selection |
| layer-0d-flow-factory-pairs-007.md | 5 | factory_selection |
| layer-0d-flow-factory-pairs-008.md | 5 | factory_selection |
| layer-0d-flow-factory-pairs-009.md | 5 | factory_selection |
| layer-0d-flow-factory-pairs-010.md | 5 | factory_selection |
| layer-0d-flow-factory-pairs-011.md | 5 | factory_selection |
| layer-0d-flow-factory-pairs-012.md | 6 | factory_selection |
| layer-0d-flow-factory-pairs-013.md | 5 | factory_selection — AddressBook factory |
| layer-0d-flow-factory-pairs-014.md | 5 | factory_selection — EventLog factory |
| layer-0d-flow-factory-pairs-015.md | 5 | factory_selection — SearchIndex factory |
| layer-0d-flow-factory-pairs-016.md | 5 | factory_selection — ProductVariants factory |
| layer-0d-flow-factory-pairs-017.md | 5 | factory_selection — InvoiceHistory factory |
| layer-0d-flow-factory-pairs-018.md | 5 | factory_selection — TaxRecord factory |
| layer-0d-flow-factory-pairs-019.md | 5 | factory_selection — AuditLog factory |
| layer-0d-flow-factory-pairs-020.md | 5 | factory_selection — SessionData factory |
| layer-0d-flow-factory-pairs-021.md | 5 | factory_selection — UserPreferences factory |
| layer-0d-flow-factory-pairs-022.md | 5 | factory_selection — ApiKeyRotation factory |
| layer-0d-flow-factory-pairs-023.md | 5 | factory_selection — SubscriptionBilling factory |
| layer-0d-flow-factory-pairs-024.md | 5 | factory_selection — complete factory reference |
| layer-1-transition-orchestrator-pairs-001.md | 4 | transition_orchestrator (overview) |
| layer-1-transition-orchestrator-pairs-002.md | 5 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-003.md | 5 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-004.md | 5 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-005.md | 6 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-006.md | 5 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-007.md | 5 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-008.md | 5 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-009.md | 6 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-010.md | 5 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-011.md | 5 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-012.md | 5 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-013.md | 5 | transition_orchestrator — AddressBook orchestration |
| layer-1-transition-orchestrator-pairs-014.md | 5 | transition_orchestrator — EventLog orchestration |
| layer-1-transition-orchestrator-pairs-015.md | 5 | transition_orchestrator — SearchIndex orchestration |
| layer-1-transition-orchestrator-pairs-016.md | 5 | transition_orchestrator — ProductVariants orchestration |
| layer-1-transition-orchestrator-pairs-017.md | 5 | transition_orchestrator — InvoiceHistory orchestration |
| layer-1-transition-orchestrator-pairs-018.md | 5 | transition_orchestrator — TaxRecord orchestration |
| layer-1-transition-orchestrator-pairs-019.md | 5 | transition_orchestrator — AuditLog orchestration |
| layer-1-transition-orchestrator-pairs-020.md | 5 | transition_orchestrator — SessionData orchestration |
| layer-1-transition-orchestrator-pairs-021.md | 5 | transition_orchestrator — UserPreferences orchestration |
| layer-1-transition-orchestrator-pairs-022.md | 5 | transition_orchestrator — ApiKeyRotation orchestration |
| layer-1-transition-orchestrator-pairs-023.md | 5 | transition_orchestrator — SubscriptionBilling orchestration |
| layer-1-transition-orchestrator-pairs-024.md | 5 | transition_orchestrator — complete orchestration reference |
| layer-2-nine-node-4.4-pairs-001.md | 4 | nine_node (overview) |
| layer-2-nine-node-4.4-pairs-002.md | 5 | nine_node |
| layer-2-nine-node-4.4-pairs-003.md | 5 | nine_node |
| layer-2-nine-node-4.4-pairs-004.md | 5 | nine_node |
| layer-2-nine-node-4.4-pairs-005.md | 5 | nine_node |
| layer-2-nine-node-4.4-pairs-006.md | 5 | nine_node |
| layer-2-nine-node-4.4-pairs-007.md | 5 | nine_node |
| layer-2-nine-node-4.4-pairs-008.md | 5 | nine_node |
| layer-2-nine-node-4.4-pairs-009.md | 5 | nine_node |
| layer-2-nine-node-4.4-pairs-010.md | 5 | nine_node |
| layer-2-nine-node-4.4-pairs-011.md | 5 | nine_node |
| layer-2-nine-node-4.4-pairs-012.md | 5 | nine_node |
| layer-2-nine-node-4.4-pairs-013.md | 1 (7-layer) | nine_node — AddressBookMigration (IMPLEMENT, 750K rows) |
| layer-2-nine-node-4.4-pairs-014.md | 1 (7-layer) | nine_node — AnalyticsEventLogMigration (ETL, 4.5M records) |
| layer-2-nine-node-4.4-pairs-015.md | 1 (7-layer) | nine_node — SearchIndexRebuild (seed, 95K products) |
| layer-2-nine-node-4.4-pairs-016.md | 1 (7-layer) | nine_node — ProductVariantsSeeding (seed, 43.2K variants) |
| layer-2-nine-node-4.4-pairs-017.md | 1 (7-layer) | nine_node — InvoiceHistoryMigration+DOCUMENT (3.2M rows) |
| layer-2-nine-node-4.4-pairs-018.md | 1 (7-layer) | nine_node — TaxRecordMigration one-to-many (2.1M→3.8M rows) |
| layer-2-nine-node-4.4-pairs-019.md | 1 (7-layer) | nine_node — AuditLogMigration+SECURITY (PII, 1.4M rows) |
| layer-2-nine-node-4.4-pairs-020.md | 1 (7-layer) | nine_node — UserPreferencesSeeding (1.68M preference rows) |
| layer-2-nine-node-4.4-pairs-021.md | 1 (7-layer) | nine_node — ApiKeyRotationMigration+SECURITY (85K keys) |
| layer-2-nine-node-4.4-pairs-022.md | 1 (7-layer) | nine_node — SubscriptionBillingMigration+API_CONTRACT (920K rows) |
| layer-2-nine-node-4.4-pairs-023.md | 1 (7-layer) | nine_node — LegacySessionDataMigration (probationary_rejected, FAIL) |
| layer-2-nine-node-4.4-pairs-024.md | 1 (7-layer) | nine_node — complete nine-node rule summary |
| layer-3-teaching-unit-pairs-001.md | 4 | teaching_unit (overview) |
| layer-3-teaching-unit-pairs-002.md | 5 | teaching_unit |
| layer-3-teaching-unit-pairs-003.md | 5 | teaching_unit |
| layer-3-teaching-unit-pairs-004.md | 5 | teaching_unit |
| layer-3-teaching-unit-pairs-005.md | 5 | teaching_unit |
| layer-3-teaching-unit-pairs-006.md | 5 | teaching_unit |
| layer-3-teaching-unit-pairs-007.md | 5 | teaching_unit |
| layer-3-teaching-unit-pairs-008.md | 5 | teaching_unit |
| layer-3-teaching-unit-pairs-009.md | 5 | teaching_unit |
| layer-3-teaching-unit-pairs-010.md | 5 | teaching_unit |
| layer-3-teaching-unit-pairs-011.md | 5 | teaching_unit |
| layer-3-teaching-unit-pairs-012.md | 5 | teaching_unit |
| layer-3-teaching-unit-pairs-013.md | 5 | teaching_unit — AddressBook teaching |
| layer-3-teaching-unit-pairs-014.md | 5 | teaching_unit — EventLog teaching |
| layer-3-teaching-unit-pairs-015.md | 5 | teaching_unit — SearchIndex teaching |
| layer-3-teaching-unit-pairs-016.md | 5 | teaching_unit — ProductVariants teaching |
| layer-3-teaching-unit-pairs-017.md | 5 | teaching_unit — InvoiceHistory teaching |
| layer-3-teaching-unit-pairs-018.md | 5 | teaching_unit — TaxRecord teaching |
| layer-3-teaching-unit-pairs-019.md | 5 | teaching_unit — AuditLog teaching |
| layer-3-teaching-unit-pairs-020.md | 5 | teaching_unit — SessionData teaching |
| layer-3-teaching-unit-pairs-021.md | 5 | teaching_unit — UserPreferences teaching |
| layer-3-teaching-unit-pairs-022.md | 5 | teaching_unit — ApiKeyRotation teaching |
| layer-3-teaching-unit-pairs-023.md | 5 | teaching_unit — SubscriptionBilling teaching |
| layer-3-teaching-unit-pairs-024.md | 5 | teaching_unit — complete teaching summary |
| held-out-and-corruption-pairs-001.md | 4 | held-out-corruption (overview) |
| held-out-and-corruption-pairs-002.md | 5 | held-out-corruption |
| held-out-and-corruption-pairs-003.md | 5 | held-out-corruption |
| held-out-and-corruption-pairs-004.md | 5 | held-out-corruption |
| held-out-and-corruption-pairs-005.md | 6 | held-out-corruption |
| held-out-and-corruption-pairs-006.md | 6 | held-out-corruption |
| held-out-and-corruption-pairs-007.md | 5 | held-out-corruption |
| held-out-and-corruption-pairs-008.md | 6 | held-out-corruption |
| held-out-and-corruption-pairs-009.md | 6 | held-out-corruption |
| held-out-and-corruption-pairs-010.md | 6 | held-out-corruption |
| held-out-and-corruption-pairs-011.md | 6 | held-out-corruption |
| held-out-and-corruption-pairs-012.md | 6 | held-out-corruption |
| held-out-and-corruption-pairs-013.md | 5 | held-out-corruption — AddressBook |
| held-out-and-corruption-pairs-014.md | 5 | held-out-corruption — EventLog |
| held-out-and-corruption-pairs-015.md | 5 | held-out-corruption — SearchIndex |
| held-out-and-corruption-pairs-016.md | 5 | held-out-corruption — ProductVariants |
| held-out-and-corruption-pairs-017.md | 5 | held-out-corruption — InvoiceHistory |
| held-out-and-corruption-pairs-018.md | 5 | held-out-corruption — TaxRecord |
| held-out-and-corruption-pairs-019.md | 5 | held-out-corruption — AuditLog |
| held-out-and-corruption-pairs-020.md | 5 | held-out-corruption — SessionData |
| held-out-and-corruption-pairs-021.md | 5 | held-out-corruption — UserPreferences |
| held-out-and-corruption-pairs-022.md | 5 | held-out-corruption — ApiKeyRotation |
| held-out-and-corruption-pairs-023.md | 5 | held-out-corruption — SubscriptionBilling |
| held-out-and-corruption-pairs-024.md | 5 | held-out-corruption — complete HO reference |
| cross-layer-synthesis-001.md | 1 (7-layer) | cross-layer-synthesis |
| cross-layer-synthesis-002.md | 1 (7-layer) | cross-layer-synthesis |
| cross-layer-synthesis-003.md | 1 (7-layer) | cross-layer-synthesis |
| cross-layer-synthesis-004.md | 1 (7-layer) | cross-layer-synthesis |
| cross-layer-synthesis-005.md | 1 (7-layer) | cross-layer-synthesis — AddressBookMigration (IMPLEMENT, feature_addition) |
| cross-layer-synthesis-006.md | 1 (7-layer) | cross-layer-synthesis — ProductVariantsTableConsolidation (IMPLEMENT+ARCHITECTURE, refactoring) |
| cross-layer-synthesis-007.md | 1 (7-layer) | cross-layer-synthesis — InvoiceHistoryMigration (IMPLEMENT+ARCHITECTURE+DOCUMENT, feature_addition) |
| cross-layer-synthesis-008.md | 1 (7-layer) | cross-layer-synthesis — SubscriptionBillingMigration (IMPLEMENT+ARCHITECTURE+SECURITY+TEST+DOCUMENT, feature_addition) |

**Total files: 201** (192 numbered layer files + 8 cross-layer synthesis + 1 MANIFEST)

## File count by layer

| Layer | Files | Type |
|---|---|---|
| Layer 0-A (compiler) | 24 | compiler |
| Layer 0-B (router) | 24 | router |
| Layer 0-C (knowledge-edge) | 24 | knowledge_edge |
| Layer 0-D (flow-factory) | 24 | factory_selection |
| Layer 1 (transition orchestrator) | 24 | transition_orchestrator |
| Layer 2 (nine-node-4.4) | 24 | nine_node |
| Layer 3 (teaching-unit) | 24 | teaching_unit |
| Held-out and corruption | 24 | held-out-corruption |
| Cross-layer synthesis | 8 | cross-layer-synthesis |
| MANIFEST | 1 | manifest |
| **Total** | **201** | |

## Layer 2 nine-node scenario results

| Scenario | Records | Gate B Type | Gate B Condition | Result |
|---|---|---|---|---|
| OrderHistoryMigration | 2.5M | row_count_assertion | migrated == source | PASS |
| ProductCatalogMigration | 3M | row_count_assertion | migrated == source | PASS |
| UserAccountMigration | 1.8M | row_count_assertion | migrated == source | PASS |
| ProductInventorySeeding | N/A | idempotency_check | seeded_count_stable | PASS |
| LegacyOrderEtlPipeline | incremental | row_count_assertion | extracted == loaded | PASS |
| OrderServiceRefactoring | N/A | test_regression_check | all_tests_pass | PASS |
| PaymentServiceRefactoring | N/A | test_regression_check | all_tests_pass | PASS |
| MultiStepMigration | 3M | row_count_assertion + test_pass | both conditions | PASS |
| LegacyPaymentMigration | N/A | row_count_assertion | migrated == source | FAIL (probationary_rejected) |
| OrderArchiveMigration | 5M | row_count_assertion | migrated == source | PASS |
| GdprUserDataMigration | 4M | 5 Gate B evaluations | all conditions | PASS |
| AddressBookMigration | 750K | row_count_assertion | migrated == source | PASS |
| AnalyticsEventLogMigration | 4.5M | row_count_assertion | extracted == loaded | PASS |
| SearchIndexRebuild | 95K | idempotency_check | seeded_count_stable | PASS |
| ProductVariantsSeeding | 43.2K | idempotency_check | seeded_count_stable | PASS |
| InvoiceHistoryMigration+DOCUMENT | 3.2M | row_count_assertion + doc_complete | both conditions | PASS |
| TaxRecordMigration (one-to-many) | 2.1M→3.8M | row_count_assertion | migrated == 3.8M target | PASS |
| AuditLogMigration+SECURITY | 1.4M | row_count_assertion + security_scan_pass | both conditions | PASS |
| UserPreferencesSeeding | 1.68M | idempotency_check | seeded_count_stable | PASS |
| ApiKeyRotationMigration+SECURITY | 85K | row_count_assertion + security_scan_pass | all_keys_hashed | PASS |
| SubscriptionBillingMigration+API | 920K | row_count_assertion + api_contract_verified | both conditions | PASS |
| LegacySessionDataMigration | N/A | row_count_assertion | 4,200 malformed tokens | FAIL (probationary_rejected) |

## Flows used in B18-batch-001

| Flow | Sub-phase | Trigger |
|---|---|---|
| DataMigrationFlow44 | 4.4 | migrate/seed/ETL/fixture/data vocabulary |
| BackendServiceFlow41 | 4.1 | service/class/method/repository vocabulary |
| ArchitectureFlow31 | 3.1 | structure/architecture/pattern/design triggers |
| UnitTestingFlow51 | 5.x | write tests/test/verify (TEST predicate) |
| DocumentationFlow21 | 2.1 | document/annotate/spec out/runbook triggers |
| SecurityReviewFlow | security | encrypted/PII/GDPR/credentials triggers |

## TransitionType distribution

| TransitionType | Count | Triggered by |
|---|---|---|
| feature_addition | majority | write migration, migrate data, seed, ETL |
| refactoring | secondary | refactor, extract (code), rename, consolidate, restructure, reorganize, move (code), clean up |

## Key teaching points

1. "refactor", "extract" (code target), "rename", "consolidate", "restructure", "reorganize", "move" (code), "clean up" → TransitionType: refactoring (Rule 9 is absolute — wins over all)
2. "write the migration" / "migrate data/records" → TransitionType: feature_addition (Rule 2/5 — migration is a new artifact)
3. "structure" fires ARCHITECTURE in ANY context, including "data structure", "normalized structure", "file structure", "schema structure" (Rule 3, absolute, no contextual suppression)
4. "seed" and "fixture" → IMPLEMENT → 4.4 (data vocabulary, not test vocabulary; "tests" as noun ≠ TEST predicate)
5. IMPLEMENT routes to 4.1 (service/logic/class), 4.4 (migrate/seed/ETL/data/records), or 4.3 (connector/webhook/API) based on co-occurrence vocabulary
6. TEST fires only on "write tests" / "add tests" / "create tests" (verb form); "tests" as standalone noun does not fire TEST
7. ETL (Extract, Transform, Load) → IMPLEMENT → 4.4 (data pipeline, not 4.3 integration connector)
8. "design" (verb) fires ARCHITECTURE even in database/migration context
9. SECURITY fires absolutely on: encrypted, PII, GDPR, credentials, authentication, authorization, access control, sensitive data
10. DocumentationFlow21 is parallel-eligible (can run alongside SecurityReviewFlow and UnitTestingFlow51)
11. TEST always waits on IMPLEMENT in migration scenarios (hard dependency — tests verify migration results)
12. QA fires only on "QA" abbreviation and is always last in orchestration; never implied by "verify", "integration tests", or "review"

## v6 corruption types

| Type | Description | Detection | Fix |
|---|---|---|---|
| v6-01 | ARCHITECTURE suppressed despite absolute trigger present | Check: is "structure"/"architecture"/"pattern"/"design" present but ARCHITECTURE missing? | Add ARCHITECTURE |
| v6-02 | "refactor" or refactoring verb → feature_addition (wrong) | Check: is refactoring verb present but TransitionType = feature_addition? | Set TransitionType = refactoring |
| v6-03 | Data migration vocabulary routed to 4.1 instead of 4.4 | Check: is data vocabulary present but sub-phase = 4.1? | Set sub-phase = 4.4 / DataMigrationFlow44 |

## Cross-layer synthesis traces

| File | Scenario | Predicates | TransitionType | Key lesson |
|---|---|---|---|---|
| cross-layer-synthesis-001.md | CustomerDataMigration | IMPLEMENT+ARCHITECTURE | feature_addition | "schema structure" — "structure" fires ARCHITECTURE absolutely |
| cross-layer-synthesis-002.md | OrderServiceRefactoring | IMPLEMENT | refactoring | Rule 9: "refactor" always sets TransitionType: refactoring |
| cross-layer-synthesis-003.md | MultiStepMigration | IMPLEMENT+ARCHITECTURE+TEST | feature_addition | TEST waits on IMPLEMENT (hard dependency) |
| cross-layer-synthesis-004.md | GdprUserDataMigration | IMPLEMENT+ARCHITECTURE+SECURITY+TEST+DOCUMENT | feature_addition | Five predicates; five flows; five Gate B evaluations |
| cross-layer-synthesis-005.md | AddressBookMigration | IMPLEMENT | feature_addition | "normalized" does NOT fire ARCHITECTURE; only "structure" noun fires |
| cross-layer-synthesis-006.md | ProductVariantsTableConsolidation | IMPLEMENT+ARCHITECTURE | refactoring | "consolidate" = refactoring verb; refactoring+4.4 → DataMigrationFlow44 |
| cross-layer-synthesis-007.md | InvoiceHistoryMigration | IMPLEMENT+ARCHITECTURE+DOCUMENT | feature_addition | "schema" alone ≠ ARCHITECTURE; DOC parallel-eligible for runbooks |
| cross-layer-synthesis-008.md | SubscriptionBillingMigration | IMPLEMENT+ARCHITECTURE+SECURITY+TEST+DOCUMENT | feature_addition | "credentials" absolute SECURITY trigger; "verify" ≠ TEST in migration context |

## B18 absolute rules (never overridden)

- Rule 3: "structure" token → ARCHITECTURE fires, no exceptions
- Rule 7: "architecture" token → ARCHITECTURE fires, no exceptions
- Rule 7: "pattern" token → ARCHITECTURE fires, no exceptions
- Rule 9: "refactor" verb → TransitionType: refactoring, always
- Rule 14: "encrypted" / "PII" → SECURITY fires, no exceptions
- Rule 2: "write migration" → IMPLEMENT + feature_addition + 4.4
- Rule 6: data vocabulary (records/rows/tables/ETL/seed/fixture) → 4.4 routing
