# Layer 2 — Nine-Node 3.4 Pairs (011)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: nine_node
# Scenario: FullOAuthSuite — 6 predicates — complete OAuth2 tenant system
# Predicates: SECURITY+AUTHORIZATION+API_CONTRACT+IMPLEMENT+TEST+DOCUMENT
# Flows: SecurityVerification34Flow(trusted) + ApiContract33Flow(generic) + UniversalNineNodeDevelopmentFlow(generic)
#        + GenericFallbackFlow(5.1 rejected) + DocumentationFlow21(generic) | State: mixed

---

## O1b-2-011-N01 — Node 01: DomainSplitterUnit

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "IMPLEMENT", "TEST", "DOCUMENT"], transition_type: "feature_addition", user_intent: "design, implement, test, and document the complete OAuth2 tenant authorization system" }

B: DomainPacket[]: [{ domain: "SECURITY", fragment: "complete OAuth2 security design" }, { domain: "AUTHORIZATION", fragment: "tenant scope model, service account management" }, { domain: "API_CONTRACT", fragment: "token, introspect, register, grant/revoke endpoints" }, { domain: "IMPLEMENT", fragment: "build all services and middleware" }, { domain: "TEST", fragment: "unit and integration test suite" }, { domain: "DOCUMENT", fragment: "developer portal and API reference" }]
   Unit: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>

---

## O1b-2-011-N02 — Node 02: DomainTranslatorUnit

A: DomainPacket: { domain: "SECURITY", fragment: "complete OAuth2 security design" }

B: TranslatedDomainStatement: { domain: "SECURITY", statement: "Full OAuth 2.0 security model: RFC 6749 client credentials, RFC 7591 dynamic registration, RFC 7662 introspection, JWT (RFC 7519) with HS256/RS256, BCrypt secret storage, TLS on all endpoints, token revocation on scope change." }

---

## O1b-2-011-N03 — Node 03: SchemePlannerUnit

A: TranslatedDomainStatement[]: [all 6 domain statements]

B: WorkBranch[]: [{ branch_id: "WB-01", focus: "SecurityDesign" }, { branch_id: "WB-02", focus: "AuthorizationModel" }, { branch_id: "WB-03", focus: "APIContracts" }, { branch_id: "WB-04", focus: "Implementation" }, { branch_id: "WB-05", focus: "TestSuite" }, { branch_id: "WB-06", focus: "Documentation" }]

---

## O1b-2-011-N04 — Node 04: DomainProcessorUnit

A: WorkBranch: { branch_id: "WB-01", focus: "SecurityDesign" }

B: ProcessedBranchResult: { branch_id: "WB-01", decisions: [{ decision: "JWT signing: HS256 (symmetric) for single-tenant, RS256 (asymmetric) for multi-tenant key rotation" }, { decision: "client_secret: 256-bit random, BCrypt-hashed (work factor 12)" }, { decision: "TLS 1.2+ required on all auth endpoints" }, { decision: "Token expiry: access_token 1h, no refresh (client credentials do not use refresh tokens per RFC 6749)" }] }

---

## O1b-2-011-N05 — Node 05: ApplierUnit

A: ProcessedBranchResult: { branch_id: "WB-01", decisions: ["JWT signing", "BCrypt", "TLS", "token expiry"] }

B: AppliedBranchResult: { branch_id: "WB-01", applied: { component: "OAuthSecurityPolicy", spec: "ISecurityPolicy with: SigningAlgorithm (HS256|RS256), SecretHashWorkFactor: 12, TokenExpirySeconds: 3600, TlsRequired: true, RefreshTokenEnabled: false (client credentials)" } }

---

## O1b-2-011-N06 — Node 06: ResultTranslatorUnit

A: AppliedBranchResult[]: [SecurityDesign, AuthorizationModel, APIContracts, Implementation, TestSuite, Documentation]

B: TranslatedResultFragment[]: [{ summary: "SecurityDesign: JWT policy, BCrypt, TLS 1.2+" }, { summary: "AuthorizationModel: ServiceAccount + TenantScope + GrantRevoke lifecycle" }, { summary: "APIContracts: 5 endpoints (token, introspect, register, grant, revoke)" }, { summary: "Implementation: 6 services + 2 middleware + 4 interfaces" }, { summary: "TestSuite: 9 security tests + 7 authorization tests (generic fallback for 5.1)" }, { summary: "Documentation: flow diagrams + scope catalog + integration guide" }]

---

## O1b-2-011-N07 — Node 07: ArbiterSelectorUnit

A: TranslatedResultFragment[]: [all 6 fragments]

B: ArbiterReviewRequest[]: [{ arbiter: "SecurityArbiter", focus: "Complete security policy review" }, { arbiter: "AuthorizationArbiter", focus: "Tenant isolation and scope lifecycle completeness" }, { arbiter: "ApiContractArbiter", focus: "All 5 endpoint contracts" }]

---

## O1b-2-011-N08 — Node 08: ArbiterUnit

A: ArbiterReviewRequest: { arbiter: "SecurityArbiter", focus: "Complete security policy" }

B: ArbiterVerdict: { verdict: "approved", findings: ["RS256 recommended for multi-tenant (separate tenant signing keys)", "No refresh_token for client_credentials is correct per RFC 6749 Section 4.4", "All secrets BCrypt-hashed, TLS mandatory — policy complete"], gate_b: "PASSED — complete OAuth2 tenant security model is sound" }

---

## O1b-2-011-N09 — Node 09: FormatterUnit

A: ReviewedResultSet: { all verdicts: approved, flow_count: 5, predicates: 6 }

B: HumanResult: "FullOAuthSuite: Complete OAuth2 tenant authorization system. Security: JWT (HS256/RS256), BCrypt secrets, TLS 1.2+, 1h token expiry. Authorization: ServiceAccount + TenantScope + GrantRevoke. API: 5 endpoints (token, introspect, register, grant, revoke). Implementation: 6 services, 2 middleware, 4 interfaces. Tests: 16 (basic; full edge case coverage pending UnitTestingFlow51 Gate B). Documentation: flow diagrams, scope catalog, integration guide. All security gates passed."
   Unit: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
