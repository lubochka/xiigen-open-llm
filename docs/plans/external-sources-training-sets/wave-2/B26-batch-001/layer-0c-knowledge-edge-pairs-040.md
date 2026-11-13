# Layer 0-C — Knowledge Edge Pairs (040) — XXEAudit Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E040-001 — XXE solo noun fires SECURITY

"XXE" → SECURITY (attack vector solo noun, always fires)
"external entity" in XML context → SECURITY (contextual qualifier: XML + external entity = XXE)
"parse XML" alone → gap (no security qualifier)
"disable external entity processing" → SECURITY (harden action in security context)

---

## E040-002 — DTD processing boundary

"DTD processing is disabled" → gap if no audit action verb
"audit whether DTD processing is disabled" → SECURITY (audit verb present)
"harden parsers by disabling DTD" → SECURITY (harden = security action)
Teaching: Statement of configuration state is gap; auditing or hardening fires SECURITY.

---

## E040-003 — XXE-to-SSRF chaining

"XXE-to-SSRF chaining" → SECURITY (two attack-vector triggers)
TrustState: deterministic_generic when both XXE and SSRF present
Neither SSRF nor XXE alone requires co-occurrence; each fires independently.

---

## E040-004 — XML vocabulary disambiguation

"XML schema validation" → gap (schema validation is not an attack vector audit)
"XML external entity processing" → SECURITY (external entity = XXE context)
"audit XML parser configuration for XXE" → SECURITY (audit + XXE)

---

## E040-005 — Trust state calibration

XXE alone + audit → deterministic_generic
"parse XML" alone → gap (zero predicates)
XXE + SSRF chaining audit → deterministic_generic (high, two attack-vector triggers)
