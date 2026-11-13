# Layer 2 — Nine-Node 3.4 Pairs (008)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: nine_node
# Scenario: TenantGrantRevoke — AUTHORIZATION+API_CONTRACT — tenant grant/revoke API contract
# Flow: SecurityVerification34Flow | State: trusted_learned

---

## O1b-2-008-N01 — Node 01: DomainSplitterUnit

A: SelectedLogicalTransition: { predicates: ["AUTHORIZATION", "API_CONTRACT"], transition_type: "feature_addition", user_intent: "define the tenant grant/revoke API contract for scope management" }

B: DomainPacket[]: [{ domain: "AUTHORIZATION", fragment: "grant scope, revoke scope, scope lifecycle management per tenant" }, { domain: "API_CONTRACT", fragment: "REST endpoints for grant/revoke operations, request/response schemas" }]
   Unit: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>

---

## O1b-2-008-N02 — Node 02: DomainTranslatorUnit

A: DomainPacket: { domain: "API_CONTRACT", fragment: "REST endpoints for grant/revoke" }

B: TranslatedDomainStatement: { domain: "API_CONTRACT", statement: "Endpoints: POST /tenants/{tenant_id}/scopes/grant (body: { scope: string }) → 200 OK or 409 Conflict (already granted). DELETE /tenants/{tenant_id}/scopes/{scope} → 204 No Content or 404 Not Found. Both endpoints require tenant admin authorization header. Idempotent: granting an already-granted scope returns 200, not error." }

---

## O1b-2-008-N03 — Node 03: SchemePlannerUnit

A: TranslatedDomainStatement[]: [AUTHORIZATION: scope lifecycle, API_CONTRACT: grant/revoke endpoints]

B: WorkBranch[]: [{ branch_id: "WB-01", focus: "GrantScopeEndpoint" }, { branch_id: "WB-02", focus: "RevokeScopeEndpoint" }, { branch_id: "WB-03", focus: "ScopeValidation" }]

---

## O1b-2-008-N04 — Node 04: DomainProcessorUnit

A: WorkBranch: { branch_id: "WB-01", focus: "GrantScopeEndpoint" }

B: ProcessedBranchResult: { branch_id: "WB-01", decisions: [{ decision: "POST /tenants/{id}/scopes/grant: validate scope is in allowed global scope list; add to tenant's scope set; idempotent (already present → 200); return updated scope list" }] }

---

## O1b-2-008-N05 — Node 05: ApplierUnit

A: ProcessedBranchResult: { branch_id: "WB-01", decisions: ["POST /tenants/{id}/scopes/grant"] }

B: AppliedBranchResult: { branch_id: "WB-01", applied: { component: "TenantScopeController.GrantScope(tenant_id, scope)", spec: "POST /tenants/{tenant_id}/scopes/grant. Validates scope against IGlobalScopeRegistry. Calls ITenantScopeRepository.AddScope(). Returns OperationResult<TenantScopeResponse>." } }

---

## O1b-2-008-N06 — Node 06: ResultTranslatorUnit

A: AppliedBranchResult[]: [GrantScopeEndpoint, RevokeScopeEndpoint, ScopeValidation]

B: TranslatedResultFragment[]: [{ summary: "POST /tenants/{id}/scopes/grant: idempotent scope grant, validated against global scope list" }, { summary: "DELETE /tenants/{id}/scopes/{scope}: scope revoke with 204/404 response, triggers token invalidation" }, { summary: "ScopeValidation: only scopes in IGlobalScopeRegistry can be granted to any tenant" }]

---

## O1b-2-008-N07 — Node 07: ArbiterSelectorUnit

A: TranslatedResultFragment[]: [grant, revoke, validation]

B: ArbiterReviewRequest[]: [{ arbiter: "AuthorizationArbiter", focus: "Token invalidation on revoke, idempotency correctness" }, { arbiter: "ApiContractArbiter", focus: "HTTP status codes, response schemas" }]

---

## O1b-2-008-N08 — Node 08: ArbiterUnit

A: ArbiterReviewRequest: { arbiter: "ApiContractArbiter", focus: "HTTP status codes" }

B: ArbiterVerdict: { verdict: "approved", findings: ["409 Conflict for grant of already-granted scope is acceptable; 200 idempotent also acceptable — choose one consistently", "DELETE returning 204 (no content) is correct REST pattern for revoke"], gate_b: "PASSED — API contract follows REST conventions" }

---

## O1b-2-008-N09 — Node 09: FormatterUnit

A: ReviewedResultSet: { all verdicts: approved }

B: HumanResult: "TenantGrantRevoke API: POST /tenants/{id}/scopes/grant (idempotent, 200/409), DELETE /tenants/{id}/scopes/{scope} (204/404). Scope revoke triggers token invalidation. IGlobalScopeRegistry validates grantable scopes. TenantScopeController produced."
   Unit: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
