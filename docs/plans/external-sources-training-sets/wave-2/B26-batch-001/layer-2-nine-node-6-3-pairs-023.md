# Layer 2 — Nine-Node Flow Pairs (023): ProbationaryRejected
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# SubPhase: 6.3 | Flow: SecurityComplianceReviewFlow63
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## N023 — SecurityComplianceReviewFlow63: Probationary Rejected — Compliance Testing Flow (SECURITY+QA+TEST)

Request: "Write compliance tests that verify the SecurityService enforces ASVS Level 2 controls and QA the test suite coverage."
CompiledPredicateIds: ["SECURITY", "TEST", "QA"] | TrustState: probationary_accepted

Teaching: This request fires SECURITY via "ASVS" + "controls" and TEST via "write compliance tests".
In a test-writing context, SECURITY routes to 5.2 (Security Testing), NOT 6.3 (Security Review).
The initial probationary routing to SecurityComplianceReviewFlow63 (6.3) is REJECTED by Node 8.

### Node 1 — DomainSplitter
A: Request text
B:
  SelectedLogicalTransition: SecurityComplianceReviewFlow63 [PROBATIONARY — test-writing context detected]
  DomainBoundaries:
    - domain: "compliance_test_writing" | scope: "Writing test code for ASVS controls — action verb is 'write tests', not 'audit'"
    - domain: "qa_coverage" | scope: "QA verification of test suite completeness"
  ProbationaryWarning: "Action verb 'write compliance tests' suggests SecurityTestingFlow52 (5.2), not SecurityComplianceReviewFlow63 (6.3)"

### Node 2 — DomainTranslator
A: Domain buckets with probationary warning
B:
  TranslatedBoundaries:
    - domain: compliance_test_writing
      note: "Test-writing action detected. ASVS Level 2 compliance tests → sub-phase 5.2 (Security Testing). Continuing in probationary mode."
    - domain: qa_coverage
      note: "QA of test suite → sub-phase 7.3 (QA Testing and Validation)."

### Node 3 — SchemePlanner
A: Probationary routing in effect
B:
  AuditSchema:
    probationary_note: "If this request is for writing test code (not auditing), correct flow is SecurityTestingFlow52 at sub-phase 5.2."
    proceeding_with: "SecurityComplianceReviewFlow63 in probationary mode for context comparison."

### Node 4 — DomainProcessor
A: Attempt to process as security review
B:
  Findings:
    context_mismatch:
      - item: "Action verb" | finding: "'Write compliance tests' is a test-authoring action, not a security audit action" | severity: N/A | note: "This request belongs to SecurityTestingFlow52 (5.2)"
      - item: "ASVS in test context" | finding: "ASVS Level 2 as a test target fires SECURITY but 'write tests for ASVS' → 5.2" | severity: N/A | note: "Audit context → 6.3; test-writing context → 5.2"

### Node 5 — Applier
A: Context mismatch findings
B:
  AppliedClassification:
    total_findings: 0 (no audit findings — wrong flow for this request context)
    mismatch_detected: true

### Node 6 — ResultTranslator
A: No audit findings; mismatch
B:
  TranslatedOutput:
    format: "FlowMismatchSignal"
    message: "Request is test-writing, not auditing. SecurityComplianceReviewFlow63 is incorrect for this request."
    correct_flow: "SecurityTestingFlow52 (sub-phase 5.2)"

### Node 7 — ArbiterSelector
A: FlowMismatchSignal, probationary_accepted
B:
  SelectedArbiter: "ContextArbiter"
  SelectionReason: "probationary_accepted with context mismatch → ContextArbiter evaluates routing correctness"

### Node 8 — Arbiter
A: ContextArbiter evaluates request
B:
  ArbiterVerdict:
    OverallVerdict: REJECTED
    RejectionReason: "Action verb 'write compliance tests' routes SECURITY to 5.2 (SecurityTestingFlow52), not 6.3. This probationary routing to 6.3 is rejected."
    CorrectRouting: "SECURITY → 5.2 | TEST → 5.1 | QA → 7.3"
    ProbationaryOutcome: "probationary_rejected"

### Node 9 — Formatter
A: REJECTED verdict
B:
  FormattedOutput:
    status: "REJECTED — Probationary routing to 6.3 rejected. Route to SecurityTestingFlow52 (5.2) instead."
    teaching: "Write-test-code context → SECURITY → 5.2. Audit/review context → SECURITY → 6.3. Action verb is the discriminator."
    output_status: "probationary_rejected"
