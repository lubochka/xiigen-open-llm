# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 3.4 Security and Authorization Model
# Batch: O1-batch-001
# Scenario: O1 — multi-tenant JWT authentication
# Contract: SecurityVerification34Flow — nine sequential logical units
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: nine_node_flow

---

## Method note

Layer 2 pairs cover each of the nine canonical logical units inside the
SecurityVerification34Flow for sub-phase 3.4. The flow is invoked with
the SelectedLogicalTransition from Layer 1 pair O1B1-WDA-T1-001.

Node input chaining rule:
- Nodes 03, 06, 07, 09 take collection inputs (arrays) assembled from the
  outputs of all preceding per-item runs.
- Nodes 02, 04, 05, 08 process one item at a time; each pair shows a
  representative item (one DomainPacket, one WorkBranch, one ArbiterReviewRequest).
- A must always be the canonical output type of the preceding node — never a
  raw UserPrompt string (EM-9 class failure).

Node 01 contract: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
A must be a SelectedLogicalTransition — never a raw UserPrompt string.

---

## Pair ID scheme

  O1B1-3.4-N01-001 through O1B1-3.4-N09-001

---

## Pair O1B1-3.4-N01-001 — Node 01 DomainSplitterUnit

```
source_id: xiigen-o1-3.4-n01-001
source_title: XIIGen O1 nine-node 3.4 — Node 01 DomainSplitterUnit
source_license_state: first_party
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
transition_type: feature_addition
phase: 3
sub_phase: 3.4
task_type: security-verification
node_role: node-01-domain-splitter
coverage_status: covered
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-O1-t1-001-3.4-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "3"
  SubPhaseId: "3.4"
  DomainId: "software-development"
  UserRequest: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  PredicateIds: ["SECURITY", "AUTHORIZATION"]
  ContextCarryForward: true
  EvidenceIds: ["O1-router-001-prompt"]
  Metadata:
    transition_classification_basis: "add_new_capability_signal: jwt, authentication, multi-tenant"

B:
  DomainPackets:
    - PacketId: "O1-3.4-packet-security"
      DomainTag: "SECURITY"
      Focus: "JWT token design, signing key management, token lifecycle, ASVS alignment"
      SourceTransitionId: "transition-O1-t1-001-3.4-feature_addition"
      UserSignal: "JWT authentication"

    - PacketId: "O1-3.4-packet-authorization"
      DomainTag: "AUTHORIZATION"
      Focus: "Tenant claim structure, RBAC rules, tenant isolation boundary, cross-tenant access prevention"
      SourceTransitionId: "transition-O1-t1-001-3.4-feature_addition"
      UserSignal: "multi-tenant, tenant isolation"

    - PacketId: "O1-3.4-packet-api-contract"
      DomainTag: "API_CONTRACT"
      Focus: "Authentication endpoint contracts, token request and response schema, tenant header specification"
      SourceTransitionId: "transition-O1-t1-001-3.4-feature_addition"
      UserSignal: "API"

    - PacketId: "O1-3.4-packet-test"
      DomainTag: "TEST"
      Focus: "Tenant isolation test coverage, token lifecycle test cases, negative authorization tests"
      SourceTransitionId: "transition-O1-t1-001-3.4-feature_addition"
      UserSignal: "make sure tenant isolation is tested"

  [unit result envelope]
    UnitId: "node-01-domain-splitter"
    UnitRole: "domain-splitter"
    SubPhaseId: "3.4"
    PacketCount: 4
    Confidence: 0.92
    Warnings: []

---

## Pair O1B1-3.4-N02-001 — Node 02 DomainTranslatorUnit

```
source_id: xiigen-o1-3.4-n02-001
source_title: XIIGen O1 nine-node 3.4 — Node 02 DomainTranslatorUnit
source_license_state: first_party
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
transition_type: feature_addition
phase: 3
sub_phase: 3.4
task_type: security-verification
node_role: node-02-domain-translator
coverage_status: covered
pair_kind: nine_node_flow
```

Note: DomainTranslatorUnit processes one DomainPacket at a time. This pair shows the
representative run for the SECURITY packet. The unit runs once per packet from Node 01.

A: [Representative DomainPacket from Node 01 B — SECURITY packet]
  PacketId: "O1-3.4-packet-security"
  DomainTag: "SECURITY"
  Focus: "JWT token design, signing key management, token lifecycle, ASVS alignment"
  SourceTransitionId: "transition-O1-t1-001-3.4-feature_addition"
  UserSignal: "JWT authentication"

B:
  StatementId: "O1-3.4-stmt-security"
  DomainTag: "SECURITY"
  TechnicalStatement: "Multi-tenant JWT authentication requires RS256-signed tokens with a mandatory tenant_id claim, 15-minute access token TTL (time-to-live), 24-hour refresh token TTL, and ASVS (Application Security Verification Standard) L2 alignment covering requirements V2.1.1, V2.6.1, V8.1.1, and V8.3.1. The signing private key must be held by the API gateway only; tenant services receive the public key for verification."
  SourcePacketId: "O1-3.4-packet-security"
  KeyTerms: ["RS256", "JWT", "tenant_id", "ASVS L2", "token lifecycle", "signing key"]

  [unit result envelope]
    UnitId: "node-02-domain-translator"
    UnitRole: "domain-translator"
    SubPhaseId: "3.4"
    Confidence: 0.92
    Warnings: []

---

## Pair O1B1-3.4-N03-001 — Node 03 SchemePlannerUnit

```
source_id: xiigen-o1-3.4-n03-001
source_title: XIIGen O1 nine-node 3.4 — Node 03 SchemePlannerUnit
source_license_state: first_party
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
transition_type: feature_addition
phase: 3
sub_phase: 3.4
task_type: security-verification
node_role: node-03-scheme-planner
coverage_status: covered
pair_kind: nine_node_flow
```

A: [All TranslatedDomainStatements — one per DomainPacket from Node 01, each translated by Node 02]
  - StatementId: "O1-3.4-stmt-security"
    DomainTag: "SECURITY"
    TechnicalStatement: "RS256-signed JWTs with tenant_id claim; 15-min access TTL; 24-hr refresh TTL; ASVS L2 V2.1.1, V2.6.1, V8.1.1, V8.3.1; private key at API gateway only."
  - StatementId: "O1-3.4-stmt-authorization"
    DomainTag: "AUTHORIZATION"
    TechnicalStatement: "Claim-based tenant isolation with tenant_id validated pre-handler on every request; RBAC roles claim; cross-tenant token reuse returns HTTP 403 with body { error: tenant_mismatch }."
  - StatementId: "O1-3.4-stmt-api-contract"
    DomainTag: "API_CONTRACT"
    TechnicalStatement: "POST /auth/token and POST /auth/refresh endpoints with defined request/response schemas; tenant_id required in all token requests; OAuth 2.0 RFC 6749 error format for failures."
  - StatementId: "O1-3.4-stmt-test"
    DomainTag: "TEST"
    TechnicalStatement: "Test coverage must include: happy path token issuance, token expiry, refresh rotation, cross-tenant rejection (403), missing tenant_id claim rejection, and expired refresh token rejection."

B:
  WorkBranches:
    - BranchId: "O1-3.4-branch-signing"
      BranchTag: "SECURITY-SIGNING"
      Scope: "JWT signing algorithm selection, key management, RS256 configuration, token lifetime policy"
      Priority: 1
      SourceStatementIds: ["O1-3.4-stmt-security"]

    - BranchId: "O1-3.4-branch-isolation"
      BranchTag: "AUTHORIZATION-ISOLATION"
      Scope: "Tenant claim injection at issuance, cross-tenant access prevention, RBAC enforcement, pre-handler validation"
      Priority: 2
      SourceStatementIds: ["O1-3.4-stmt-authorization"]

    - BranchId: "O1-3.4-branch-contract"
      BranchTag: "API-CONTRACT"
      Scope: "Auth endpoint request/response schema, error codes, cross-tenant rejection rule"
      Priority: 3
      SourceStatementIds: ["O1-3.4-stmt-api-contract"]

    - BranchId: "O1-3.4-branch-test"
      BranchTag: "TEST-COVERAGE"
      Scope: "Test cases covering happy path, expiry, tenant isolation, negative authorization"
      Priority: 4
      SourceStatementIds: ["O1-3.4-stmt-test"]

  [unit result envelope]
    UnitId: "node-03-scheme-planner"
    UnitRole: "scheme-planner"
    SubPhaseId: "3.4"
    BranchCount: 4
    Confidence: 0.92
    Warnings: []

---

## Pair O1B1-3.4-N04-001 — Node 04 DomainProcessorUnit

```
source_id: xiigen-o1-3.4-n04-001
source_title: XIIGen O1 nine-node 3.4 — Node 04 DomainProcessorUnit
source_license_state: first_party
contract_signature: DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>
transition_type: feature_addition
phase: 3
sub_phase: 3.4
task_type: security-verification
node_role: node-04-domain-processor
coverage_status: covered
pair_kind: nine_node_flow
```

Note: DomainProcessorUnit processes one WorkBranch at a time. This pair shows the
representative run for the SECURITY-SIGNING branch.

A: [Representative WorkBranch from Node 03 B — SECURITY-SIGNING branch]
  BranchId: "O1-3.4-branch-signing"
  BranchTag: "SECURITY-SIGNING"
  Scope: "JWT signing algorithm selection, key management, RS256 configuration, token lifetime policy"
  Priority: 1
  SourceStatementIds: ["O1-3.4-stmt-security"]

B:
  ProcessedResultId: "O1-3.4-proc-signing"
  BranchId: "O1-3.4-branch-signing"
  BranchTag: "SECURITY-SIGNING"
  Decisions:
    - DecisionId: "D-3.4-01"
      Title: "JWT signing algorithm selection"
      Decision: "Use RS256 (RSA + SHA-256) for all JWT issuance. The API gateway holds the private key. Tenant services receive only the public key for verification. HS256 (symmetric) is not permitted for multi-tenant APIs where tenant services receive tokens."
      Rationale: "Asymmetric signing prevents tenant services from forging tokens — a symmetric key shared across tenants would allow cross-tenant token forgery."
      Alternatives: ["ES256 acceptable if ECDSA key infrastructure is provisioned"]

    - DecisionId: "D-3.4-02"
      Title: "Token lifetime policy"
      Decision: "Access token TTL = 900 seconds (15 minutes). Refresh token TTL = 86 400 seconds (24 hours). Absolute expiry only — no sliding window."
      Rationale: "ASVS V2.1 requires short-lived access tokens. Absolute expiry prevents token lifetime extension through continuous use."
      Alternatives: []

    - DecisionId: "D-3.4-03"
      Title: "Tenant claim injection"
      Decision: "Token issuance service injects tenant_id claim during signing. Value is the UUID of the authenticated tenant record. A token without tenant_id claim is rejected at the resource layer before business logic executes."
      Rationale: "Claim injection at issuance ensures the tenant boundary is enforced by the trusted signing service, not by individual resource handlers."
      Alternatives: []

  AsvsAlignment:
    Addressed: ["V2.1.1", "V2.6.1", "V8.1.1", "V8.3.1"]
    Deferred: ["V2.10 — service-to-service authentication — not in scope for this feature"]

  [unit result envelope]
    UnitId: "node-04-domain-processor"
    UnitRole: "domain-processor"
    SubPhaseId: "3.4"
    DecisionCount: 3
    Confidence: 0.92
    Warnings: ["asvs_gap_deferred:V2.10"]

---

## Pair O1B1-3.4-N05-001 — Node 05 ApplierUnit

```
source_id: xiigen-o1-3.4-n05-001
source_title: XIIGen O1 nine-node 3.4 — Node 05 ApplierUnit
source_license_state: first_party
contract_signature: ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
transition_type: feature_addition
phase: 3
sub_phase: 3.4
task_type: security-verification
node_role: node-05-applier
coverage_status: covered
pair_kind: nine_node_flow
```

Note: ApplierUnit processes one ProcessedBranchResult at a time. This pair shows the
representative run for the SECURITY-SIGNING branch result.

A: [ProcessedBranchResult from Node 04 B — SECURITY-SIGNING branch]
  ProcessedResultId: "O1-3.4-proc-signing"
  BranchId: "O1-3.4-branch-signing"
  BranchTag: "SECURITY-SIGNING"
  Decisions: ["D-3.4-01 (RS256 signing)", "D-3.4-02 (token lifetime)", "D-3.4-03 (tenant claim injection)"]
  AsvsAlignment:
    Addressed: ["V2.1.1", "V2.6.1", "V8.1.1", "V8.3.1"]
    Deferred: ["V2.10"]

B:
  AppliedResultId: "O1-3.4-applied-signing"
  BranchId: "O1-3.4-branch-signing"
  BranchTag: "SECURITY-SIGNING"
  AppliedItems:
    - ItemId: "applied-3.4-01"
      Target: "TokenIssuanceService — signing configuration"
      Change: "Configure RS256 signing with private key loaded from key store at service startup. Sign every issued JWT with RS256 before returning it to the caller."
      DrivenByDecision: "D-3.4-01"

    - ItemId: "applied-3.4-02"
      Target: "TokenIssuanceService — token expiry options"
      Change: "Set AccessTokenExpiry = TimeSpan.FromSeconds(900). Set RefreshTokenExpiry = TimeSpan.FromHours(24). Remove any sliding-expiry configuration. Enforce absolute expiry at issuance."
      DrivenByDecision: "D-3.4-02"

    - ItemId: "applied-3.4-03"
      Target: "TokenIssuanceService — claims builder"
      Change: "Add tenant_id claim injection step after authentication. Source the value from the authenticated tenant record UUID. Reject the issuance request with OperationResult.Fail if tenant UUID is null or empty."
      DrivenByDecision: "D-3.4-03"

  ApplicationSummary: "RS256 signing applied at issuance. Absolute token lifetimes set. tenant_id claim injection wired to authenticated tenant record lookup."
  Warnings: ["asvs_gap_deferred:V2.10"]

  [unit result envelope]
    UnitId: "node-05-applier"
    UnitRole: "applier"
    SubPhaseId: "3.4"
    AppliedItemCount: 3
    Confidence: 0.92
    Warnings: ["asvs_gap_deferred:V2.10"]

---

## Pair O1B1-3.4-N06-001 — Node 06 ResultTranslatorUnit

```
source_id: xiigen-o1-3.4-n06-001
source_title: XIIGen O1 nine-node 3.4 — Node 06 ResultTranslatorUnit
source_license_state: first_party
contract_signature: ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
transition_type: feature_addition
phase: 3
sub_phase: 3.4
task_type: security-verification
node_role: node-06-result-translator
coverage_status: covered
pair_kind: nine_node_flow
```

A: [All AppliedBranchResults — one per WorkBranch from Node 03, each processed by Nodes 04 and 05]
  - AppliedResultId: "O1-3.4-applied-signing"
    BranchTag: "SECURITY-SIGNING"
    AppliedItemCount: 3
  - AppliedResultId: "O1-3.4-applied-isolation"
    BranchTag: "AUTHORIZATION-ISOLATION"
    AppliedItemCount: 3
  - AppliedResultId: "O1-3.4-applied-contract"
    BranchTag: "API-CONTRACT"
    AppliedItemCount: 3
  - AppliedResultId: "O1-3.4-applied-test"
    BranchTag: "TEST-COVERAGE"
    AppliedItemCount: 6

B:
  TranslatedResultFragments:
    - FragmentId: "frag-3.4-security-model"
      FragmentType: "security_model"
      Content:
        AuthenticationMechanism: "JWT Bearer / RS256"
        SigningKey: "RSA private key at API gateway; public key distributed to tenant services"
        AccessTokenTTL: 900
        RefreshTokenTTL: 86400
        TenantClaim: "tenant_id"
        TenantClaimSource: "authenticated tenant record UUID"
        TenantClaimEnforcement: "pre-handler validation on every request"
        CrossTenantRejectionCode: "HTTP 403"
        CrossTenantRejectionBody: "{ 'error': 'tenant_mismatch' }"
        AsvsLevel: "L2"
        SatisfiedRequirements: ["V2.1.1", "V2.6.1", "V8.1.1", "V8.3.1"]
        DeferredRequirements: ["V2.10 (service-to-service auth — deferred)"]
      SourceBranchIds: ["O1-3.4-branch-signing", "O1-3.4-branch-isolation"]

    - FragmentId: "frag-3.4-api-contracts"
      FragmentType: "api_contract"
      Content:
        Endpoints:
          - Endpoint: "POST /auth/token"
            RequestContentType: "application/x-www-form-urlencoded"
            RequestFields: ["grant_type (required)", "username (required)", "password (required)", "tenant_id (UUID, required)"]
            ResponseStatus: 200
            ResponseFields: ["access_token (JWT)", "token_type: Bearer", "expires_in: 900", "refresh_token (opaque)"]
            ErrorStatus: 401
            ErrorBody: "{ error, error_description }"
          - Endpoint: "POST /auth/refresh"
            RequestContentType: "application/json"
            RequestFields: ["refresh_token (opaque, required)", "tenant_id (UUID, required)"]
            ResponseStatus: 200
            ResponseFields: ["access_token (JWT)", "token_type: Bearer", "expires_in: 900"]
            ErrorStatus: 401
            ErrorBody: "{ error }"
        CrossTenantRule: "Any request bearing a JWT whose tenant_id does not match the resource tenant scope must receive HTTP 403. Body must be { 'error': 'tenant_mismatch' } with no additional tenant identifying information."
      SourceBranchIds: ["O1-3.4-branch-contract"]

    - FragmentId: "frag-3.4-test-cases"
      FragmentType: "test_plan"
      Content:
        TestCases:
          - TestId: "TC-3.4-01"
            Name: "Token issued with RS256 signature and correct tenant_id claim"
            Scenario: "POST /auth/token with valid credentials and valid tenant_id"
            ExpectedResult: "200; access_token is a valid JWT signed with RS256; tenant_id claim in payload equals input tenant_id"
          - TestId: "TC-3.4-02"
            Name: "Access token expires after 900 seconds"
            Scenario: "Issue token; wait for TTL to elapse; call protected endpoint"
            ExpectedResult: "401 after TTL elapses; 200 before TTL"
          - TestId: "TC-3.4-03"
            Name: "Cross-tenant token rejected with HTTP 403"
            Scenario: "Issue token for tenant A; call endpoint scoped to tenant B"
            ExpectedResult: "HTTP 403; { 'error': 'tenant_mismatch' }; no tenant B data returned"
          - TestId: "TC-3.4-04"
            Name: "Missing tenant_id claim rejected at resource layer"
            Scenario: "Craft JWT without tenant_id claim; call protected endpoint"
            ExpectedResult: "HTTP 403; not 401; no data returned"
          - TestId: "TC-3.4-05"
            Name: "Refresh token rotates access token correctly"
            Scenario: "POST /auth/refresh with valid refresh token"
            ExpectedResult: "200; new access_token with same tenant_id claim; expires_in: 900"
          - TestId: "TC-3.4-06"
            Name: "Expired refresh token rejected"
            Scenario: "POST /auth/refresh with refresh_token past its 24-hour TTL"
            ExpectedResult: "401 Unauthorized; no new token issued"
      SourceBranchIds: ["O1-3.4-branch-test"]

  [unit result envelope]
    UnitId: "node-06-result-translator"
    UnitRole: "result-translator"
    SubPhaseId: "3.4"
    FragmentCount: 3
    Confidence: 0.92
    Warnings: ["asvs_gap_deferred:V2.10"]

---

## Pair O1B1-3.4-N07-001 — Node 07 ArbiterSelectorUnit

```
source_id: xiigen-o1-3.4-n07-001
source_title: XIIGen O1 nine-node 3.4 — Node 07 ArbiterSelectorUnit
source_license_state: first_party
contract_signature: ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
transition_type: feature_addition
phase: 3
sub_phase: 3.4
task_type: security-verification
node_role: node-07-arbiter-selector
coverage_status: covered
pair_kind: nine_node_flow
```

A: [TranslatedResultFragments from Node 06 B — 3 fragments]
  - FragmentId: "frag-3.4-security-model", FragmentType: "security_model"
  - FragmentId: "frag-3.4-api-contracts", FragmentType: "api_contract"
  - FragmentId: "frag-3.4-test-cases", FragmentType: "test_plan"

B:
  ArbiterReviewRequests:
    - RequestId: "arbiter-req-3.4-01"
      ArbiterRole: "asvs-compliance-arbiter"
      FragmentIds: ["frag-3.4-security-model"]
      ReviewFocus: "Verify ASVS L2 alignment — confirm all addressed requirements are correctly satisfied and all deferred gaps are explicitly documented with justification"
      AcceptanceCriteria: "SatisfiedCount >= 3; every deferred requirement listed in DeferredIds with reason; UnacknowledgedGaps == 0"

    - RequestId: "arbiter-req-3.4-02"
      ArbiterRole: "api-contract-arbiter"
      FragmentIds: ["frag-3.4-api-contracts"]
      ReviewFocus: "Verify auth endpoint contracts are complete — required request fields, response fields, error status codes, and the cross-tenant rejection rule are all specified"
      AcceptanceCriteria: "Both endpoints have request and response schemas; cross-tenant rule specifies status code and response body; no required field is missing"

    - RequestId: "arbiter-req-3.4-03"
      ArbiterRole: "test-coverage-arbiter"
      FragmentIds: ["frag-3.4-test-cases"]
      ReviewFocus: "Verify test coverage spans happy path, token expiry, refresh rotation, tenant isolation (cross-tenant 403), missing tenant_id claim, and expired refresh rejection"
      AcceptanceCriteria: "TestCaseCount >= 5; tenant isolation case present; at least one negative authorization case present"

  [unit result envelope]
    UnitId: "node-07-arbiter-selector"
    UnitRole: "arbiter-selector"
    SubPhaseId: "3.4"
    RequestCount: 3
    Confidence: 0.92
    Warnings: []

---

## Pair O1B1-3.4-N08-001 — Node 08 ArbiterUnit

```
source_id: xiigen-o1-3.4-n08-001
source_title: XIIGen O1 nine-node 3.4 — Node 08 ArbiterUnit
source_license_state: first_party
contract_signature: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
transition_type: feature_addition
phase: 3
sub_phase: 3.4
task_type: security-verification
node_role: node-08-arbiter
coverage_status: covered
pair_kind: nine_node_flow
```

Note: ArbiterUnit processes one ArbiterReviewRequest at a time. This pair shows the
representative run for the ASVS compliance review request.

A: [Representative ArbiterReviewRequest from Node 07 B — ASVS compliance request]
  RequestId: "arbiter-req-3.4-01"
  ArbiterRole: "asvs-compliance-arbiter"
  FragmentIds: ["frag-3.4-security-model"]
  ReviewFocus: "Verify ASVS L2 alignment — addressed requirements correctly satisfied; deferred gaps documented"
  AcceptanceCriteria: "SatisfiedCount >= 3; every deferred requirement in DeferredIds with reason; UnacknowledgedGaps == 0"

B:
  VerdictId: "verdict-3.4-01-asvs"
  RequestId: "arbiter-req-3.4-01"
  ArbiterRole: "asvs-compliance-arbiter"
  Verdict: "PASS_WITH_GAPS"
  ReviewedFragmentId: "frag-3.4-security-model"
  Findings:
    - FindingId: "finding-3.4-01"
      Type: "pass"
      Description: "ASVS L2 requirements V2.1.1 (short-lived tokens), V2.6.1 (lookup secret verifiers), V8.1.1 (sensitive data protection), V8.3.1 (data exposure prevention) are satisfied by the RS256 + tenant_id claim + absolute token lifetime policy."
    - FindingId: "finding-3.4-02"
      Type: "gap"
      Description: "ASVS V2.10 (service-to-service authentication) is not addressed in this feature. Explicitly deferred to a separate infrastructure task. Not an unacknowledged gap."
      Severity: "deferred"
  CriteriaResult:
    SatisfiedCount: "4 (threshold >= 3: PASS)"
    DeferredIds: ["V2.10 — service-to-service authentication — separate infrastructure task required"]
    UnacknowledgedGaps: 0
  AcceptanceMet: true

  [unit result envelope]
    UnitId: "node-08-arbiter"
    UnitRole: "arbiter"
    SubPhaseId: "3.4"
    Confidence: 0.92
    Warnings: ["asvs_gap_deferred:V2.10"]

---

## Pair O1B1-3.4-N09-001 — Node 09 FormatterUnit

```
source_id: xiigen-o1-3.4-n09-001
source_title: XIIGen O1 nine-node 3.4 — Node 09 FormatterUnit
source_license_state: first_party
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
transition_type: feature_addition
phase: 3
sub_phase: 3.4
task_type: security-verification
node_role: node-09-formatter
coverage_status: covered
pair_kind: nine_node_flow
```

A: [ReviewedResultSet — all ArbiterVerdicts from Node 08 runs, one per ArbiterReviewRequest]
  Verdicts:
    - VerdictId: "verdict-3.4-01-asvs"
      ArbiterRole: "asvs-compliance-arbiter"
      Verdict: "PASS_WITH_GAPS"
      AcceptanceMet: true
    - VerdictId: "verdict-3.4-02-api"
      ArbiterRole: "api-contract-arbiter"
      Verdict: "PASS"
      AcceptanceMet: true
    - VerdictId: "verdict-3.4-03-test"
      ArbiterRole: "test-coverage-arbiter"
      Verdict: "PASS"
      AcceptanceMet: true

B:
  FlowId: "security-verification-flow-3.4"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "PASS_WITH_GAPS"
  FinalHumanResult: |
    Security and authorization model for multi-tenant JWT authentication:

    Authentication: RS256-signed JWTs issued at POST /auth/token. Private
    key held at the API gateway only; tenant services use the public key.
    Access token TTL (time-to-live): 900 s. Refresh token TTL: 86 400 s.
    Absolute expiry — no sliding window.

    Tenant isolation: tenant_id claim injected at issuance from the
    authenticated tenant record UUID. Pre-handler validation on every
    request. Cross-tenant token reuse returns HTTP 403 { error:
    tenant_mismatch } with no identifying information about the correct
    tenant.

    API contracts: POST /auth/token and POST /auth/refresh schemas
    fully defined with request fields, response fields, and error codes.

    Test coverage: six test cases cover token issuance (RS256 + tenant_id),
    expiry enforcement, cross-tenant rejection, missing claim rejection,
    refresh rotation, and expired refresh rejection.

    ASVS (Application Security Verification Standard) L2 alignment: four
    requirements satisfied (V2.1.1, V2.6.1, V8.1.1, V8.3.1). V2.10
    (service-to-service authentication) deferred — separate infrastructure
    task required.

  Metrics:
    node_count: "9"
    sub_phase_id: "3.4"
    node_01_unit_id: "node-01-domain-splitter"
    node_02_unit_id: "node-02-domain-translator"
    node_03_unit_id: "node-03-scheme-planner"
    node_04_unit_id: "node-04-domain-processor"
    node_05_unit_id: "node-05-applier"
    node_06_unit_id: "node-06-result-translator"
    node_07_unit_id: "node-07-arbiter-selector"
    node_08_unit_id: "node-08-arbiter"
    node_09_unit_id: "node-09-formatter"
    branch_count: "4"
    applied_item_count: "12"
    fragment_count: "3"
    arbiter_request_count: "3"
    verdict_count: "3"
    test_case_count: "6"
    api_contract_count: "2"
    asvs_level: "L2"
    asvs_satisfied: "4"
    asvs_deferred: "1"

  EvidenceIds:
    - "O1-3.4-stmt-security"
    - "O1-3.4-stmt-authorization"
    - "O1-3.4-stmt-api-contract"
    - "O1-3.4-stmt-test"
    - "O1-3.4-branch-signing"
    - "O1-3.4-branch-isolation"
    - "O1-3.4-branch-contract"
    - "O1-3.4-branch-test"
    - "frag-3.4-security-model"
    - "frag-3.4-api-contracts"
    - "frag-3.4-test-cases"
    - "verdict-3.4-01-asvs"
    - "verdict-3.4-02-api"
    - "verdict-3.4-03-test"

  Warnings:
    - "asvs_gap_deferred:V2.10"

  [unit result envelope]
    UnitId: "node-09-formatter"
    UnitRole: "formatter"
    SubPhaseId: "3.4"
    Confidence: 0.92
    Warnings: ["asvs_gap_deferred:V2.10"]
