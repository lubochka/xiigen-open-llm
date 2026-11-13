# Layer 3 — Teaching Unit Pairs (024) — Teaching Summary for Sub-Phase 6.3
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU024-001 — Master rule: SECURITY sub-phase routing
A: When does SECURITY route to 6.3 vs 5.2?
B: 6.3 (Security and Compliance Review): audit/review/scan context — "audit the service", "review findings", "scan for vulnerabilities"
   5.2 (Security Testing): test-writing context — "write security tests", "assert the service blocks", "implement security test suite"
   The action verb is the routing discriminator. Attack vector vocabulary (XSS, SQL injection, SSRF) alone does not determine the sub-phase.

## TU024-002 — Master rule: QA predicate gate
A: When does QA fire?
B: Only when the literal abbreviation "QA" appears (two capital letters).
   "quality assurance" (spelled out) → gap. "QA check", "QA sign-off", "QA the controls" all fire QA.
   QA → sub-phase 7.3 always. QA does not contribute TransitionType in sub-phase 6.3 context.

## TU024-003 — Master rule: gap vocabulary for SECURITY
A: What vocabulary does NOT fire SECURITY?
B: Regulatory labels: GDPR, SOC2, ISO 27001, PCI-DSS (standalone acronyms)
   Compliance terms: "compliance", "compliant", "compliance-ready"
   Risk terms: "risk assessment", "risk management", "security risk"
   Vague security: "make it secure", "more secure", "ensure security"
   Event nouns: "the upcoming audit", "the scheduled audit"
   None of these fire SECURITY. Only technical security-action vocabulary fires.

## TU024-004 — Master rule: dual-predicate triggers
A: Which vocabulary triggers BOTH SECURITY and AUTHORIZATION?
B: "privilege escalation" → SECURITY + AUTHORIZATION
   "IDOR" (Insecure Direct Object Reference) → SECURITY + AUTHORIZATION
   "wildcard permission" → SECURITY + AUTHORIZATION
   All three route both predicates to sub-phase 6.3 in audit context.

## TU024-005 — Master rule: Arbiter always reports FAIL — no suppression
A: Can the Arbiter suppress a FAIL verdict when security findings are present?
B: No. The Arbiter NEVER suppresses FAIL results. FAIL is a valid and required output.
   Security audits commonly produce FAIL results. The model must output negative findings.
   FAIL ≠ error; FAIL = correct reporting of a security finding.
   Teaching point: a model that always outputs PASS is miscalibrated. FAIL is expected when CRITICAL findings exist.
