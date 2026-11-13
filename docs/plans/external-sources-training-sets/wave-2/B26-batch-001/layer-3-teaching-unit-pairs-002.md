# Layer 3 — Teaching Unit Pairs (002)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU002-001 — Teach: SECURITY sub-phase context routing
  Audit/review/scan context → 6.3 (SecurityAuditFlow63)
  Write/implement/enforce context → 5.2 (SecurityTestingFlow52)
  "Audit the AuthService for SQL injection." → 6.3
  "Implement SQL injection prevention." → 5.2
  Rule: The action verb determines the sub-phase, not the attack vector vocabulary.

## TU002-002 — Teach: "QA" abbreviation fires QA; "quality assurance" does NOT fire QA
  "QA the security controls." → ["QA"] — abbreviation fires
  "Perform quality assurance on the security controls." → [] — spelled out is gap
  Rule: QA is abbreviation-gated. Strict lexicon rule.

## TU002-003 — Teach: "audit" as verb fires SECURITY; "audit" as noun (event) is gap
  "Audit the AdminService." → ["SECURITY"] — verb
  "The audit is scheduled for next month." → [] — noun event
  Rule: Grammatical form determines predicate. Verb "audit" → SECURITY.

## TU002-004 — Teach: GDPR, SOC2, ISO 27001 are gap vocabulary
  Compliance framework names do NOT fire SECURITY.
  "Ensure GDPR compliance." → [] | "Prepare for SOC2." → []
  Rule: Compliance is a business/legal domain — gap. SECURITY fires on attack/enforcement vocabulary.

## TU002-005 — Teach: "vulnerability" as standalone noun fires SECURITY
  "There is a vulnerability in the LoginService." → ["SECURITY"]
  Contrast: "There is a bug in the LoginService." → [] (gap)
  Rule: "vulnerability" fires SECURITY; "bug" is gap. The word choice matters.
