# Layer 2 — Nine-Node 3.4 Pairs (008)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Sub-phase: 3.4 — Security and Authorization Model
# Flow: SecurityVerification34Flow
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## ServiceAccountAuth — Predicates: ["AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition

Scenario: Service account OAuth2 (Open Authorization 2.0) scopes and API contract for tenant-to-tenant service communication.
Source: "Define service account authorization scopes for inter-service communication and the API contract for the service account management endpoint."

### Node 01 — DomainSplitter
A: SelectedLogicalTransition: ServiceAccountAuth | Predicates: ["AUTHORIZATION", "API_CONTRACT"]
B: DomainPacket[]: [
     DomainPacket(domain: "service-account-authorization", predicate: "AUTHORIZATION", priority: 1),
     DomainPacket(domain: "service-account-api-contract", predicate: "API_CONTRACT", priority: 2)
   ]

### Node 02 — DomainTranslator
A: DomainPacket(domain: "service-account-authorization", predicate: "AUTHORIZATION")
B: WorkBranch(
     branchId: "service-account-scopes",
     workType: "service-account-scope-definition",
     authorizationDomain: "3.4",
     expectedOutput: "service account scope definitions with tenant binding"
   )

### Node 03 — SchemePlanner
A: WorkBranch[]: [service-account-scopes, service-account-api-contract]
B: ExecutionSchema(
     steps: [
       Step(id: 1, type: "define-service-account-scope-set", branch: "service-account-scopes"),
       Step(id: 2, type: "bind-scopes-to-tenant-context", branch: "service-account-scopes"),
       Step(id: 3, type: "design-service-account-api-schema", branch: "service-account-api-contract"),
       Step(id: 4, type: "validate-scope-api-contract-consistency", branch: "service-account-api-contract")
     ],
     parallelGroups: [[1, 3], [2, 4]]
   )

### Node 04 — DomainProcessor
A: ExecutionSchema step 1: define-service-account-scope-set
B: ProcessedDomain(
     step: "define-service-account-scope-set",
     result: "Service account scopes: read:tenant-data, write:tenant-data, manage:users, manage:api-keys",
     scopeGranularity: "per-resource per-operation",
     tenantBinding: "each scope prefixed with tenant_id at grant time"
   )

### Node 05 — Applier
A: ProcessedDomain(step: "define-service-account-scope-set", status: "defined")
B: AppliedResult(
     applied: true,
     artifact: "ServiceAccountScopeRegistry and ServiceAccountAuthorizationMiddleware registered",
     grantMechanism: "scope grants issued as JWT claims in service-account access tokens"
   )

### Node 06 — ResultTranslator
A: AppliedResult[]: [scope-set-result, tenant-binding-result, api-schema-result, consistency-validation-result]
B: TranslatedResult(
     summary: "Service account authorization model complete: 4 scopes defined with tenant binding, API contract schema generated, scope-contract consistency validated",
     authorizationFindings: ["scope set: 4 scopes", "tenant binding: enforced", "scope-contract consistency: validated"],
     apiContractFindings: ["service account API schema: OpenAPI (Open API Specification) 3.0 compliant"]
   )

### Node 07 — ArbiterSelector
A: TranslatedResult[]: [service-account-auth-result]
B: ArbiterReviewRequest(
     reviewType: "service-account-authorization-review",
     flowId: "ServiceAccountAuth",
     reviewCriteria: ["scope-granularity", "tenant-binding-completeness", "api-contract-conformance"],
     confidence: 0.91
   )

### Node 08 — Arbiter
A: ArbiterReviewRequest(reviewType: "service-account-authorization-review", confidence: 0.91)
B: ArbiterDecision(
     verdict: "accepted",
     rationale: "Per-resource per-operation scope granularity meets least-privilege principle; tenant binding enforced at grant time; API contract schema is OpenAPI 3.0 compliant",
     gateB: "passed",
     warnings: ["add scope expiry mechanism for time-limited service account grants"]
   )

### Node 09 — Formatter
A: ArbiterDecision[]: [ServiceAccountAuth-decision]
B: FormattedResponse(
     response: "Service account authorization model complete: 4 scopes defined (read/write tenant-data, manage users/api-keys), tenant binding enforced, OpenAPI 3.0 service account API contract generated.",
     subPhase: "3.4",
     flowId: "SecurityVerification34Flow"
   ) | FactoryState: trusted_learned
