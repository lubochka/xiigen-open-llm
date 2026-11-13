# Layer 2 — Nine-Node 3.4 Pairs (009)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: nine_node
# Scenario: ProbationaryRejected — SECURITY+TEST — OAuthTestSuite flow rejected, generic fallback
# Flow: GenericFallbackFlow (5.1 probationary_rejected) | State: probationary_rejected

---

## O1b-2-009-N01 — Node 01: DomainSplitterUnit

A: SelectedLogicalTransition: { predicates: ["SECURITY", "TEST"], transition_type: "documentation", user_intent: "write the OAuth 2.0 test suite for client credentials validation", flow_context: "5.1 probationary_rejected — generic fallback active" }

B: DomainPacket[]: [{ domain: "SECURITY", fragment: "client credentials validation test scenarios" }, { domain: "TEST", fragment: "generic test scaffold for OAuth flow, note: specialized test flow in fallback mode" }]
   Unit: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>

---

## O1b-2-009-N02 — Node 02: DomainTranslatorUnit

A: DomainPacket: { domain: "TEST", fragment: "generic test scaffold — fallback mode" }

B: TranslatedDomainStatement: { domain: "TEST", statement: "FALLBACK NOTE: UnitTestingFlow51 is probationary_rejected. GenericFallbackFlow produces a standard test scaffold. This scaffold covers happy-path and one negative case. Specialized edge cases (timing attacks, token rotation boundary) are NOT covered by the generic fallback — those require UnitTestingFlow51 to pass Gate B." }

---

## O1b-2-009-N03 — Node 03: SchemePlannerUnit

A: TranslatedDomainStatement[]: [SECURITY: OAuth credential tests, TEST: generic scaffold (fallback)]

B: WorkBranch[]: [{ branch_id: "WB-01", focus: "HappyPathTest" }, { branch_id: "WB-02", focus: "InvalidCredentialsTest" }]
   Note: Generic fallback limits scope to 2 branches. Specialized flow would produce 5+ branches.

---

## O1b-2-009-N04 — Node 04: DomainProcessorUnit

A: WorkBranch: { branch_id: "WB-01", focus: "HappyPathTest" }

B: ProcessedBranchResult: { branch_id: "WB-01", decisions: [{ decision: "Arrange: valid client_id, valid client_secret, tenant with [read] scope. Act: OAuthTokenService.IssueToken(). Assert: Result.IsSuccess, token.scope == 'read', token.exp > now." }] }

---

## O1b-2-009-N05 — Node 05: ApplierUnit

A: ProcessedBranchResult: { branch_id: "WB-01", decisions: ["HappyPath test"] }

B: AppliedBranchResult: { branch_id: "WB-01", applied: { component: "OAuthTokenServiceTests.Test_IssueToken_HappyPath", spec: "[Fact] test: arrange mock IClientRegistry + ITenantScopeResolver; act; assert token valid", note: "Generic fallback — no specialized timing/rotation edge cases" } }

---

## O1b-2-009-N06 — Node 06: ResultTranslatorUnit

A: AppliedBranchResult[]: [HappyPathTest, InvalidCredentialsTest]

B: TranslatedResultFragment[]: [{ summary: "HappyPath: valid credentials + valid scope → token issued" }, { summary: "InvalidCredentials: wrong secret → 401 returned" }, { note: "FALLBACK: specialized edge cases omitted — requires UnitTestingFlow51 Gate B pass" }]

---

## O1b-2-009-N07 — Node 07: ArbiterSelectorUnit

A: TranslatedResultFragment[]: [HappyPath, InvalidCredentials]

B: ArbiterReviewRequest[]: [{ arbiter: "SecurityArbiter", focus: "Test coverage gaps from fallback mode" }]

---

## O1b-2-009-N08 — Node 08: ArbiterUnit

A: ArbiterReviewRequest: { arbiter: "SecurityArbiter", focus: "Test coverage gaps" }

B: ArbiterVerdict: { verdict: "conditionally_approved", findings: ["Generic fallback covers only 2 of 7 required test scenarios", "Missing: scope intersection boundary, empty scope set, token expiry edge, timing attack"], gate_b: "CONDITIONAL — basic coverage present; full coverage requires UnitTestingFlow51 Gate B pass", fallback_flag: true }

---

## O1b-2-009-N09 — Node 09: FormatterUnit

A: ReviewedResultSet: { verdict: "conditionally_approved", fallback_flag: true }

B: HumanResult: "OAuthTestSuite (FALLBACK MODE — UnitTestingFlow51 probationary_rejected): 2 generic tests produced (HappyPath, InvalidCredentials). WARNING: 5 specialized edge cases not covered. Re-run after UnitTestingFlow51 passes Gate B for full coverage."
   Unit: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
