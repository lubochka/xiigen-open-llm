# Layer 2 — Nine-Node 3.4 Pairs (003)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: nine_node
# Scenario: TenantScopeEnforcement — SECURITY+AUTHORIZATION — add per-tenant scope validation
# Flow: SecurityVerification34Flow | State: trusted_learned

---

## O1b-2-003-N01 — Node 01: DomainSplitterUnit

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION"], transition_type: "feature_addition", user_intent: "add per-tenant scope validation middleware to the API gateway" }

B: DomainPacket[]: [{ domain: "SECURITY", fragment: "bearer token validation, JWT claim extraction" }, { domain: "AUTHORIZATION", fragment: "per-tenant scope enforcement, access decision" }]
   Unit: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>

---

## O1b-2-003-N02 — Node 02: DomainTranslatorUnit

A: DomainPacket: { domain: "AUTHORIZATION", fragment: "per-tenant scope enforcement" }

B: TranslatedDomainStatement: { domain: "AUTHORIZATION", statement: "For every API request, extract scope claim from bearer JWT; resolve tenant's allowed scope set via ITenantScopeResolver; verify requested operation's required scope is within tenant's allowed set; return 403 (Forbidden) if scope not permitted." }

---

## O1b-2-003-N03 — Node 03: SchemePlannerUnit

A: TranslatedDomainStatement[]: [SECURITY: JWT validation, AUTHORIZATION: per-tenant scope enforcement]

B: WorkBranch[]: [{ branch_id: "WB-01", focus: "JWTValidation", units: ["JWTDecoder", "SignatureVerifier"] }, { branch_id: "WB-02", focus: "TenantScopeCheck", units: ["TenantResolver", "ScopeValidator", "AccessDecider"] }]

---

## O1b-2-003-N04 — Node 04: DomainProcessorUnit

A: WorkBranch: { branch_id: "WB-02", focus: "TenantScopeCheck" }

B: ProcessedBranchResult: { branch_id: "WB-02", decisions: [{ unit: "TenantResolver", decision: "Extract tenant_id from JWT 'aud' claim" }, { unit: "ScopeValidator", decision: "Load tenant allowed scopes from ITenantScopeRepository; check if operation_scope ∈ allowed_scopes" }, { unit: "AccessDecider", decision: "Return Allow if scope present; return Deny(403) if not" }] }

---

## O1b-2-003-N05 — Node 05: ApplierUnit

A: ProcessedBranchResult: { branch_id: "WB-02", decisions: ["TenantResolver", "ScopeValidator", "AccessDecider"] }

B: AppliedBranchResult: { branch_id: "WB-02", applied: { component: "TenantScopeEnforcerMiddleware", spec: "IMiddleware: extracts tenant_id from JWT aud claim, loads ITenantScopeRepository.GetAllowedScopes(tenant_id), verifies required_scope ∈ allowed_scopes, returns OperationResult<AccessDecision>" } }

---

## O1b-2-003-N06 — Node 06: ResultTranslatorUnit

A: AppliedBranchResult[]: [WB-01: JWT validation, WB-02: TenantScopeEnforcer]

B: TranslatedResultFragment[]: [{ summary: "JWTDecoder: decode and verify HS256 signature, return Claims" }, { summary: "TenantScopeEnforcerMiddleware: tenant-scoped access decision on every API request" }]

---

## O1b-2-003-N07 — Node 07: ArbiterSelectorUnit

A: TranslatedResultFragment[]: [JWTDecoder, TenantScopeEnforcerMiddleware]

B: ArbiterReviewRequest[]: [{ arbiter: "SecurityArbiter", focus: "JWT signature verification, token expiry check" }, { arbiter: "AuthorizationArbiter", focus: "Tenant isolation — one tenant cannot access another's allowed scope set" }]

---

## O1b-2-003-N08 — Node 08: ArbiterUnit

A: ArbiterReviewRequest: { arbiter: "AuthorizationArbiter", focus: "Tenant isolation" }

B: ArbiterVerdict: { verdict: "approved", findings: ["ITenantScopeRepository must be keyed by tenant_id with no cross-tenant reads", "ScopeValidator must not cache across tenants"], gate_b: "PASSED — tenant isolation enforced at data access layer" }

---

## O1b-2-003-N09 — Node 09: FormatterUnit

A: ReviewedResultSet: { all verdicts: approved }

B: HumanResult: "TenantScopeEnforcement: middleware validates JWT, extracts tenant_id from aud claim, enforces per-tenant scope policy. 403 returned on scope mismatch. Tenant isolation confirmed — no cross-tenant scope reads."
   Unit: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
