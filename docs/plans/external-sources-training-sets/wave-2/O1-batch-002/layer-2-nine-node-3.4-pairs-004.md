# Layer 2 — Nine-Node 3.4 Pairs (004)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: nine_node
# Scenario: ServiceAccountSetup — AUTHORIZATION+API_CONTRACT — service account with scoped API access
# Flow: SecurityVerification34Flow | State: trusted_learned

---

## O1b-2-004-N01 — Node 01: DomainSplitterUnit

A: SelectedLogicalTransition: { predicates: ["AUTHORIZATION", "API_CONTRACT"], transition_type: "feature_addition", user_intent: "create service account management API with per-scope access control" }

B: DomainPacket[]: [{ domain: "AUTHORIZATION", fragment: "service account principal, scope assignment, grant/revoke lifecycle" }, { domain: "API_CONTRACT", fragment: "REST API for service account CRUD (Create, Read, Update, Delete) and scope management" }]
   Unit: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>

---

## O1b-2-004-N02 — Node 02: DomainTranslatorUnit

A: DomainPacket: { domain: "AUTHORIZATION", fragment: "service account principal, scope assignment" }

B: TranslatedDomainStatement: { domain: "AUTHORIZATION", statement: "A service account is a principal with a fixed set of scopes. Scopes are assigned at account creation and can be modified via grant/revoke operations. Each service account belongs to one tenant. Service account credentials (client_id, client_secret) are used to obtain OAuth2 tokens limited to assigned scopes." }

---

## O1b-2-004-N03 — Node 03: SchemePlannerUnit

A: TranslatedDomainStatement[]: [AUTHORIZATION: service account principal, API_CONTRACT: REST CRUD API]

B: WorkBranch[]: [{ branch_id: "WB-01", focus: "ServiceAccountModel" }, { branch_id: "WB-02", focus: "ScopeAssignmentOperations" }, { branch_id: "WB-03", focus: "APIEndpointContracts" }]

---

## O1b-2-004-N04 — Node 04: DomainProcessorUnit

A: WorkBranch: { branch_id: "WB-02", focus: "ScopeAssignmentOperations" }

B: ProcessedBranchResult: { branch_id: "WB-02", decisions: [{ decision: "GrantScope(service_account_id, scope): add scope to account's scope set if not already present; idempotent" }, { decision: "RevokeScope(service_account_id, scope): remove scope; invalidate all existing tokens for this account after revocation" }] }

---

## O1b-2-004-N05 — Node 05: ApplierUnit

A: ProcessedBranchResult: { branch_id: "WB-02", decisions: ["GrantScope", "RevokeScope"] }

B: AppliedBranchResult: { branch_id: "WB-02", applied: { component: "IServiceAccountScopeManager", spec: "GrantScope(accountId, scope): OperationResult<Unit>; RevokeScope(accountId, scope): OperationResult<Unit> (invalidates existing tokens)" } }

---

## O1b-2-004-N06 — Node 06: ResultTranslatorUnit

A: AppliedBranchResult[]: [ServiceAccountModel, ScopeAssignmentOps, API endpoints]

B: TranslatedResultFragment[]: [{ summary: "ServiceAccount model: { id, tenant_id, client_id, client_secret_hash, scopes[] }" }, { summary: "Scope operations: GrantScope/RevokeScope with token invalidation on revoke" }, { summary: "API: POST /service-accounts, GET /service-accounts/{id}, POST /service-accounts/{id}/scopes/grant, DELETE /service-accounts/{id}/scopes/{scope}" }]

---

## O1b-2-004-N07 — Node 07: ArbiterSelectorUnit

A: TranslatedResultFragment[]: [ServiceAccount model, scope ops, API endpoints]

B: ArbiterReviewRequest[]: [{ arbiter: "AuthorizationArbiter", focus: "Token invalidation on revoke, scope boundary enforcement" }, { arbiter: "ApiContractArbiter", focus: "REST endpoint naming, idempotency of GrantScope" }]

---

## O1b-2-004-N08 — Node 08: ArbiterUnit

A: ArbiterReviewRequest: { arbiter: "AuthorizationArbiter", focus: "Token invalidation on revoke" }

B: ArbiterVerdict: { verdict: "approved", findings: ["RevokeScope must invalidate all tokens for the account immediately", "Use token blacklist or short expiry (≤5 minutes) with forced refresh"], gate_b: "PASSED — revocation cascade is enforced" }

---

## O1b-2-004-N09 — Node 09: FormatterUnit

A: ReviewedResultSet: { all verdicts: approved }

B: HumanResult: "ServiceAccountSetup: ServiceAccount principal with scopes[], per-tenant. API: POST /service-accounts, scope grant/revoke endpoints. RevokeScope invalidates existing tokens immediately. IServiceAccountScopeManager interface produced."
   Unit: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
