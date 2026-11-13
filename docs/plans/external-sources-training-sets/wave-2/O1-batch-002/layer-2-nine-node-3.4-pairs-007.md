# Layer 2 — Nine-Node 3.4 Pairs (007)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: nine_node
# Scenario: OAuthClientRegistration — SECURITY+AUTHORIZATION+IMPLEMENT — implement dynamic client registration
# Flow: SecurityVerification34Flow | State: trusted_learned

---

## O1b-2-007-N01 — Node 01: DomainSplitterUnit

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION", "IMPLEMENT"], transition_type: "feature_addition", user_intent: "implement dynamic client registration per RFC 7591 (OAuth Dynamic Client Registration)" }

B: DomainPacket[]: [{ domain: "SECURITY", fragment: "client registration endpoint, client_secret generation, secure storage" }, { domain: "AUTHORIZATION", fragment: "tenant binding, initial scope assignment at registration" }, { domain: "IMPLEMENT", fragment: "build ClientRegistrationService, IClientRegistry" }]
   Unit: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>

---

## O1b-2-007-N02 — Node 02: DomainTranslatorUnit

A: DomainPacket: { domain: "SECURITY", fragment: "client registration, client_secret generation" }

B: TranslatedDomainStatement: { domain: "SECURITY", statement: "RFC 7591 dynamic client registration: POST /register with { client_name, redirect_uris, grant_types }. Generate client_id (UUID (Universally Unique Identifier)) and client_secret (cryptographically random 32 bytes). Store client_secret as BCrypt hash. Return client_id and plaintext client_secret once (never again)." }

---

## O1b-2-007-N03 — Node 03: SchemePlannerUnit

A: TranslatedDomainStatement[]: [SECURITY: RFC 7591 registration, AUTHORIZATION: tenant binding, IMPLEMENT: ClientRegistrationService]

B: WorkBranch[]: [{ branch_id: "WB-01", focus: "ClientSecretGeneration" }, { branch_id: "WB-02", focus: "TenantBinding" }, { branch_id: "WB-03", focus: "RegistrationEndpoint" }]

---

## O1b-2-007-N04 — Node 04: DomainProcessorUnit

A: WorkBranch: { branch_id: "WB-01", focus: "ClientSecretGeneration" }

B: ProcessedBranchResult: { branch_id: "WB-01", decisions: [{ decision: "Generate client_id: Guid.NewGuid().ToString()" }, { decision: "Generate client_secret: RandomNumberGenerator.GetBytes(32) → Base64Url encode" }, { decision: "Hash client_secret: BCrypt.HashPassword(secret, workFactor: 12)" }, { decision: "Store: { client_id, client_secret_hash, tenant_id, scopes }" }] }

---

## O1b-2-007-N05 — Node 05: ApplierUnit

A: ProcessedBranchResult: { branch_id: "WB-01", decisions: ["Guid client_id", "BCrypt secret", "Store hash"] }

B: AppliedBranchResult: { branch_id: "WB-01", applied: { component: "ClientRegistrationService.RegisterAsync(request)", spec: "Returns OperationResult<ClientRegistrationResponse{ client_id, client_secret }>. client_secret is returned once in plaintext; only hash stored. IClientRegistry.Store(client_id, hash, tenant_id, scopes)." } }

---

## O1b-2-007-N06 — Node 06: ResultTranslatorUnit

A: AppliedBranchResult[]: [SecretGeneration, TenantBinding, RegistrationEndpoint]

B: TranslatedResultFragment[]: [{ summary: "ClientRegistrationService: generates client_id (UUID), client_secret (32-byte random, BCrypt hash)" }, { summary: "TenantBinding: client registration requires valid tenant_id; scopes limited to tenant's allowed set" }, { summary: "POST /oauth/register: RFC 7591 compliant, returns client_id + client_secret (once)" }]

---

## O1b-2-007-N07 — Node 07: ArbiterSelectorUnit

A: TranslatedResultFragment[]: [ClientRegistration, TenantBinding, RegistrationEndpoint]

B: ArbiterReviewRequest[]: [{ arbiter: "SecurityArbiter", focus: "client_secret generation entropy, BCrypt work factor, one-time return policy" }]

---

## O1b-2-007-N08 — Node 08: ArbiterUnit

A: ArbiterReviewRequest: { arbiter: "SecurityArbiter", focus: "client_secret entropy, one-time return" }

B: ArbiterVerdict: { verdict: "approved", findings: ["RandomNumberGenerator.GetBytes(32) provides 256-bit entropy — sufficient", "BCrypt work factor 12 is acceptable (approximately 250ms on modern hardware)", "client_secret must never be stored in logs, response headers, or error messages"], gate_b: "PASSED — client secret handling is secure" }

---

## O1b-2-007-N09 — Node 09: FormatterUnit

A: ReviewedResultSet: { all verdicts: approved }

B: HumanResult: "OAuthClientRegistration: POST /oauth/register (RFC 7591). Generates UUID client_id, 256-bit client_secret (BCrypt stored, plaintext returned once). Tenant binding enforced at registration. IClientRegistry interface."
   Unit: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
