# Layer 2 — Nine-Node 3.4 Pairs (002)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: nine_node
# Scenario: ClientCredentialsFlow — SECURITY+AUTHORIZATION+IMPLEMENT — implement OAuth2 client creds
# Flow: SecurityVerification34Flow | State: trusted_learned

---

## O1b-2-002-N01 — Node 01: DomainSplitterUnit

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION", "IMPLEMENT"], transition_type: "feature_addition", user_intent: "implement OAuth 2.0 client credentials flow for machine-to-machine authentication" }

B: DomainPacket[]: [{ domain: "SECURITY", fragment: "OAuth 2.0 client credentials grant, client_id/client_secret validation, JWT issuance" }, { domain: "AUTHORIZATION", fragment: "tenant scope assignment, service account binding" }, { domain: "IMPLEMENT", fragment: "build OAuthTokenService, ClientAuthenticator, TokenGenerator" }]
   Unit: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>

---

## O1b-2-002-N02 — Node 02: DomainTranslatorUnit

A: DomainPacket: { domain: "SECURITY", fragment: "OAuth 2.0 client credentials grant" }

B: TranslatedDomainStatement: { domain: "SECURITY", statement: "Implement RFC 6749 Section 4.4 (client credentials grant): POST /oauth/token with grant_type=client_credentials, client_id, client_secret; validate against IClientRegistry; return JWT access_token with 1-hour expiry." }

---

## O1b-2-002-N03 — Node 03: SchemePlannerUnit

A: TranslatedDomainStatement[]: [SECURITY: RFC 6749 client credentials, AUTHORIZATION: tenant scope assignment, IMPLEMENT: OAuthTokenService]

B: WorkBranch[]: [{ branch_id: "WB-01", focus: "ClientAuthentication" }, { branch_id: "WB-02", focus: "TokenGeneration" }, { branch_id: "WB-03", focus: "ScopeAssignment" }]

---

## O1b-2-002-N04 — Node 04: DomainProcessorUnit

A: WorkBranch: { branch_id: "WB-01", focus: "ClientAuthentication" }

B: ProcessedBranchResult: { branch_id: "WB-01", decisions: [{ decision: "Validate client_id exists in IClientRegistry; compare client_secret hash using BCrypt.Verify; return 401 on failure" }] }

---

## O1b-2-002-N05 — Node 05: ApplierUnit

A: ProcessedBranchResult: { branch_id: "WB-01", decisions: ["BCrypt validate credentials"] }

B: AppliedBranchResult: { branch_id: "WB-01", applied: { component: "ClientAuthenticator.ValidateAsync(client_id, client_secret)", spec: "Returns OperationResult<ClientIdentity> — Ok with ClientIdentity on success, Fail with AUTH_001 on invalid credentials" } }

---

## O1b-2-002-N06 — Node 06: ResultTranslatorUnit

A: AppliedBranchResult[]: [WB-01: ClientAuthenticator, WB-02: TokenGenerator, WB-03: ScopeAssignment]

B: TranslatedResultFragment[]: [{ summary: "ClientAuthenticator: BCrypt credential validation → OperationResult<ClientIdentity>" }, { summary: "TokenGenerator: JWT HS256, exp=3600, scope=intersected" }, { summary: "ScopeAssignment: tenant scope intersection via ITenantScopeResolver" }]

---

## O1b-2-002-N07 — Node 07: ArbiterSelectorUnit

A: TranslatedResultFragment[]: [ClientAuthenticator, TokenGenerator, ScopeAssignment]

B: ArbiterReviewRequest[]: [{ arbiter: "SecurityArbiter", focus: "BCrypt usage, JWT signing, secret storage" }, { arbiter: "AuthorizationArbiter", focus: "Scope intersection correctness, tenant isolation" }]

---

## O1b-2-002-N08 — Node 08: ArbiterUnit

A: ArbiterReviewRequest: { arbiter: "SecurityArbiter", focus: "BCrypt usage, JWT signing" }

B: ArbiterVerdict: { verdict: "approved", findings: ["BCrypt work factor 12+ required", "JWT must use HS256 or RS256 (RSA-SHA256)"], gate_b: "PASSED — client credentials flow is RFC 6749 compliant" }

---

## O1b-2-002-N09 — Node 09: FormatterUnit

A: ReviewedResultSet: { all verdicts: approved }

B: HumanResult: "ClientCredentialsFlow implementation: POST /oauth/token validates client_id+secret (BCrypt), assigns tenant-scoped JWT (HS256, 3600s), returns TokenResponse{access_token, expires_in, scope}. All security gates passed."
   Unit: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
