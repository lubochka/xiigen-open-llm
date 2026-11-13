# Layer 2 — Nine-Node 3.4 Pairs (004)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Sub-phase: 3.4 — Security and Authorization Model
# Flow: SecurityVerification34Flow
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## ApiKeySecuritySchema — Predicates: ["SECURITY", "API_CONTRACT"] | TransitionType: feature_addition

Scenario: Create API key authentication schema with documentation for multi-tenant services.
Source: "Create the API key authentication schema for the tenant management API and document the key lifecycle."

### Node 01 — DomainSplitter
A: SelectedLogicalTransition: ApiKeySecuritySchema | Predicates: ["SECURITY", "API_CONTRACT"]
B: DomainPacket[]: [
     DomainPacket(domain: "api-key-security", predicate: "SECURITY", priority: 1),
     DomainPacket(domain: "api-key-contract-schema", predicate: "API_CONTRACT", priority: 2)
   ]

### Node 02 — DomainTranslator
A: DomainPacket(domain: "api-key-security", predicate: "SECURITY")
B: WorkBranch(
     branchId: "api-key-security-design",
     workType: "api-key-lifecycle-security",
     securityDomain: "3.4",
     expectedOutput: "API key creation, rotation, and revocation security model"
   )

### Node 03 — SchemePlanner
A: WorkBranch[]: [api-key-security-design, api-key-contract-schema]
B: ExecutionSchema(
     steps: [
       Step(id: 1, type: "define-api-key-format", branch: "api-key-security-design"),
       Step(id: 2, type: "design-key-rotation-policy", branch: "api-key-security-design"),
       Step(id: 3, type: "define-key-schema-in-openapi", branch: "api-key-contract-schema"),
       Step(id: 4, type: "document-key-lifecycle", branch: "api-key-contract-schema")
     ],
     parallelGroups: [[1, 3], [2, 4]]
   )

### Node 04 — DomainProcessor
A: ExecutionSchema step 1: define-api-key-format
B: ProcessedDomain(
     step: "define-api-key-format",
     result: "API keys are 256-bit (bit — binary unit) cryptographically random hex strings; prefixed with tenant-id for scope isolation",
     format: "tenantId-32hexchars",
     hashStorage: "SHA-256 (Secure Hash Algorithm 256-bit) hash stored; plaintext never persisted"
   )

### Node 05 — Applier
A: ProcessedDomain(step: "define-api-key-format", status: "defined")
B: AppliedResult(
     applied: true,
     artifact: "ApiKeyGenerator and ApiKeyValidator registered in security middleware",
     tenantBinding: "key prefix enforces tenant binding at generation time"
   )

### Node 06 — ResultTranslator
A: AppliedResult[]: [key-format-result, rotation-policy-result, openapi-schema-result, lifecycle-doc-result]
B: TranslatedResult(
     summary: "API key security schema defined: 256-bit random keys with tenant prefix, SHA-256 hash storage, rotation policy, OpenAPI schema",
     securityFindings: ["key format: cryptographically random 256-bit", "storage: hash-only (SHA-256)"],
     apiContractFindings: ["OpenAPI schema: defined", "lifecycle documentation: complete"]
   )

### Node 07 — ArbiterSelector
A: TranslatedResult[]: [api-key-security-schema-result]
B: ArbiterReviewRequest(
     reviewType: "api-key-security-review",
     flowId: "ApiKeySecuritySchema",
     reviewCriteria: ["key-randomness", "hash-storage-only", "tenant-scope-binding", "contract-completeness"],
     confidence: 0.90
   )

### Node 08 — Arbiter
A: ArbiterReviewRequest(reviewType: "api-key-security-review", confidence: 0.90)
B: ArbiterDecision(
     verdict: "accepted",
     rationale: "256-bit random keys with SHA-256 hash storage meets ASVS (Application Security Verification Standard) level 2 key requirements; tenant binding enforced at generation",
     gateB: "passed",
     warnings: ["rotation policy frequency should be configurable per tenant"]
   )

### Node 09 — Formatter
A: ArbiterDecision[]: [ApiKeySecuritySchema-decision]
B: FormattedResponse(
     response: "API key security schema complete: 256-bit cryptographically random keys, tenant-prefix binding, SHA-256 hash storage only. OpenAPI contract and key lifecycle documentation generated.",
     subPhase: "3.4",
     flowId: "SecurityVerification34Flow"
   ) | FactoryState: trusted_learned
