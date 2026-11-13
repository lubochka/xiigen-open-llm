# Layer 0-C — Knowledge Edge Pairs (029) — NISTFramework Knowledge Edge
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E029-001 — NIST labels and functions that are always gap

"NIST CSF" (Cybersecurity Framework) — framework label → gap alone
"NIST CSF Identify function" — management function → gap
"NIST CSF Protect function" — management function → gap
"asset inventory" — inventory/admin → gap (no security action verb)
"risk assessment" — risk vocabulary → gap always
"NIST CSF alignment" — alignment/management → gap

---

## E029-002 — NIST SP 800-series controls that fire SECURITY

NIST SP 800-53 AC-2: "privilege escalation" in audit context → SECURITY + AUTHORIZATION
NIST SP 800-53 IA-2: "authentication controls" in audit context → SECURITY (authentication fires)
NIST SP 800-53 SC-8: "TLS / encryption in transit" → SECURITY (TLS/encryption fires)
NIST FIPS 140-2: "cryptographic module" in review → SECURITY (cryptographic fires)

---

## E029-003 — NIST Detect function + scan fires SECURITY

"Scan for anomalous authentication events per NIST Detect" → SECURITY
"Monitor for intrusion events" → borderline; "monitor" alone is weak unless paired with security nouns
Teaching: Detect function vocabulary fires SECURITY only when scan/audit verb present with security nouns.

---

## E029-004 — Risk vocabulary is always gap regardless of NIST context

"Perform risk management activities per NIST CSF" → gap
"Conduct risk assessment per NIST SP 800-30" → gap
"Maintain a risk register" → gap
Teaching: "risk" vocabulary family is always gap even in a NIST security audit context.

---

## E029-005 — Trust state for NIST-referenced audits

"Audit cryptographic modules against FIPS 140-2" → deterministic_generic
"Review authentication events" → probationary_accepted (review + auth, no strong audit verb)
"Align to NIST CSF" → knowledge_edge (management alignment, zero predicates)
