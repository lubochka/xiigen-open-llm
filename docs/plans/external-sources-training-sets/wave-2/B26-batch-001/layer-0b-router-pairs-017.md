# Layer 0-B — Router Pairs (017)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: router

---

## R017-001 — SECURITY+QA → [6.3, 7.3]: vulnerability assessment SSRF/XXE/path traversal + QA
Request: "Conduct a vulnerability assessment of the OrderService for SSRF, XXE, and path traversal — and QA the findings."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]

## R017-002 — SECURITY → 6.3: CVE scan
Request: "Scan the InvoiceService for known CVE vulnerabilities in its serialization layer."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## R017-003 — SECURITY+DOCUMENT → [6.3, 6.2]: timing attack assessment documented
Request: "Assess the LoginService for timing attack vulnerabilities and document the remediation steps."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]

## R017-004 — Zero predicates → GapHandler: "performance assessment"
Request: "Assess the OrderService performance under peak load conditions."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Gap: "assess" requires security/vulnerability co-occurrence to fire SECURITY.

## R017-005 — SECURITY+QA → [6.3, 7.3]: ASVS Level 2 audit + QA failing controls
Request: "Audit the NotificationService against the ASVS Level 2 checklist and QA all failing controls."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
  Note: "ASVS" is an explicit SECURITY trigger per predicate rules.
