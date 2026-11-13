# Layer 2 — Nine-Node Flow Pairs (020)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Flow: UnitTestingFlow51 — PasswordValidatorTests | TEST+SECURITY
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## Flow Context

Request: "Write security tests for the PasswordValidator to assert that hashed passwords are never logged and validation timing is constant."
Sub-phase: 5.1 | TrustState: deterministic_generic | FlowId: UnitTestingFlow51
Predicates: ["TEST", "IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
Note: TEST routes to 5.1 (UnitTestingFlow51); SECURITY routes to 5.2 (SecurityComplianceFlow52) in parallel.
This nine-node flow covers the 5.1 (TEST) portion only.

---

## Node 01 — DomainSplitterUnit (SelectedLogicalTransition)

B:
  DomainBoundaries:
    - domain: "complexity_rules" | scope: "Validate(password) — length, uppercase, digit, special char"
    - domain: "blocklist_check" | scope: "IsCommonPassword(password)"
    - domain: "history_enforcement" | scope: "IsRecentPassword(accountId, password, historyCount)"
    - domain: "security_properties" | scope: "No log of plaintext/hash; constant-time comparison [→ 5.2]"

---

## Node 02 — DomainTranslatorUnit

B:
  TranslatedTestBoundaries:
    - source_domain: "complexity_rules" | test_boundary: "too short, missing uppercase, missing digit, missing special char, all rules met" | test_type: "unit"
    - source_domain: "blocklist_check" | test_boundary: "'password123' → blocked; random strong password → allowed" | test_type: "unit"
    - source_domain: "history_enforcement" | test_boundary: "last-5 password match → rejected; unique password → accepted" | test_type: "unit"

---

## Node 03 — SchemePlannerUnit

B:
  TestSuiteSchema:
    framework: "xUnit" | pattern: "AAA"
    test_classes:
      - class: "PasswordValidatorTests"
        methods:
          - "Validate_TooShort_ReturnsFalse"
          - "Validate_MissingUppercase_ReturnsFalse"
          - "Validate_MissingDigit_ReturnsFalse"
          - "Validate_MissingSpecialChar_ReturnsFalse"
          - "Validate_AllRulesMet_ReturnsTrue"
          - "IsCommonPassword_KnownWeakPassword_ReturnsTrue"
          - "IsCommonPassword_StrongPassword_ReturnsFalse"
          - "IsRecentPassword_MatchesLastFive_ReturnsFalse"
          - "IsRecentPassword_UniquePassword_ReturnsTrue"

---

## Node 04 — DomainProcessorUnit

B:
  TestDoubles: ["MockPasswordHistoryRepository", "MockCommonPasswordList"]

---

## Node 05 — ApplierUnit

B: TestsProduced: 9 | Framework: xUnit

---

## Node 06 — ResultTranslatorUnit

B: VerificationResult: "All 9 pass" | FailedTests: 0

---

## Node 07 — ArbiterSelectorUnit

B: QualityGateStatus: PASS | CoverageEstimate: "~95%"
  Note: Constant-time comparison security test is covered in the SECURITY sub-flow at 5.2, not in this 5.1 flow.

---

## Node 08 — ArbiterUnit

B: Verdict: PASS | Files: ["PasswordValidatorTests.cs"]

---

## Node 09 — FormatterUnit

B: Result: PASS | Tests: 9 | Status: green
  OutputSummary: "PasswordValidatorTests — complexity rules, blocklist, history enforcement. Security timing tests in 5.2 companion flow."
