# Layer 0-C — Knowledge Edge Pairs (046) — BrokenAuthAudit Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E046-001 — Broken auth vocabulary

"broken authentication" → SECURITY (OWASP Top 10 category name fires as attack-vector vocabulary)
"credential stuffing" → SECURITY (attack vector)
"MFA bypass" → SECURITY (bypass + MFA = auth group)
"add login page" → gap (feature creation without audit vocabulary)

---

## E046-002 — Session token entropy boundary

"session token entropy" in audit context → SECURITY (token = auth group)
"generate session tokens" → gap (functional task without audit vocabulary)
"audit session token entropy" → SECURITY (audit + token)
Teaching: Generating tokens is gap; auditing their security properties fires SECURITY.

---

## E046-003 — Credential stuffing protection audit

"protect against credential stuffing" → SECURITY (protect = security-action verb)
"implement rate limiting to block credential stuffing" → IMPLEMENT + SECURITY
"configure rate limiting" → gap (configuration without audit vocabulary)

---

## E046-004 — Fallback authentication path

"fallback authentication path" in audit → SECURITY (authentication = auth group)
"implement fallback login via email" → gap (feature implementation without audit vocabulary)
"audit fallback authentication paths for bypass vulnerabilities" → SECURITY (audit + vulnerability)

---

## E046-005 — Trust state calibration

Broken auth alone + audit → deterministic_generic
MFA + credential stuffing + audit → deterministic_generic (high, multiple auth-group triggers)
"add login page" alone → gap (zero predicates)
