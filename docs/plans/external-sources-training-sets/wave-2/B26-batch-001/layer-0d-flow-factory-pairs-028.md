# Layer 0-D — Flow Factory Pairs (028) — CISBenchmark Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F028-001 — CIS Benchmark gap → no flow

"Complete CIS Benchmark assessment."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

---

## F028-002 — CIS + hardening audit → SecurityComplianceReviewFlow63

"Audit server hardening against CIS Benchmark Level 2."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + hardening)
Note: "harden" as standalone action verb creates this flow without additional triggers.

---

## F028-003 — CIS + vulnerability scan → SecurityComplianceReviewFlow63

"Scan infrastructure for CIS violations and open vulnerabilities."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (scan + vulnerabilities)

---

## F028-004 — "harden" verb → SecurityComplianceReviewFlow63 directly

"Harden the API gateway and review injection vulnerability exposure."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
"harden" → security-action verb → instantiates SecurityComplianceReviewFlow63 without other triggers.

---

## F028-005 — CIS + SSH + TLS → SecurityComplianceReviewFlow63 (two independent triggers)

"Review SSH key authentication and TLS protocol versions against CIS controls."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
"ssh" + "tls" = two independent crypto/auth triggers → TrustState: deterministic_generic
