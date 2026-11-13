# Layer 2 — Nine-Node 3.4 Pairs (006)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: nine_node
# Scenario: ScopeManagementTests — SECURITY+AUTHORIZATION+TEST — write tests for scope assignment logic
# Flow: SecurityVerification34Flow | State: trusted_learned

---

## O1b-2-006-N01 — Node 01: DomainSplitterUnit

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION", "TEST"], transition_type: "documentation", user_intent: "write tests for the tenant scope assignment and enforcement logic" }

B: DomainPacket[]: [{ domain: "SECURITY", fragment: "JWT claim extraction in tests, token mock setup" }, { domain: "AUTHORIZATION", fragment: "scope assignment scenarios, intersection logic, tenant isolation tests" }, { domain: "TEST", fragment: "unit tests for ScopeValidator, TenantResolver, AccessDecider" }]
   Unit: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>

---

## O1b-2-006-N02 — Node 02: DomainTranslatorUnit

A: DomainPacket: { domain: "AUTHORIZATION", fragment: "scope assignment scenarios, intersection logic" }

B: TranslatedDomainStatement: { domain: "AUTHORIZATION", statement: "Test scenarios: (1) Tenant allows [read, write], request asks for [read] → AccessDecision.Allow. (2) Tenant allows [read], request asks for [write] → AccessDecision.Deny. (3) Tenant allows [read, write], request asks for [admin] → AccessDecision.Deny. (4) Empty scope intersection → AccessDecision.Deny. (5) Cross-tenant scope read → isolated (no leakage)." }

---

## O1b-2-006-N03 — Node 03: SchemePlannerUnit

A: TranslatedDomainStatement[]: [SECURITY: JWT mock, AUTHORIZATION: scope scenarios, TEST: unit tests]

B: WorkBranch[]: [{ branch_id: "WB-01", focus: "ScopeValidatorTests" }, { branch_id: "WB-02", focus: "TenantIsolationTests" }, { branch_id: "WB-03", focus: "AccessDeciderTests" }]

---

## O1b-2-006-N04 — Node 04: DomainProcessorUnit

A: WorkBranch: { branch_id: "WB-01", focus: "ScopeValidatorTests" }

B: ProcessedBranchResult: { branch_id: "WB-01", decisions: [{ decision: "Test_AllowWhenScopePresent: Arrange tenant allows [read]; Act: ScopeValidator.Validate([read]); Assert: Allow" }, { decision: "Test_DenyWhenScopeAbsent: Arrange tenant allows [read]; Act: ScopeValidator.Validate([write]); Assert: Deny" }, { decision: "Test_EmptyScopeIntersection: Arrange tenant allows []; Assert: Deny" }] }

---

## O1b-2-006-N05 — Node 05: ApplierUnit

A: ProcessedBranchResult: { branch_id: "WB-01", decisions: ["AllowWhenPresent", "DenyWhenAbsent", "EmptyIntersection"] }

B: AppliedBranchResult: { branch_id: "WB-01", applied: { component: "ScopeValidatorTests", spec: "xUnit test class with [Fact] methods: Test_Allow_WhenRequiredScopePresent, Test_Deny_WhenRequiredScopeAbsent, Test_Deny_WhenScopeSetEmpty, Test_Deny_WhenScopeSetNull. Mock: IInMemoryTenantScopeRepository." } }

---

## O1b-2-006-N06 — Node 06: ResultTranslatorUnit

A: AppliedBranchResult[]: [ScopeValidatorTests, TenantIsolationTests, AccessDeciderTests]

B: TranslatedResultFragment[]: [{ summary: "ScopeValidatorTests: 4 unit tests covering allow/deny/empty cases" }, { summary: "TenantIsolationTests: 2 tests verifying cross-tenant scope reads return empty (no leakage)" }, { summary: "AccessDeciderTests: 3 tests covering Allow/Deny/boundary conditions" }]

---

## O1b-2-006-N07 — Node 07: ArbiterSelectorUnit

A: TranslatedResultFragment[]: [ScopeValidatorTests, TenantIsolationTests, AccessDeciderTests]

B: ArbiterReviewRequest[]: [{ arbiter: "SecurityArbiter", focus: "JWT mock setup correctness" }, { arbiter: "AuthorizationArbiter", focus: "Cross-tenant isolation test coverage" }]

---

## O1b-2-006-N08 — Node 08: ArbiterUnit

A: ArbiterReviewRequest: { arbiter: "AuthorizationArbiter", focus: "Cross-tenant isolation test coverage" }

B: ArbiterVerdict: { verdict: "approved", findings: ["TenantIsolationTests must use separate mock repositories per tenant", "Assert that Tenant A's scopes are never visible to Tenant B's validator"], gate_b: "PASSED — tenant isolation test coverage is complete" }

---

## O1b-2-006-N09 — Node 09: FormatterUnit

A: ReviewedResultSet: { all verdicts: approved }

B: HumanResult: "ScopeManagementTests: 9 unit tests total — 4 ScopeValidator (allow/deny/empty), 2 TenantIsolation (cross-tenant separation), 3 AccessDecider (boundary cases). JWT mocked via IInMemoryTenantScopeRepository. All tests use xUnit [Fact] pattern."
   Unit: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
