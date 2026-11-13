# Layer 0-C — Knowledge Edge Pairs (007)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-007-001 — Phase 3 sub-phase map: 3.1/3.2/3.3/3.4/3.5 ownership table

```
source_id: xiigen-b12-0c-007-001
rule_id: B12-RULE-PHASE3-SUBPHASE-MAP
pair_kind: knowledge_edge
```

### Phase 3 Sub-Phase Ownership Table

| Sub-phase | Owner predicate(s) | Name | Focus |
|-----------|-------------------|------|-------|
| 3.1 | ARCHITECTURE | Architecture (system) | System-level design, component relationships, service boundaries |
| 3.2 | ARCHITECTURE + IMPLEMENT | Architecture + Implementation | Component design with simultaneous implementation |
| 3.3 | API_CONTRACT | API Contract Design | API specifications, endpoint contracts, schemas, webhooks, gRPC contracts |
| 3.4 | SECURITY + AUTHORIZATION | Security + Authorization | Authentication, tokens, OAuth, access control, permission scopes |
| 3.5 | ARCHITECTURE (infra) | Infrastructure Architecture | Cloud, deployment, infrastructure, platform design |

### B12 primary sub-phase

B12 primary sub-phase: 3.3 (API Contract Design)
DomainId: "software-development"
Primary predicate: API_CONTRACT
Primary flow: APIContractFlow33

### Sub-phase trigger vocabulary

3.1 triggers: "design" + system vocabulary (no API terms), "architecture" alone
3.2 triggers: "design" + "implement/build/create" in same request
3.3 triggers: api, contract, endpoint, route, interface (API context), webhook, grpc, graphql, openapi, swagger, rest, schema (with API co-occurrence), versioning/version (API context)
3.4 triggers: authentication, token, oauth, authorization, access control
3.5 triggers: "infrastructure", "cloud", "deployment", "kubernetes", "terraform" + "architecture"/"design"

---

## B12-0C-007-002 — Sub-phase routing: which predicates go where

```
source_id: xiigen-b12-0c-007-002
rule_id: B12-RULE-PHASE3-SUBPHASE-MAP
pair_kind: knowledge_edge
```

Predicate → Sub-phase mapping:

ARCHITECTURE → 3.1 (and potentially 3.5 for infra context)
IMPLEMENT alone → 4.3 (not Phase 3 — IMPLEMENT goes to Phase 4)
ARCHITECTURE + IMPLEMENT → 3.2 (when both appear in same request)
API_CONTRACT → 3.3
SECURITY → 3.4
AUTHORIZATION → 3.4
DOCUMENT → 2.x (Phase 2, not Phase 3)

Important: DOCUMENT routes to Phase 2, NOT Phase 3. Even when paired with API_CONTRACT (which routes
to Phase 3.3), the DOCUMENT predicate itself routes to Phase 2.1.

---

## B12-0C-007-003 — Phase 3 sub-phase disambiguation: 3.1 vs 3.3 vs 3.5

```
source_id: xiigen-b12-0c-007-003
rule_id: B12-RULE-PHASE3-SUBPHASE-MAP
pair_kind: knowledge_edge
```

3.1 vs 3.3 disambiguation:

3.1 (ARCHITECTURE): System-level design decisions
  Signals: "design", "architecture", "system", "component", "service boundary"
  Example: "Design the microservice architecture for the platform" → 3.1

3.3 (API_CONTRACT): API interface and contract documentation
  Signals: api, contract, endpoint, webhook, grpc, graphql, openapi, swagger, rest
  Example: "Document the REST API contracts for the payment service" → 3.3

When both appear: both 3.1 AND 3.3 routing decisions are generated
  Example: "Design the REST API architecture" → both 3.1 (design) and 3.3 (REST API)

3.5 (ARCHITECTURE, infra):
  Signals: "cloud", "infrastructure", "deployment", "kubernetes", "terraform", "CI/CD"
  These signals shift ARCHITECTURE routing from 3.1 to 3.5
  Example: "Design the Kubernetes deployment architecture" → 3.5

---

## B12-0C-007-004 — B12 sub-phase 3.3: what APIContractFlow33 covers

```
source_id: xiigen-b12-0c-007-004
rule_id: B12-RULE-PHASE3-SUBPHASE-MAP
pair_kind: knowledge_edge
```

APIContractFlow33 handles:

  REST API contracts:
    - Endpoint definitions, HTTP method mappings
    - Request/response schemas
    - OpenAPI/Swagger specifications
    - URL route patterns

  GraphQL contracts:
    - Schema definitions (types, queries, mutations, subscriptions)
    - Resolver interfaces
    - GraphQL schema files

  gRPC contracts:
    - .proto file definitions
    - Service contract documents
    - Protobuf message schemas

  Webhook contracts:
    - Payload contract definitions
    - Event schema specifications
    - Delivery guarantee documentation

  API versioning:
    - Versioning strategy documentation
    - Version-specific endpoint contracts
    - Breaking change documentation

  Data/service contracts:
    - Inter-service data contracts
    - Message contracts
    - Event contracts
    - Integration contracts

All of the above map to Phase 3.3 via the API_CONTRACT predicate.
APIContractFlow33 is the single flow that handles all API contract documentation work.
