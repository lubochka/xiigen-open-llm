# Cross-Layer Synthesis (004)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## S004 — End-to-end trace: Four-predicate — "Audit the PaymentService for PCI-DSS encryption gaps, document the findings, write regression tests for the controls, and get QA sign-off"

### Layer 0-A
Input: "Audit the PaymentService for PCI-DSS encryption gaps, document the findings, write regression tests for the controls, and get QA sign-off."
  "audit" → SECURITY | "encryption gaps" → SECURITY | "PCI-DSS" → gap (compliance framework, not SECURITY) | "document the findings" → DOCUMENT | "write regression tests" → TEST | "QA sign-off" → QA
  CompiledPredicateIds: ["SECURITY", "DOCUMENT", "TEST", "QA"]
  Note: PCI-DSS alone is gap; "encryption gaps" provides the SECURITY trigger independently.

### Layer 0-B
  SECURITY → SubPhase 6.3 | DOCUMENT → SubPhase 6.2 | TEST → SubPhase 5.1 | QA → SubPhase 7.3
  DecisionSet: [SecurityAuditDecision(6.3), DocumentDecision(6.2), TestDecision(5.1), QAGatingDecision(7.3)]

### Layer 0-D
  FlowFactory.CreateMulti(["SECURITY","DOCUMENT","TEST","QA"]) → [SecurityAuditFlow63, DocumentFlow62, UnitTestingFlow51, QAGatingFlow73]
  Sequential ordering: SECURITY audit → TEST (verify controls) → DOCUMENT (findings) → QA (sign-off)

### Layer 1
  TransitionType: not assigned (no IMPLEMENT predicate in set)
  Sequential: SECURITY → TEST → DOCUMENT → QA

### Layer 2 (SecurityAuditFlow63)
  Audit findings: PAN (Primary Account Number) stored unencrypted in PaymentRecord.CardLast4 (CRITICAL) | CVV retained after authorization (CRITICAL) | Node 7: FAIL

### Layer 2 (UnitTestingFlow51)
  Tests: EncryptionService.EncryptPAN → PASS | CardDataRetentionPolicy.CVVRetained → FAIL (pre-existing) | Node 7: FAIL

### Layer 3
  Teaching: PCI-DSS alone is a compliance-framework gap. "encryption gaps" is the SECURITY trigger. DOCUMENT, TEST, QA each fire independently. Four-predicate requests route to four concurrent flows. No TransitionType when IMPLEMENT is absent.

### Cross-layer invariant
  SECURITY, DOCUMENT, TEST, QA are fully independent predicates. Each routes to its own sub-phase. No predicate suppresses another. PCI-DSS does not fire SECURITY; "encryption gaps" does.

---

## S004-B — Corruption variant (v6-01: predicate invention)

Corrupt output: "PCI-DSS" fires SECURITY (invented firing rule)
  CompiledPredicateIds: ["SECURITY", "DOCUMENT", "TEST", "QA", "COMPLIANCE"] ← v6-01 corruption
  Correct output: ["SECURITY", "DOCUMENT", "TEST", "QA"] — PCI-DSS is gap, COMPLIANCE is not a valid predicate

### Teaching
  v6-01 corruption invents predicates ("COMPLIANCE") or incorrectly fires SECURITY on compliance-framework abbreviations (PCI-DSS, SOC2, ISO 27001, GDPR). Correct behavior: compliance frameworks are gap unless paired with a direct security-action verb.

