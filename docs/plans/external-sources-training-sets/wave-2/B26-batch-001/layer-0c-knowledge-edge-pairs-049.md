# Layer 0-C — Knowledge Edge Pairs (049) — PenTestReport Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E049-001 — Pen test event noun vs audit verb

"the upcoming penetration test" → gap (event noun)
"schedule a pen test" → gap (planning action)
"audit pen test findings" → SECURITY (audit verb on findings)
"review pen test report" → SECURITY (review verb on vulnerability report)
Teaching: "penetration test" as a noun event is always gap. Action on findings fires SECURITY.

---

## E049-002 — Triage fires SECURITY in vulnerability context

"triage vulnerabilities" → SECURITY ("triage" = audit-action verb in security context)
"triage bug reports" in general → gap (project management without vulnerability vocabulary)
"triage the pen test vulnerability findings" → SECURITY (triage + vulnerability)
Teaching: "triage" fires SECURITY when the object is a vulnerability or security finding.

---

## E049-003 — Pen test scope contains multiple attack vectors

"pen test scope: SQL injection, XSS, auth bypass" → SECURITY (three independent attack-vector triggers)
TrustState: deterministic_generic (three triggers)
"pen test scope document" alone → gap (document reference without audit vocabulary)

---

## E049-004 — Remediation from pen test findings boundary

"implement remediations from pen test" → IMPLEMENT (implementation action)
"audit remediation completeness" → SECURITY (audit + security context)
Both can fire together: IMPLEMENT + SECURITY

---

## E049-005 — Trust state calibration

"schedule pen test" → gap
"audit pen test findings with SQL injection and XSS" → deterministic_generic (audit + two triggers)
"review pen test report" → deterministic_generic (review + vulnerability report context)
