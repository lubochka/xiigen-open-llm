# Layer 3 — Teaching Unit Pairs (017)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU017-001 — SSRF fires SECURITY; SSRF is a named attack vector
A: "Audit the WebhookService for SSRF vulnerabilities." Does SECURITY fire?
B: Yes. "audit" → SECURITY. "SSRF" (Server-Side Request Forgery) → SECURITY (named attack vector). "vulnerabilities" → SECURITY.
   Triple signal. CompiledPredicateIds: ["SECURITY"]. SubPhase: 6.3.

## TU017-002 — XXE fires SECURITY; XML parsing alone does not
A: "Scan the DataImportService for XXE injection in the XML parser." vs "Update the XML parser for schema validation."
B: "XXE injection" + "scan" → SECURITY. CompiledPredicateIds: ["SECURITY"].
   "Update the XML parser for schema validation" → gap (no vulnerability vocabulary).
   Rule: Named injection types (XXE, SQL, SSRF, XSS, IDOR) fire SECURITY. XML operations without injection vocabulary do not.

## TU017-003 — ASVS is an explicit SECURITY trigger
A: "Audit the NotificationService against ASVS Level 2." Does SECURITY fire?
B: Yes. "ASVS" is listed as an explicit SECURITY trigger in the predicate rules. "audit" → SECURITY. Both independently fire.
   CompiledPredicateIds: ["SECURITY"]. Sub-phase 6.3 in audit context.

## TU017-004 — "CVE" fires SECURITY; "bug" does not fire SECURITY
A: "Fix the CVE in the LoginService serialization layer." vs "Fix the bug in the LoginService serialization layer."
B: "CVE" → SECURITY (Common Vulnerabilities and Exposures = vulnerability identifier). CompiledPredicateIds: ["SECURITY"].
   "bug" → gap. CompiledPredicateIds: []. Rule: Named vulnerability identifiers fire SECURITY; generic "bug" is gap.

## TU017-005 — CVSS score fires SECURITY; "score" alone in other contexts is gap
A: "Prioritize the dependency updates by CVSS score — address CRITICAL (≥9.0) first." Does SECURITY fire?
B: Yes. "CVSS score" → SECURITY (vulnerability scoring vocabulary). "CRITICAL" → severity context reinforces SECURITY.
   CompiledPredicateIds: ["SECURITY"]. Teaching: CVSS is vulnerability domain vocabulary → SECURITY.
