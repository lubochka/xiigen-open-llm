# Layer 0-B — Router Pairs (045) — SensitiveDataExposure Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R045-001 — Sensitive data exposure audit → 6.3

R045-001 test: "Audit application logs for sensitive PII leakage."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R045-002 — TLS audit → 6.3

R045-002 test: "Audit all external API calls to verify TLS encryption is enforced for data in transit."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R045-003 — "mask credit card numbers in UI" → GapHandler

R045-003 test: "Mask the last four digits of credit card numbers in the account dashboard UI."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R045-004 — Secret scan + IMPLEMENT → 4.1 + 6.3

R045-004 test: "Scan for hardcoded secrets and implement a secrets management solution to rotate them."
  SECURITY → 6.3 (scan + hardcoded secrets) | IMPLEMENT → 4.1
  TransitionType: feature_addition

---

## R045-005 — Sensitive data + DOCUMENT → 6.3 + 6.2

R045-005 test: "Audit sensitive data exposure in API responses and document findings in a data classification report."
  SECURITY → 6.3 | DOCUMENT → 6.2 | TransitionType: documentation
