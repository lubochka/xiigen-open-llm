# Layer 0-D — Flow Factory Pairs (040) — XXEAudit Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F040-001 — XXE audit → SecurityComplianceReviewFlow63

"Audit all XML parsers for XXE vulnerability exposure and disabled external entity processing."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + XXE + vulnerabilities)

---

## F040-002 — XXE+SSRF chain audit → SecurityComplianceReviewFlow63

"Audit XML processing endpoints for XXE-to-SSRF chaining vectors."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + XXE + SSRF = two attack-vector triggers)

---

## F040-003 — "parse XML response" → GapHandler

"Parse the XML response from the shipping provider API and extract order status."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: XML parsing feature without audit vocabulary is gap.

---

## F040-004 — XXE harden → SecurityComplianceReviewFlow63

"Harden the XML parsers by disabling DTD processing and external entity resolution."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (harden = security-action verb)

---

## F040-005 — XXE write-tests → SecurityTestingFlow52

"Write security tests asserting DTD processing is disabled in all XML parsers."
FlowFactory.Create("SECURITY", subPhase: "5.2") → SecurityTestingFlow52
Teaching: Test-writing context → 5.2.
