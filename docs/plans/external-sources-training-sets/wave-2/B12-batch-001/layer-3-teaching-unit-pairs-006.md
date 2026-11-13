# Layer 3 — Teaching Unit Pairs (006)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-006 — Teaching: Phase 3 sub-phase ownership — 3.1/3.2/3.3/3.4/3.5 complete map

```
source_id: xiigen-b12-tu-006
rule_id: B12-RULE-PHASE3-SUBPHASE-MAP
pair_kind: teaching_unit
```

### Concept

Phase 3 of the development lifecycle contains five sub-phases, each owned by specific predicates.
Understanding which sub-phase a request routes to requires knowing which predicates own each sub-phase.

### Phase 3 Sub-Phase Map

Sub-phase 3.1 — Architecture (system)
  Owner: ARCHITECTURE predicate
  Triggered by: "design", "architecture" (in system design context, no specific API terms)
  Focus: component design, service topology, system-level decisions
  Example: "Design the microservice architecture for the platform" → 3.1
  Flow: ArchitectureFlow31

Sub-phase 3.2 — Architecture + Implementation (component)
  Owner: ARCHITECTURE predicate + IMPLEMENT predicate (together)
  Triggered by: "design" + "implement/build" in the same request
  Focus: designing AND implementing a component simultaneously
  Example: "Design and implement the order service component" → 3.2
  Flow: ArchitectureFlow31 + ImplementationFlow43 (combined at 3.2)
  Note: less common in B12 (B12 is documentation-focused, not 3.2 primary)

Sub-phase 3.3 — API Contract Design (B12 primary)
  Owner: API_CONTRACT predicate
  Triggered by: api, contract, endpoint, route, webhook, grpc, graphql, openapi, swagger, rest, 
                schema (with API co-occurrence), interface (in API context), versioning/version (API context)
  Focus: API specification, endpoint contracts, webhook contracts, data contracts
  Example: "Document the REST API endpoints" → 3.3
  Flow: APIContractFlow33

Sub-phase 3.4 — Security + Authorization
  Owner: SECURITY predicate + AUTHORIZATION predicate
  Triggered by: authentication/token → SECURITY; access control/authorization/oauth → AUTHORIZATION
  Focus: security requirements, authentication flows, authorization models, OAuth
  Example: "Document authentication requirements" → SECURITY → 3.4
  Example: "Specify access control for admin routes" → AUTHORIZATION → 3.4
  Flows: SecurityAuditFlow34 (SECURITY) + AuthorizationFlow34 (AUTHORIZATION)

Sub-phase 3.5 — Infrastructure Architecture
  Owner: ARCHITECTURE predicate (infrastructure context)
  Triggered by: "design/architecture" + infrastructure terms (kubernetes, terraform, cloud, deployment)
  Focus: cloud infrastructure, container orchestration, CI/CD pipeline architecture
  Example: "Design the Kubernetes deployment architecture" → 3.5
  Flow: ArchitectureFlow35 (infra variant)
  Note: not primary in B12; B12 is API contract documentation, not infrastructure

### Routing Quick Reference for B12

Most common B12 routings:
  3.3 — API_CONTRACT (every B12 request with api/contract/webhook/grpc/graphql)
  2.1 — DOCUMENT (documentation sub-phase — all B12 requests with define/specify/document)
  3.4 — SECURITY or AUTHORIZATION (when authentication/token/access control appear)
  3.1 — ARCHITECTURE (when design/architecture appear)
  4.3 — IMPLEMENT (when write/implement/build appear)

### Training Examples

A: "Document the gRPC service contract."
B: Routes to 3.3 (API_CONTRACT) + 2.1 (DOCUMENT)

A: "Design the REST API architecture."
B: Routes to 3.1 (ARCHITECTURE) + 3.3 (API_CONTRACT)

A: "Document authentication requirements for the API."
B: Routes to 3.3 (API_CONTRACT) + 2.1 (DOCUMENT) + 3.4 (SECURITY)
