# Layer 2 — Nine-Node Flow Pairs (021)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Flow: UnitTestingFlow51 — JwtTokenBuilderTests | TEST+SECURITY
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## Flow Context

Request: "Write unit tests for the JwtTokenBuilder covering token generation, claims insertion, expiry, and signature verification."
Sub-phase: 5.1 | TrustState: deterministic_generic | FlowId: UnitTestingFlow51
Predicates: ["TEST", "IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
Note: 5.1 flow covers unit test structure; 5.2 companion flow covers security compliance tests.

---

## Node 01 — DomainSplitterUnit (SelectedLogicalTransition)

B:
  DomainBoundaries:
    - domain: "token_generation" | scope: "Build(claims, expiry) → signed JWT"
    - domain: "claims_insertion" | scope: "AddClaim(key, value) — userId, roles, issued-at"
    - domain: "expiry_enforcement" | scope: "IsExpired(token, now)"
    - domain: "signature_verification" | scope: "VerifySignature(token, publicKey) [→ 5.2]"

---

## Node 02 — DomainTranslatorUnit

B:
  TranslatedTestBoundaries:
    - source_domain: "token_generation" | test_boundary: "valid input → non-null token; null claims → error" | test_type: "unit"
    - source_domain: "claims_insertion" | test_boundary: "userId claim present; roles claim present; missing required claim → error" | test_type: "unit"
    - source_domain: "expiry_enforcement" | test_boundary: "within expiry → not expired; past expiry → expired" | test_type: "unit"

---

## Node 03 — SchemePlannerUnit

B:
  TestSuiteSchema:
    framework: "xUnit" | pattern: "AAA"
    test_classes:
      - class: "JwtTokenBuilderTests"
        methods:
          - "Build_ValidClaims_ReturnsNonNullToken"
          - "Build_NullClaims_ThrowsArgumentException"
          - "AddClaim_UserId_PresentInTokenPayload"
          - "AddClaim_Roles_PresentInTokenPayload"
          - "AddClaim_MissingRequiredClaim_ThrowsInvalidClaimException"
          - "IsExpired_TokenWithinWindow_ReturnsFalse"
          - "IsExpired_TokenPastWindow_ReturnsTrue"

---

## Node 04 — DomainProcessorUnit

B:
  TestDoubles: ["MockClockProvider (controls 'now' for expiry tests)", "FakeSigningKeyProvider"]

---

## Node 05 — ApplierUnit

B: TestsProduced: 7 | Framework: xUnit

---

## Node 06 — ResultTranslatorUnit

B: VerificationResult: "All 7 pass" | FailedTests: 0

---

## Node 07 — ArbiterSelectorUnit

B: QualityGateStatus: PASS | CoverageEstimate: "~90%"
  Note: RS256 tamper test and revocation test are in 5.2 companion flow.

---

## Node 08 — ArbiterUnit

B: Verdict: PASS | Files: ["JwtTokenBuilderTests.cs"]

---

## Node 09 — FormatterUnit

B: Result: PASS | Tests: 7 | Status: green
  OutputSummary: "JwtTokenBuilderTests — generation, claims, expiry. Tamper/revocation security tests in 5.2 companion."
