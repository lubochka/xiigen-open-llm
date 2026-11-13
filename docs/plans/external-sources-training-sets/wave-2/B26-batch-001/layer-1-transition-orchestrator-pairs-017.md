# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T017-001 — SECURITY+QA: feature_addition (vulnerability assessment SSRF/XXE)
Request: "Conduct a vulnerability assessment of the OrderService for SSRF, XXE, and path traversal — and QA findings."
  TransitionType: feature_addition | SECURITY → 6.3 | QA → 7.3

## T017-002 — SECURITY: feature_addition (CVE scan)
Request: "Scan the InvoiceService for known CVE vulnerabilities in its serialization layer."
  TransitionType: feature_addition | SECURITY → 6.3

## T017-003 — SECURITY+DOCUMENT: documentation (timing attack + remediation documented)
Request: "Assess the LoginService for timing attack vulnerabilities and document the remediation steps."
  TransitionType: documentation | SECURITY → 6.3 | DOCUMENT → 6.2

## T017-004 — Zero predicates: N/A (performance assessment)
Request: "Assess the OrderService performance under peak load conditions."
  TransitionType: N/A | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

## T017-005 — SECURITY+QA: feature_addition (ASVS Level 2 + QA failing controls)
Request: "Audit the NotificationService against ASVS Level 2 checklist and QA all failing controls."
  TransitionType: feature_addition | SECURITY → 6.3 | QA → 7.3
