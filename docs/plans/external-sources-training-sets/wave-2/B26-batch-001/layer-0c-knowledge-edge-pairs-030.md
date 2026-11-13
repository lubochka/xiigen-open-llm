# Layer 0-C — Knowledge Edge Pairs (030) — OWASPTop10Review Knowledge Edge
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E030-001 — OWASP Top 10 label behavior

"OWASP Top 10" alone → gap label
"OWASP Top 10 compliant" → gap (compliance adjective)
"Review against OWASP Top 10" → SECURITY fires (OWASP Top 10 is strong enough co-occurrence to push "review" past gap)
Teaching: OWASP Top 10 is the exception — "review" + OWASP Top 10 → SECURITY fires despite "review" being borderline.

---

## E030-002 — OWASP category mapping to predicates

A01 Broken Access Control: IDOR, privilege escalation → SECURITY + AUTHORIZATION (dual-trigger)
A02 Cryptographic Failures: cipher, certificate, TLS → SECURITY alone
A03 Injection: SQL injection, XSS, XXE, SSRF → SECURITY alone
A07 Identification and Authentication: JWT, MFA, session → SECURITY alone
A08 Software and Data Integrity: SRI, signing, integrity → SECURITY alone

---

## E030-003 — "make OWASP compliant" vs "audit for OWASP"

"Make the application OWASP Top 10 compliant" → gap (make compliant = goal, no action)
"Audit the application for OWASP Top 10 vulnerabilities" → SECURITY
Teaching: Compliance goal without technical action → gap. Audit action → SECURITY.

---

## E030-004 — OWASP + DOCUMENT synthesis

"Audit OWASP vulnerabilities and document all findings" → SECURITY + DOCUMENT
"Write OWASP compliance documentation" → DOCUMENT only (no security action in document creation)

---

## E030-005 — Trust state for OWASP reviews

"Audit for SQL injection and XSS per OWASP A03" → deterministic_generic (3 SECURITY triggers)
"Review application against OWASP Top 10" → deterministic_generic (OWASP co-occurrence elevates trust)
"Make OWASP compliant" → knowledge_edge (zero predicates)
