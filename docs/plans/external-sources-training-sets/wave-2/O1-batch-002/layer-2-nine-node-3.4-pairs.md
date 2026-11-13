# Layer 2 — Nine-Node 3.4 Pairs (Base)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: nine_node
# Flow: SecurityVerification34Flow | State: trusted_learned
# Scenario: Implementing OAuth2 (OAuth 2.0) client credentials with tenant scopes

---

## O1b-9N-001 — Node 01: DomainSplitterUnit

A: SelectedLogicalTransition: { user_prompt: "Implement OAuth 2.0 client credentials flow with tenant-scoped API access and service account management.", predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "IMPLEMENT"], transition_type: "feature_addition" }

B: DomainPacket[]: [{ domain: "SECURITY", fragment: "OAuth 2.0 client credentials flow" }, { domain: "AUTHORIZATION", fragment: "tenant-scoped API access and service account management" }, { domain: "API_CONTRACT", fragment: "API access endpoint contracts" }, { domain: "IMPLEMENT", fragment: "implement the flow end-to-end" }]
   Unit: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>

---

## O1b-9N-002 — Node 02: DomainTranslatorUnit

A: DomainPacket: { domain: "SECURITY", fragment: "OAuth 2.0 client credentials flow" }

B: TranslatedDomainStatement: { domain: "SECURITY", statement: "System must implement RFC 6749 (Request for Comments 6749) client credentials grant type with client_id/client_secret authentication, generating short-lived JWT (JSON Web Token) access tokens signed with HS256 (HMAC-SHA-256)." }
   Unit: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>

---

## O1b-9N-003 — Node 03: SchemePlannerUnit

A: TranslatedDomainStatement[]: [{ domain: "SECURITY", statement: "Implement RFC 6749 client credentials grant" }, { domain: "AUTHORIZATION", statement: "Enforce per-tenant scope validation on every API call" }, { domain: "API_CONTRACT", statement: "Expose /token and /introspect endpoints per OpenAPI (Open API Initiative) spec" }, { domain: "IMPLEMENT", statement: "Build OAuthTokenService with TenantScopeResolver" }]

B: WorkBranch[]: [{ branch_id: "WB-01", focus: "TokenIssuance", units: ["ClientAuthenticator", "TokenGenerator", "ScopeAssigner"] }, { branch_id: "WB-02", focus: "TenantScopeEnforcement", units: ["TenantResolver", "ScopeValidator", "AccessDecider"] }, { branch_id: "WB-03", focus: "APIContractEndpoints", units: ["TokenEndpoint", "IntrospectionEndpoint"] }]
   Unit: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>

---

## O1b-9N-004 — Node 04: DomainProcessorUnit

A: WorkBranch: { branch_id: "WB-01", focus: "TokenIssuance", units: ["ClientAuthenticator", "TokenGenerator", "ScopeAssigner"] }

B: ProcessedBranchResult: { branch_id: "WB-01", decisions: [{ unit: "ClientAuthenticator", decision: "Validate client_id+client_secret against ClientRegistry; return 401 (HTTP Unauthorized) on mismatch" }, { unit: "TokenGenerator", decision: "Issue JWT with exp=3600s, iss=auth-server, aud=tenant-id, scope=requested∩allowed" }, { unit: "ScopeAssigner", decision: "Intersect requested scopes with tenant-allowed scopes; reject if intersection is empty" }] }
   Unit: DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>

---

## O1b-9N-005 — Node 05: ApplierUnit

A: ProcessedBranchResult: { branch_id: "WB-01", decisions: [{ unit: "ClientAuthenticator", decision: "Validate credentials against registry" }, { unit: "TokenGenerator", decision: "Issue scoped JWT" }, { unit: "ScopeAssigner", decision: "Intersect scopes" }] }

B: AppliedBranchResult: { branch_id: "WB-01", applied: [{ component: "OAuthTokenService.IssueToken()", spec: "Accepts ClientCredentialsRequest{client_id, client_secret, scope[]}; validates against IClientRegistry; intersects scopes via ITenantScopeResolver; returns OperationResult<TokenResponse>" }, { component: "TokenResponse", spec: "{ access_token: string, token_type: 'Bearer', expires_in: 3600, scope: string }" }] }
   Unit: ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>

---

## O1b-9N-006 — Node 06: ResultTranslatorUnit

A: AppliedBranchResult[]: [{ branch_id: "WB-01", applied: "OAuthTokenService spec" }, { branch_id: "WB-02", applied: "TenantScopeEnforcer middleware spec" }, { branch_id: "WB-03", applied: "POST /token and GET /introspect endpoint specs" }]

B: TranslatedResultFragment[]: [{ fragment_id: "RF-01", summary: "OAuthTokenService: client credentials grant issuing tenant-scoped JWT tokens" }, { fragment_id: "RF-02", summary: "TenantScopeEnforcer: middleware validating Bearer token scope per tenant on each request" }, { fragment_id: "RF-03", summary: "Token API: POST /oauth/token (grant) and POST /oauth/introspect (token validation)" }]
   Unit: ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>

---

## O1b-9N-007 — Node 07: ArbiterSelectorUnit

A: TranslatedResultFragment[]: [{ fragment_id: "RF-01", summary: "OAuthTokenService spec" }, { fragment_id: "RF-02", summary: "TenantScopeEnforcer spec" }, { fragment_id: "RF-03", summary: "Token API spec" }]

B: ArbiterReviewRequest[]: [{ request_id: "AR-01", fragment_id: "RF-01", arbiter: "SecurityArbiter", review_focus: "Validate JWT signing algorithm, token expiry, and client secret storage security" }, { request_id: "AR-02", fragment_id: "RF-02", arbiter: "AuthorizationArbiter", review_focus: "Validate scope intersection logic and tenant isolation guarantees" }, { request_id: "AR-03", fragment_id: "RF-03", arbiter: "ApiContractArbiter", review_focus: "Validate endpoint naming, HTTP methods, and response schema against OpenAPI spec" }]
   Unit: ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>

---

## O1b-9N-008 — Node 08: ArbiterUnit

A: ArbiterReviewRequest: { request_id: "AR-01", arbiter: "SecurityArbiter", review_focus: "JWT signing, expiry, client secret storage" }

B: ArbiterVerdict: { request_id: "AR-01", verdict: "approved", findings: ["Use HS256 or RS256 (RSA-SHA256) — document algorithm choice", "client_secret must be stored as bcrypt (Bcrypt) hash, never plaintext", "Token expiry 3600s is acceptable; add refresh_token for long-lived service accounts if required"], gate_b: "PASSED — JWT spec is RFC 7519 (JSON Web Token standard) compliant and secrets are hashed" }
   Unit: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>

---

## O1b-9N-009 — Node 09: FormatterUnit

A: ReviewedResultSet: { verdicts: [{ request_id: "AR-01", verdict: "approved" }, { request_id: "AR-02", verdict: "approved" }, { request_id: "AR-03", verdict: "approved" }], fragments: ["RF-01", "RF-02", "RF-03"] }

B: HumanResult: "## OAuth 2.0 Client Credentials Implementation Plan\n\n**OAuthTokenService** — Accepts POST /oauth/token with client_id, client_secret, scope[]. Validates credentials via IClientRegistry. Intersects requested scopes with tenant-allowed scopes via ITenantScopeResolver. Issues JWT (exp=3600, HS256) on success. Returns 401 on invalid credentials, 400 on empty scope intersection.\n\n**TenantScopeEnforcer** — Middleware validating Bearer JWT on every API request. Decodes token, extracts scope claim, verifies request operation is within tenant-allowed scope set. Returns 403 (HTTP Forbidden) on scope mismatch.\n\n**API Contract** — POST /oauth/token (client credentials grant), POST /oauth/introspect (token validation). Both documented in OpenAPI 3.0 spec.\n\n**Security gates passed:** client_secret hashed (bcrypt), JWT RFC 7519 compliant, tenant isolation enforced at middleware layer."
   Unit: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
