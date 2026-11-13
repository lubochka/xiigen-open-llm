# Layer 0-A — Compiler Pairs (017)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: compiler

---

## C017-001 — SECURITY+QA: vulnerability assessment

A: "Conduct a vulnerability assessment of the OrderService — check for SSRF (Server-Side Request Forgery), XXE (XML External Entity), and path traversal exposures — and QA the findings."
  "vulnerability assessment" → SECURITY; "SSRF" → SECURITY; "XXE" → SECURITY; "path traversal" → SECURITY; "QA" → QA
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3

---

## C017-002 — SECURITY: CVE (Common Vulnerabilities and Exposures) scan

A: "Scan the InvoiceService for known CVE vulnerabilities in its serialization layer."
  "scan" → SECURITY; "CVE vulnerabilities" → SECURITY; "serialization" → SECURITY (deserialization attacks are security vulnerabilities)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C017-003 — SECURITY+DOCUMENT: vulnerability findings report

A: "Assess the LoginService for timing attack vulnerabilities and document the remediation steps."
  "assess" → SECURITY (vulnerability-assessment context); "timing attack vulnerabilities" → SECURITY; "document" → DOCUMENT; "remediation steps" → no predicate alone
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C017-004 — Gap: "performance assessment" is not SECURITY

A: "Assess the OrderService performance under peak load conditions."
  "assess" → gap (performance context, no security co-occurrence); "performance" → gap; "peak load" → gap
  CompiledPredicateIds: [] → knowledge_edge
  Gap: "assess" fires SECURITY only with vulnerability/security co-occurrence.

---

## C017-005 — SECURITY+QA: ASVS (Application Security Verification Standard) assessment + QA

A: "Audit the NotificationService against the ASVS Level 2 checklist and QA all failing controls."
  "audit" → SECURITY; "ASVS" → SECURITY (asvs trigger per predicate rules); "failing controls" → SECURITY context; "QA" → QA
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3
  Note: "ASVS" is an explicit SECURITY trigger per predicate rules.
