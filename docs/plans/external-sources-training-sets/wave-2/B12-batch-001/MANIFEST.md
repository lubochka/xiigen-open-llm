# MANIFEST — B12-batch-001
# Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Sub-phase: 3.3 (API Contract Design)
# Primary predicate: API_CONTRACT
# Production date: 2026-05-05
# Gate B status: ACCEPTED

## File inventory

### Original files (wave-1 seed)
| File | Pairs | Type |
|------|-------|------|
| layer-0a-compiler-pairs.md | 3 | compiler |
| layer-0b-router-pairs.md | 3 | router |
| layer-0c-knowledge-edge-pairs.md | 2 | knowledge_edge |
| layer-0d-flow-factory-pairs.md | 4 | factory_selection |
| layer-1-transition-orchestrator-pairs.md | 2 | transition_orchestrator |
| layer-2-nine-node-3.3-pairs.md | 9 | nine_node |
| layer-3-teaching-unit-pairs.md | 5 | teaching_unit |
| held-out-and-corruption-pairs.md | 3 held-out + 3 corruption | held_out + corruption |

### New files (B12-batch-001 wave-2 expansion — 192 files)

#### Layer 0-A — Compiler Pairs (23 files)
| File | Pairs | Type |
|------|-------|------|
| layer-0a-compiler-pairs-002.md | 4 | compiler (gRPC contract) |
| layer-0a-compiler-pairs-003.md | 4 | compiler (webhook contract) |
| layer-0a-compiler-pairs-004.md | 4 | compiler (data contracts) |
| layer-0a-compiler-pairs-005.md | 4 | compiler (database schema vs API schema) |
| layer-0a-compiler-pairs-006.md | 4 | compiler (design the API) |
| layer-0a-compiler-pairs-007.md | 4 | compiler (write OpenAPI + authentication) |
| layer-0a-compiler-pairs-008.md | 4 | compiler (interface disambiguation) |
| layer-0a-compiler-pairs-009.md | 4 | compiler (API versioning) |
| layer-0a-compiler-pairs-010.md | 4 | compiler (five-predicate OAuth) |
| layer-0a-compiler-pairs-011.md | 4 | compiler (v6 corruptions) |
| layer-0a-compiler-pairs-012.md | 4 | compiler (B12 rule reference tables) |
| layer-0a-compiler-pairs-013.md | 5 | compiler (OrderWebhooks domain) |
| layer-0a-compiler-pairs-014.md | 5 | compiler (InventoryAPI + database schema negative) |
| layer-0a-compiler-pairs-015.md | 5 | compiler (UserManagementAPI + interface) |
| layer-0a-compiler-pairs-016.md | 5 | compiler (ShippingRatesAPI + swagger) |
| layer-0a-compiler-pairs-017.md | 5 | compiler (ProductSearchAPI + GraphQL) |
| layer-0a-compiler-pairs-018.md | 5 | compiler (ReportingAPI + scaffold IMPLEMENT) |
| layer-0a-compiler-pairs-019.md | 5 | compiler (NotificationAPI + gRPC) |
| layer-0a-compiler-pairs-020.md | 5 | compiler (FileUploadAPI + multipart) |
| layer-0a-compiler-pairs-021.md | 5 | compiler (PartnerAPI + publish not IMPLEMENT) |
| layer-0a-compiler-pairs-022.md | 5 | compiler (API versioning + update not IMPLEMENT) |
| layer-0a-compiler-pairs-023.md | 5 | compiler (IMPLEMENT+API_CONTRACT dual cases) |
| layer-0a-compiler-pairs-024.md | 5 | compiler (complete predicate rule reference) |

#### Layer 0-B — Router Pairs (23 files)
| File | Pairs | Type |
|------|-------|------|
| layer-0b-router-pairs-002.md | 4 | router (gRPC → 3.3 + 2.1) |
| layer-0b-router-pairs-003.md | 4 | router (webhook → 3.3 + 2.1) |
| layer-0b-router-pairs-004.md | 4 | router (data contracts → 3.3 + 2.1) |
| layer-0b-router-pairs-005.md | 4 | router (DOCUMENT only → 2.1) |
| layer-0b-router-pairs-006.md | 4 | router (3-decision: 3.1 + 3.3 + 2.1) |
| layer-0b-router-pairs-007.md | 4 | router (4-decision: 4.3 + 3.3 + 2.1 + 3.4) |
| layer-0b-router-pairs-008.md | 4 | router (event bus interface) |
| layer-0b-router-pairs-009.md | 4 | router (API versioning) |
| layer-0b-router-pairs-010.md | 4 | router (5-decision OAuth) |
| layer-0b-router-pairs-011.md | 4 | router (v6 corruption routing impact) |
| layer-0b-router-pairs-012.md | 4 | router (complete routing decision table) |
| layer-0b-router-pairs-013.md | 4 | router (OrderWebhooks routing) |
| layer-0b-router-pairs-014.md | 4 | router (InventoryAPI routing + DOCUMENT only case) |
| layer-0b-router-pairs-015.md | 4 | router (UserManagementAPI routing) |
| layer-0b-router-pairs-016.md | 4 | router (ShippingRatesAPI routing) |
| layer-0b-router-pairs-017.md | 4 | router (ProductSearchAPI GraphQL vs REST) |
| layer-0b-router-pairs-018.md | 4 | router (ReportingAPI routing) |
| layer-0b-router-pairs-019.md | 4 | router (NotificationAPI routing) |
| layer-0b-router-pairs-020.md | 4 | router (FileUploadAPI routing) |
| layer-0b-router-pairs-021.md | 4 | router (PartnerAPI routing) |
| layer-0b-router-pairs-022.md | 4 | router (API versioning routing) |
| layer-0b-router-pairs-023.md | 4 | router (IMPLEMENT+API_CONTRACT routing patterns) |
| layer-0b-router-pairs-024.md | 4 | router (complete routing decision table) |

#### Layer 0-C — Knowledge Edge Pairs (23 files)
| File | Pairs | Type |
|------|-------|------|
| layer-0c-knowledge-edge-pairs-002.md | 3 | knowledge_edge (contract always fires) |
| layer-0c-knowledge-edge-pairs-003.md | 4 | knowledge_edge (schema co-occurrence) |
| layer-0c-knowledge-edge-pairs-004.md | 4 | knowledge_edge (interface disambiguation) |
| layer-0c-knowledge-edge-pairs-005.md | 4 | knowledge_edge (design + API combination) |
| layer-0c-knowledge-edge-pairs-006.md | 4 | knowledge_edge (authentication always SECURITY) |
| layer-0c-knowledge-edge-pairs-007.md | 4 | knowledge_edge (Phase 3 sub-phase map) |
| layer-0c-knowledge-edge-pairs-008.md | 4 | knowledge_edge (TransitionType for API work) |
| layer-0c-knowledge-edge-pairs-009.md | 4 | knowledge_edge (API_CONTRACT routing) |
| layer-0c-knowledge-edge-pairs-010.md | 4 | knowledge_edge (v6 corruption types) |
| layer-0c-knowledge-edge-pairs-011.md | 4 | knowledge_edge (complete vocabulary table) |
| layer-0c-knowledge-edge-pairs-012.md | 3 | knowledge_edge (summary reference) |
| layer-0c-knowledge-edge-pairs-013.md | 3 | knowledge_edge (webhook double trigger, idempotent, HMAC) |
| layer-0c-knowledge-edge-pairs-014.md | 3 | knowledge_edge (database vs API schema, route, payload) |
| layer-0c-knowledge-edge-pairs-015.md | 3 | knowledge_edge (interface disambiguation, auth, IMPLEMENT) |
| layer-0c-knowledge-edge-pairs-016.md | 3 | knowledge_edge (swagger+openapi, idempotent, integration) |
| layer-0c-knowledge-edge-pairs-017.md | 3 | knowledge_edge (design+graphql dual, schema gate, IMPLEMENT) |
| layer-0c-knowledge-edge-pairs-018.md | 3 | knowledge_edge (spec→DOCUMENT, scaffold, develop) |
| layer-0c-knowledge-edge-pairs-019.md | 3 | knowledge_edge (gRPC modifier, webhook, notification domain) |
| layer-0c-knowledge-edge-pairs-020.md | 3 | knowledge_edge (auth in upload, idempotent, multipart) |
| layer-0c-knowledge-edge-pairs-021.md | 3 | knowledge_edge (integration co-occurrence, dual fire, publish) |
| layer-0c-knowledge-edge-pairs-022.md | 3 | knowledge_edge (versioned modifier, update, design) |
| layer-0c-knowledge-edge-pairs-023.md | 3 | knowledge_edge (build vs document, update, IMPLEMENT verbs) |
| layer-0c-knowledge-edge-pairs-024.md | 3 | knowledge_edge (complete boundary summary + rules index) |

#### Layer 0-D — Flow Factory Pairs (23 files)
| File | Pairs | Type |
|------|-------|------|
| layer-0d-flow-factory-pairs-002.md | 4 | factory (deterministic_generic) |
| layer-0d-flow-factory-pairs-003.md | 4 | factory (probationary_accepted) |
| layer-0d-flow-factory-pairs-004.md | 4 | factory (trusted_learned) |
| layer-0d-flow-factory-pairs-005.md | 4 | factory (probationary_rejected) |
| layer-0d-flow-factory-pairs-006.md | 4 | factory (ARCHITECTURE + API_CONTRACT) |
| layer-0d-flow-factory-pairs-007.md | 4 | factory (IMPLEMENT + API_CONTRACT + DOCUMENT) |
| layer-0d-flow-factory-pairs-008.md | 4 | factory (API_CONTRACT + DOCUMENT + SECURITY) |
| layer-0d-flow-factory-pairs-009.md | 4 | factory (five-predicate map) |
| layer-0d-flow-factory-pairs-010.md | 4 | factory (v6 corruption) |
| layer-0d-flow-factory-pairs-011.md | 4 | factory (DOCUMENT sub-phase routing) |
| layer-0d-flow-factory-pairs-012.md | 4 | factory (complete factory map) |
| layer-0d-flow-factory-pairs-013.md | 3 | factory (OrderWebhooks) |
| layer-0d-flow-factory-pairs-014.md | 4 | factory (InventoryAPI + DOCUMENT only) |
| layer-0d-flow-factory-pairs-015.md | 4 | factory (UserManagementAPI) |
| layer-0d-flow-factory-pairs-016.md | 3 | factory (ShippingRatesAPI) |
| layer-0d-flow-factory-pairs-017.md | 4 | factory (ProductSearchAPI) |
| layer-0d-flow-factory-pairs-018.md | 3 | factory (ReportingAPI) |
| layer-0d-flow-factory-pairs-019.md | 3 | factory (NotificationAPI) |
| layer-0d-flow-factory-pairs-020.md | 3 | factory (FileUploadAPI) |
| layer-0d-flow-factory-pairs-021.md | 4 | factory (PartnerAPI) |
| layer-0d-flow-factory-pairs-022.md | 3 | factory (API versioning) |
| layer-0d-flow-factory-pairs-023.md | 3 | factory (IMPLEMENT+API_CONTRACT) |
| layer-0d-flow-factory-pairs-024.md | 4 | factory (complete factory map reference) |

#### Layer 1 — Transition Orchestrator Pairs (23 files)
| File | Pairs | Type |
|------|-------|------|
| layer-1-transition-orchestrator-pairs-002.md | 4 | orchestrator (API_CONTRACT + DOCUMENT, parallel) |
| layer-1-transition-orchestrator-pairs-003.md | 4 | orchestrator (+ SECURITY, three flows) |
| layer-1-transition-orchestrator-pairs-004.md | 4 | orchestrator (IMPLEMENT + API_CONTRACT) |
| layer-1-transition-orchestrator-pairs-005.md | 4 | orchestrator (ARCHITECTURE first) |
| layer-1-transition-orchestrator-pairs-006.md | 4 | orchestrator (TransitionType distinction) |
| layer-1-transition-orchestrator-pairs-007.md | 4 | orchestrator (four flows ordered) |
| layer-1-transition-orchestrator-pairs-008.md | 4 | orchestrator (five flows) |
| layer-1-transition-orchestrator-pairs-009.md | 4 | orchestrator (QA last rule) |
| layer-1-transition-orchestrator-pairs-010.md | 4 | orchestrator (TransitionType summary) |
| layer-1-transition-orchestrator-pairs-011.md | 4 | orchestrator (canonical B12 patterns) |
| layer-1-transition-orchestrator-pairs-012.md | 4 | orchestrator (v6 ordering violations) |
| layer-1-transition-orchestrator-pairs-013.md | 4 | orchestrator (OrderWebhooks) |
| layer-1-transition-orchestrator-pairs-014.md | 4 | orchestrator (InventoryAPI) |
| layer-1-transition-orchestrator-pairs-015.md | 4 | orchestrator (UserManagementAPI) |
| layer-1-transition-orchestrator-pairs-016.md | 4 | orchestrator (ShippingRatesAPI) |
| layer-1-transition-orchestrator-pairs-017.md | 4 | orchestrator (ProductSearchAPI) |
| layer-1-transition-orchestrator-pairs-018.md | 4 | orchestrator (ReportingAPI + QA position rule) |
| layer-1-transition-orchestrator-pairs-019.md | 4 | orchestrator (NotificationAPI) |
| layer-1-transition-orchestrator-pairs-020.md | 4 | orchestrator (FileUploadAPI + idempotency) |
| layer-1-transition-orchestrator-pairs-021.md | 4 | orchestrator (PartnerAPI) |
| layer-1-transition-orchestrator-pairs-022.md | 4 | orchestrator (API versioning) |
| layer-1-transition-orchestrator-pairs-023.md | 4 | orchestrator (IMPLEMENT+API_CONTRACT orchestration) |
| layer-1-transition-orchestrator-pairs-024.md | 4 | orchestrator (complete orchestration reference) |

#### Layer 2 — Nine-Node 3.3 Pairs (23 files)
| File | Flow | Predicates | Gate B |
|------|------|------------|--------|
| layer-2-nine-node-3.3-pairs-002.md | PaymentGatewayOpenApiSpec | IMPLEMENT+API_CONTRACT+DOCUMENT | PASS |
| layer-2-nine-node-3.3-pairs-003.md | ProductCatalogGraphQLSchema | API_CONTRACT+ARCHITECTURE+DOCUMENT | PASS |
| layer-2-nine-node-3.3-pairs-004.md | UserServiceGrpcContract | API_CONTRACT+DOCUMENT | PASS |
| layer-2-nine-node-3.3-pairs-005.md | NotificationWebhookContract | API_CONTRACT+DOCUMENT | PASS |
| layer-2-nine-node-3.3-pairs-006.md | InventoryRestEndpoints | API_CONTRACT+DOCUMENT+SECURITY | PASS |
| layer-2-nine-node-3.3-pairs-007.md | AdminPanelApiRoutes | API_CONTRACT+DOCUMENT+AUTHORIZATION | PASS |
| layer-2-nine-node-3.3-pairs-008.md | CartApiInterface | ARCHITECTURE+API_CONTRACT+DOCUMENT | PASS |
| layer-2-nine-node-3.3-pairs-009.md | AuthServiceApiContract | IMPLEMENT+API_CONTRACT+DOCUMENT+SECURITY+AUTHORIZATION | PASS |
| layer-2-nine-node-3.3-pairs-010.md | EventBusApiContract | API_CONTRACT+DOCUMENT (fallback) | FALLBACK |
| layer-2-nine-node-3.3-pairs-011.md | CustomerAccountApiVersioning | API_CONTRACT+DOCUMENT | PASS |
| layer-2-nine-node-3.3-pairs-012.md | DataContractsDefinition | API_CONTRACT+DOCUMENT+ARCHITECTURE | PASS |
| layer-2-nine-node-3.3-pairs-013.md | OrderWebhookContract | API_CONTRACT alone | PASS |
| layer-2-nine-node-3.3-pairs-014.md | InventoryApiSchema | API_CONTRACT+DOCUMENT | PASS |
| layer-2-nine-node-3.3-pairs-015.md | UserManagementApiContract | API_CONTRACT alone | PASS |
| layer-2-nine-node-3.3-pairs-016.md | ShippingRatesRestApi | API_CONTRACT+IMPLEMENT | PASS |
| layer-2-nine-node-3.3-pairs-017.md | ProductSearchGraphQL | API_CONTRACT alone | PASS |
| layer-2-nine-node-3.3-pairs-018.md | ReportingApiSpec | API_CONTRACT+DOCUMENT | PASS |
| layer-2-nine-node-3.3-pairs-019.md | NotificationWebhookContract | API_CONTRACT alone | PASS |
| layer-2-nine-node-3.3-pairs-020.md | PartnerApiContract | API_CONTRACT+SECURITY | PASS |
| layer-2-nine-node-3.3-pairs-021.md | ApiVersioningStrategy | API_CONTRACT+ARCHITECTURE | PASS |
| layer-2-nine-node-3.3-pairs-022.md | FileUploadApiContract | API_CONTRACT+IMPLEMENT | PASS |
| layer-2-nine-node-3.3-pairs-023.md | ProbationaryRejected | API_CONTRACT+TEST rejected | FAIL→PASS |
| layer-2-nine-node-3.3-pairs-024.md | complete nine-node rule summary | reference | N/A |

#### Layer 3 — Teaching Unit Pairs (23 files)
| File | Rule | Teaching point |
|------|------|----------------|
| layer-3-teaching-unit-pairs-002.md | B12-R01 | contract always fires API_CONTRACT |
| layer-3-teaching-unit-pairs-003.md | B12-R02 | schema needs API co-occurrence |
| layer-3-teaching-unit-pairs-004.md | B12-R03 | interface fires both |
| layer-3-teaching-unit-pairs-005.md | B12-R06 | authentication always SECURITY |
| layer-3-teaching-unit-pairs-006.md | phase map | Phase 3 sub-phase ownership |
| layer-3-teaching-unit-pairs-007.md | TransitionType | feature_addition vs documentation |
| layer-3-teaching-unit-pairs-008.md | B12-R23 | DecisionCount = predicate count |
| layer-3-teaching-unit-pairs-009.md | B12-R05 | design alone vs design + API |
| layer-3-teaching-unit-pairs-010.md | v6 | three v6 corruption patterns |
| layer-3-teaching-unit-pairs-011.md | vocab | complete API_CONTRACT vocabulary |
| layer-3-teaching-unit-pairs-012.md | summary | complete B12 teaching summary |
| layer-3-teaching-unit-pairs-013.md | B12-RULE-WEBHOOK-DOUBLE | webhook contract double trigger |
| layer-3-teaching-unit-pairs-014.md | B12-RULE-SCHEMA-GATE | database schema vs API schema |
| layer-3-teaching-unit-pairs-015.md | B12-RULE-IMPLEMENT-VERBS | IMPLEMENT verb exhaustive list |
| layer-3-teaching-unit-pairs-016.md | B12-RULE-SPEC | "spec" fires DOCUMENT not API_CONTRACT |
| layer-3-teaching-unit-pairs-017.md | B12-RULE-DESIGN-DUAL-FIRE | "design" + API term fires both ARCHITECTURE and API_CONTRACT |
| layer-3-teaching-unit-pairs-018.md | B12-RULE-IDEMPOTENT | "idempotent" fires API_CONTRACT |
| layer-3-teaching-unit-pairs-019.md | B12-RULE-INTEGRATION-CO-OCCURRENCE | "integration" requires API co-occurrence |
| layer-3-teaching-unit-pairs-020.md | B12-RULE-ROUTE | "route" fires API_CONTRACT (domain-independent) |
| layer-3-teaching-unit-pairs-021.md | B12-RULE-PUBLISH | "publish" not IMPLEMENT |
| layer-3-teaching-unit-pairs-022.md | B12-RULE-UPDATE | "update" vs "implement" TransitionType |
| layer-3-teaching-unit-pairs-023.md | B12-RULE-DUAL-PATTERN | IMPLEMENT+API_CONTRACT pattern summary |
| layer-3-teaching-unit-pairs-024.md | summary | complete B12 extended teaching summary |

#### Held-Out and Corruption Pairs (23 files)
| File | HO type | Corruption type |
|------|---------|-----------------|
| held-out-and-corruption-pairs-002.md | service contract → API_CONTRACT | v6-01 suppression |
| held-out-and-corruption-pairs-003.md | event schema + API context | schema over-firing |
| held-out-and-corruption-pairs-004.md | interface dual-fire | ARCHITECTURE suppressed |
| held-out-and-corruption-pairs-005.md | authentication in API docs | v6-02 (SECURITY suppressed) |
| held-out-and-corruption-pairs-006.md | documentation TransitionType | feature_addition without IMPLEMENT |
| held-out-and-corruption-pairs-007.md | gRPC → API_CONTRACT | gRPC → IMPLEMENT (wrong) |
| held-out-and-corruption-pairs-008.md | webhook + document | webhook → IMPLEMENT (wrong) |
| held-out-and-corruption-pairs-009.md | versioning → API_CONTRACT | versioning → ARCHITECTURE (wrong) |
| held-out-and-corruption-pairs-010.md | data contract → API_CONTRACT | v6-01 (data suppresses) |
| held-out-and-corruption-pairs-011.md | three v6 primary tests | v6-01 + v6-02 + v6-03 |
| held-out-and-corruption-pairs-012.md | four mixed held-out | multiple corruption types |
| held-out-and-corruption-pairs-013.md | order webhook contract double trigger | v6-01 domain suppress + v6-03 DecisionCount |
| held-out-and-corruption-pairs-014.md | inventory API endpoints | v6-01 domain suppress + database schema over-fire |
| held-out-and-corruption-pairs-015.md | user management API interface | v6-01 interface→ARCHITECTURE + v6-02 auth suppress |
| held-out-and-corruption-pairs-016.md | swagger shipping rates | v6-01 publish→IMPLEMENT + v6-03 DecisionCount |
| held-out-and-corruption-pairs-017.md | design GraphQL API dual-fire | v6-01 ARCHITECTURE suppress + v6-03 DecisionCount |
| held-out-and-corruption-pairs-018.md | spec fires DOCUMENT not API_CONTRACT | v6-01 spec→API_CONTRACT + v6-01 write→IMPLEMENT suppress |
| held-out-and-corruption-pairs-019.md | gRPC service contract | v6-01 gRPC unrecognised + service contract modifier |
| held-out-and-corruption-pairs-020.md | multipart upload auth | v6-02 SECURITY suppress + v6-03 DecisionCount |
| held-out-and-corruption-pairs-021.md | partner API integration contract | v6-01 integration suppress + publish→IMPLEMENT |
| held-out-and-corruption-pairs-022.md | update not IMPLEMENT versioning | v6-01 update→IMPLEMENT + design ARCHITECTURE suppress |
| held-out-and-corruption-pairs-023.md | IMPLEMENT+API_CONTRACT dual | v6-01 IMPLEMENT suppress + v6-03 FlowOrder wrong |
| held-out-and-corruption-pairs-024.md | five-predicate stress test | v6-01 ARCHITECTURE suppress + v6 summary reference |

#### Cross-Layer Synthesis (8 files)
| File | Predicates | TransitionType |
|------|------------|----------------|
| cross-layer-synthesis-001.md | API_CONTRACT+DOCUMENT+SECURITY | documentation |
| cross-layer-synthesis-002.md | API_CONTRACT+DOCUMENT | documentation |
| cross-layer-synthesis-003.md | ARCHITECTURE+API_CONTRACT+DOCUMENT | documentation |
| cross-layer-synthesis-004.md | IMPLEMENT+API_CONTRACT+DOCUMENT+SECURITY+AUTHORIZATION | feature_addition |
| cross-layer-synthesis-005.md | API_CONTRACT+IMPLEMENT+DOCUMENT | feature_addition |
| cross-layer-synthesis-006.md | ARCHITECTURE+DOCUMENT (database schema — no API_CONTRACT) | documentation |
| cross-layer-synthesis-007.md | API_CONTRACT+SECURITY+DOCUMENT | documentation |
| cross-layer-synthesis-008.md | ARCHITECTURE+API_CONTRACT+SECURITY+IMPLEMENT+DOCUMENT | feature_addition |

## File count

| Layer | Files | Description |
|-------|-------|-------------|
| Original seed (wave-1) | 8 | Initial B12 pairs |
| Layer 0-A (002-024) | 23 | Compiler pairs |
| Layer 0-B (002-024) | 23 | Router pairs |
| Layer 0-C (002-024) | 23 | Knowledge edge pairs |
| Layer 0-D (002-024) | 23 | Flow factory pairs |
| Layer 1 (002-024) | 23 | Transition orchestrator pairs |
| Layer 2 (002-024) | 23 | Nine-node 3.3 pairs |
| Layer 3 (002-024) | 23 | Teaching unit pairs |
| Held-out/Corruption (002-024) | 23 | Held-out and corruption pairs |
| Cross-layer synthesis (001-008) | 8 | Full layer traces |
| MANIFEST.md | 1 | This file |
| **Total new files** | **192** | |
| **Total batch files** | **201** | (192 new + 8 original + 1 manifest) |

## Layer 2 nine-node results table

| Flow | Predicates | Files | Gate B |
|------|------------|-------|--------|
| B12-batch-001 original (nine-node) | API_CONTRACT+DOCUMENT+SECURITY | 3 | PASS |
| PaymentGatewayOpenApiSpec | IMPLEMENT+API_CONTRACT+DOCUMENT | 4 | PASS |
| ProductCatalogGraphQLSchema | API_CONTRACT+ARCHITECTURE+DOCUMENT | 3 | PASS |
| UserServiceGrpcContract | API_CONTRACT+DOCUMENT | 3 | PASS |
| NotificationWebhookContract | API_CONTRACT+DOCUMENT | 2 | PASS |
| InventoryRestEndpoints | API_CONTRACT+DOCUMENT+SECURITY | 4 | PASS |
| AdminPanelApiRoutes | API_CONTRACT+DOCUMENT+AUTHORIZATION | 4 | PASS |
| CartApiInterface | ARCHITECTURE+API_CONTRACT+DOCUMENT | 4 | PASS |
| AuthServiceApiContract | IMPLEMENT+API_CONTRACT+DOCUMENT+SECURITY+AUTHORIZATION | 6 | PASS |
| EventBusApiContract | API_CONTRACT+DOCUMENT (fallback) | 2 | FALLBACK→PASS |
| CustomerAccountApiVersioning | API_CONTRACT+DOCUMENT | 4 | PASS |
| DataContractsDefinition | API_CONTRACT+DOCUMENT+ARCHITECTURE | 5 | PASS |
| OrderWebhookContract | API_CONTRACT alone | 2 | PASS |
| InventoryApiSchema | API_CONTRACT+DOCUMENT | 3 | PASS |
| UserManagementApiContract | API_CONTRACT alone | 2 | PASS |
| ShippingRatesRestApi | API_CONTRACT+IMPLEMENT | 3 | PASS |
| ProductSearchGraphQL | API_CONTRACT alone | 2 | PASS |
| ReportingApiSpec | API_CONTRACT+DOCUMENT | 3 | PASS |
| NotificationWebhookContract (013) | API_CONTRACT alone | 2 | PASS |
| PartnerApiContract | API_CONTRACT+SECURITY | 3 | PASS |
| ApiVersioningStrategy | API_CONTRACT+ARCHITECTURE | 3 | PASS |
| FileUploadApiContract | API_CONTRACT+IMPLEMENT | 3 | PASS |
| ProbationaryRejected | API_CONTRACT+TEST → fallback | 2 | FAIL→PASS |
| NineNodeRuleSummary | reference | 0 | N/A |

All 25 flows: PASS (including 2 FALLBACK cases that used deterministic_generic successfully)

## 12 Key Teaching Points for B12

1. "contract" fires API_CONTRACT regardless of modifier (data contracts, service contracts, message contracts, event contracts, webhook contracts — all fire via "contract" term)

2. "schema" fires API_CONTRACT ONLY with API co-occurrence (api/rest/graphql/openapi/swagger/grpc/webhook); "database schema" → DOCUMENT only

3. "interface" fires BOTH API_CONTRACT (API context) AND ARCHITECTURE (system design context) when both signals are present

4. "authentication" fires SECURITY in ANY context — even in API documentation, OpenAPI spec context, webhook specification context (Core Rule 6: unconditional)

5. "authorization" / "access control" / "oauth" → AUTHORIZATION → routes to 3.4; "oauth" also fires SECURITY simultaneously

6. TransitionType: feature_addition when IMPLEMENT fires; documentation otherwise. "integrate" → API_CONTRACT (NOT IMPLEMENT) per Core Rule 17

7. DecisionCount = COUNT(predicates fired) — NOT count of unique sub-phases. SECURITY + AUTHORIZATION both routing to 3.4 = two separate decisions (Core Rule 23)

8. Phase 3 sub-phases: 3.1 ARCHITECTURE, 3.2 ARCH+IMPL, 3.3 API_CONTRACT (B12 primary), 3.4 SECURITY+AUTHORIZATION, 3.5 infra-ARCHITECTURE; DOCUMENT routes to Phase 2.1

9. "design" alone → ARCHITECTURE. "design the API" → ARCHITECTURE + API_CONTRACT both fire (design vocabulary + API vocabulary = both predicates, neither suppresses the other)

10. Three v6 corruptions in B12: v6-01 (contract modifier suppresses API_CONTRACT), v6-02 (authentication in API doc suppresses SECURITY), v6-03 (multiple routing decisions collapsed to single 3.3)

11. No GapHandler in B12 (Core Rule 22). DomainId = "software-development" for all B12 pairs (Core Rule 24)

12. FlowOrder: ARCHITECTURE first → IMPLEMENT/API_CONTRACT parallel → DOCUMENT/SECURITY/AUTH parallel → QA always last. Documentation before security is a v6 ordering violation.

## 4 Cross-Layer Synthesis Traces

### Trace 1: REST API contract documentation with authentication
Request: "Document the REST API endpoints for the order service including authentication requirements."
Layer 0-A: [API_CONTRACT, DOCUMENT, SECURITY]
Layer 0-B: 3.3 + 2.1 + 3.4
Layer 0-C: authentication → SECURITY always (v6-02 rule exercised)
Layer 0-D: deterministic_generic → [APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34]
Layer 1: APIContractFlow33 sequential → parallel(DocumentationFlow21, SecurityAuditFlow34) → QA
Layer 2: order-service-openapi.yaml + order-api-reference.md + auth-security-notes.md
TransitionType: documentation

### Trace 2: Webhook payload contract specification
Request: "Specify the webhook payload contract for order status events."
Layer 0-A: [API_CONTRACT, DOCUMENT] ("payload contract" → "contract" → API_CONTRACT via Rule 2)
Layer 0-B: 3.3 + 2.1
Layer 0-C: "payload" modifier on "contract" — Rule 2 exercised (modifier irrelevant)
Layer 0-D: trusted_learned → [APIContractFlow33-learned-v1, DocumentationFlow21]
Layer 1: APIContractFlow33 → DocumentationFlow21 → QA (sequential)
Layer 2: webhook-payload-contract.json + webhook-integration-guide.md
TransitionType: documentation

### Trace 3: Service interface design and API contract
Request: "Design the service interface for the event bus and specify the API contract."
Layer 0-A: [ARCHITECTURE, API_CONTRACT, DOCUMENT] ("interface" fires both via dual-context)
Layer 0-B: 3.1 + 3.3 + 2.1
Layer 0-C: "interface" dual-fire rule exercised (Rule B12-R03)
Layer 0-D: probationary_accepted → [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21]
Layer 1: ArchitectureFlow31 → APIContractFlow33 → DocumentationFlow21 → QA (full sequential)
Layer 2: event-bus-component-map.md + event-bus-api-contract.yaml + event-bus-developer-guide.md
TransitionType: documentation

### Trace 4: Full OAuth API cycle (all five predicates)
Request: "Write the OpenAPI specification for the authentication service, implement the OAuth 2.0 token endpoint, and document the security requirements."
Layer 0-A: [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]
Layer 0-B: 4.3 + 3.3 + 2.1 + 3.4 + 3.4 (five decisions, DecisionCount = 5)
Layer 0-C: oauth fires BOTH SECURITY + AUTHORIZATION; authentication fires SECURITY; five predicates → five routes
Layer 0-D: deterministic_generic → [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34]
Layer 1: parallel(ImplementationFlow43, APIContractFlow33) → parallel(DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34) → QA
Layer 2: auth-service-openapi.yaml + oauth-token-endpoint.cs + oauth-integration-guide.md + token-security-requirements.md + oauth-scopes-reference.md + oauth-flow-diagrams.md
TransitionType: feature_addition (IMPLEMENT fires)

### Trace 5: IMPLEMENT + API_CONTRACT dual-fire (inventory reservation)
Request: "Build the REST API endpoints for the inventory reservation service and document the request/response contract."
Layer 0-A: [API_CONTRACT, IMPLEMENT, DOCUMENT] ("build" → IMPLEMENT; "REST"+"api"+"endpoints"+"contract" → API_CONTRACT)
Layer 0-B: 3.3 + 3.2 + 2.1 (three decisions, DecisionCount = 3)
Layer 0-C: "build" = IMPLEMENT unconditional; "document" clause does NOT suppress IMPLEMENT; feature_addition TransitionType
Layer 0-D: trusted_learned → [ImplementFlow32 ∥ APIContractFlow33, DocumentationFlow21, QAFlow]
Layer 1: parallel(ImplementFlow32, APIContractFlow33) → DocumentationFlow21 → QAFlow
Layer 2: inventory-reservation-api.yaml + endpoint-stubs/ + api-reference.md
TransitionType: feature_addition (IMPLEMENT present → code scaffolds generated)

### Trace 6: Database schema design → ARCHITECTURE only (no API_CONTRACT)
Request: "Design the database schema for the inventory service tables."
Layer 0-A: [ARCHITECTURE, DOCUMENT] ("design" → ARCHITECTURE; "database schema" → schema gate FAILS — no API co-occurrence)
Layer 0-B: 3.1 + 2.1 (two decisions, DecisionCount = 2)
Layer 0-C: "database" is not an API protocol term; schema without API co-occurrence → API_CONTRACT blocked (Core Rule 5)
Layer 0-D: trusted_learned → [ArchitectureFlow31-learned-v1, DocumentationFlow21]
Layer 1: ArchitectureFlow31 → DocumentationFlow21 (sequential)
Layer 2: inventory-schema.sql + inventory-data-dictionary.md
TransitionType: documentation (no IMPLEMENT; no API_CONTRACT — pure data-layer architecture)

### Trace 7: OAuth2 authentication contract — API_CONTRACT + SECURITY + DOCUMENT
Request: "Specify the OAuth2 authentication contract for the partner API gateway endpoints."
Layer 0-A: [API_CONTRACT, SECURITY, DOCUMENT] ("OAuth2"+"authentication" → SECURITY unconditional; "api"+"contract"+"endpoints" → API_CONTRACT; "specify" → DOCUMENT)
Layer 0-B: 3.3 + 3.4 + 2.1 (three decisions, DecisionCount = 3)
Layer 0-C: SECURITY fires even inside API documentation context (v6-02 risk); "authentication" cannot be demoted to API term
Layer 0-D: trusted_learned → [APIContractFlow33 ∥ SecurityAuthorizationFlow34, DocumentationFlow21, QAFlow]
Layer 1: parallel(APIContractFlow33, SecurityAuthorizationFlow34) → DocumentationFlow21 → QAFlow
Layer 2: partner-gateway-oauth2-contract.yaml + oauth2-security-scheme.yaml + oauth2-integration-guide.md
TransitionType: documentation (no IMPLEMENT; SECURITY + API_CONTRACT both fire but no construction verb)

### Trace 8: Five-predicate payment gateway (ARCHITECTURE + API_CONTRACT + SECURITY + IMPLEMENT + DOCUMENT)
Request: "Design and implement a secure REST API contract for the payment gateway, including authentication, OAuth2 endpoints, and write the OpenAPI specification."
Layer 0-A: [ARCHITECTURE, API_CONTRACT, SECURITY, IMPLEMENT, DOCUMENT] (all five predicates from five distinct trigger sets)
Layer 0-B: 3.1 + 3.3 + 3.4 + 3.2 + 2.1 (five decisions, DecisionCount = 5)
Layer 0-C: ARCHITECTURE (tier 0) must execute before all others; IMPLEMENT + API_CONTRACT + SECURITY parallel at tier 1; QA always last
Layer 0-D: trusted_learned → [ArchitectureFlow31] → [ImplementFlow32 ∥ APIContractFlow33 ∥ SecurityAuthorizationFlow34] → [DocumentationFlow21] → [QAFlow]
Layer 1: ArchitectureFlow31 → parallel(ImplementFlow32, APIContractFlow33, SecurityAuthorizationFlow34) → DocumentationFlow21 → QAFlow
Layer 2: payment-gateway-api.yaml + payment-gateway-architecture.md + endpoint-stubs/ + oauth2-security-policy.md + developer-integration-guide.md
TransitionType: feature_addition (IMPLEMENT present; maximum-complexity B12 pattern)
