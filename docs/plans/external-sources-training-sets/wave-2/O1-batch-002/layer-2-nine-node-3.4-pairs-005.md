# Layer 2 — Nine-Node 3.4 Pairs (005)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: nine_node
# Scenario: TokenIntrospectionEndpoint — SECURITY+API_CONTRACT — build token introspection endpoint
# Flow: SecurityVerification34Flow | State: trusted_learned

---

## O1b-2-005-N01 — Node 01: DomainSplitterUnit

A: SelectedLogicalTransition: { predicates: ["SECURITY", "API_CONTRACT"], transition_type: "feature_addition", user_intent: "build a token introspection endpoint per RFC 7662 (OAuth 2.0 Token Introspection)" }

B: DomainPacket[]: [{ domain: "SECURITY", fragment: "token introspection, JWT validation, active/inactive status" }, { domain: "API_CONTRACT", fragment: "POST /oauth/introspect endpoint, request/response schema per RFC 7662" }]
   Unit: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>

---

## O1b-2-005-N02 — Node 02: DomainTranslatorUnit

A: DomainPacket: { domain: "SECURITY", fragment: "token introspection, RFC 7662" }

B: TranslatedDomainStatement: { domain: "SECURITY", statement: "RFC 7662 (OAuth Token Introspection) defines POST /introspect accepting a token parameter. Response includes: active (bool), scope, client_id, exp, iat, tenant_id. Return active:false for expired, revoked, or unknown tokens. Endpoint must be protected — only authorized resource servers may call it." }

---

## O1b-2-005-N03 — Node 03: SchemePlannerUnit

A: TranslatedDomainStatement[]: [SECURITY: RFC 7662, API_CONTRACT: /introspect endpoint]

B: WorkBranch[]: [{ branch_id: "WB-01", focus: "TokenLookup" }, { branch_id: "WB-02", focus: "IntrospectionResponse" }, { branch_id: "WB-03", focus: "EndpointSecurity" }]

---

## O1b-2-005-N04 — Node 04: DomainProcessorUnit

A: WorkBranch: { branch_id: "WB-02", focus: "IntrospectionResponse" }

B: ProcessedBranchResult: { branch_id: "WB-02", decisions: [{ decision: "Look up token in ITokenStore; if not found return { active: false }" }, { decision: "If found: check exp > now; check not in revocation list; return { active: true, scope, client_id, exp, tenant_id } if valid" }] }

---

## O1b-2-005-N05 — Node 05: ApplierUnit

A: ProcessedBranchResult: { branch_id: "WB-02", decisions: ["TokenLookup", "IntrospectionResponse"] }

B: AppliedBranchResult: { branch_id: "WB-02", applied: { component: "TokenIntrospectionService.IntrospectAsync(token)", spec: "Returns OperationResult<IntrospectionResponse>. IntrospectionResponse: { active: bool, scope: string, client_id: string, exp: long, tenant_id: string }" } }

---

## O1b-2-005-N06 — Node 06: ResultTranslatorUnit

A: AppliedBranchResult[]: [TokenLookup, IntrospectionResponse, EndpointSecurity]

B: TranslatedResultFragment[]: [{ summary: "POST /oauth/introspect: accepts token, returns IntrospectionResponse per RFC 7662" }, { summary: "TokenIntrospectionService: ITokenStore lookup, expiry check, revocation check" }, { summary: "Endpoint protection: caller must present valid resource-server client credentials" }]

---

## O1b-2-005-N07 — Node 07: ArbiterSelectorUnit

A: TranslatedResultFragment[]: [introspect endpoint, service, endpoint protection]

B: ArbiterReviewRequest[]: [{ arbiter: "SecurityArbiter", focus: "Endpoint protection, timing attack prevention in token lookup" }, { arbiter: "ApiContractArbiter", focus: "RFC 7662 compliance of response schema" }]

---

## O1b-2-005-N08 — Node 08: ArbiterUnit

A: ArbiterReviewRequest: { arbiter: "SecurityArbiter", focus: "Timing attack prevention" }

B: ArbiterVerdict: { verdict: "approved", findings: ["Use constant-time comparison for token lookup to prevent timing attacks", "Always return same response time regardless of token validity"], gate_b: "PASSED — introspection endpoint is RFC 7662 compliant and timing-safe" }

---

## O1b-2-005-N09 — Node 09: FormatterUnit

A: ReviewedResultSet: { all verdicts: approved }

B: HumanResult: "TokenIntrospectionEndpoint: POST /oauth/introspect (RFC 7662 compliant). Returns {active, scope, client_id, exp, tenant_id}. Endpoint protected by resource-server credentials. Timing-safe token lookup. ITokenStore interface."
   Unit: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
