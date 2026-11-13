# Held-Out and Corruption Pairs (022)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H022-001 — Held-out: QA+DOCUMENT+SECURITY — compliance release

A: "Document the security controls, write the compliance audit report, and QA review the release checklist."
  Expected: ["QA", "DOCUMENT", "SECURITY"]
  Held-out: "security controls" → SECURITY; "document" → DOCUMENT; "compliance report" → DOCUMENT (dedup); "QA review" → QA. documentation (IMPLEMENT absent).

---

## H022-002 — Held-out: gap — compliance audit scheduling

A: "Schedule the SOC 2 audit with the external auditor for next month."
  Expected: []
  Held-out: "schedule audit" → gap; "external auditor" → gap. knowledge_edge.

---

## H022-003 — Held-out: QA+IMPLEMENT+SECURITY — compliance gate service

A: "Build the compliance gate service, secure the audit trail, and QA approve."
  Expected: ["QA", "IMPLEMENT", "SECURITY"]
  Held-out: "build" → IMPLEMENT; "service" → IMPLEMENT (dedup); "secure" → SECURITY; "QA approve" → QA. feature_addition.

---

## C022-001 — Corruption: "compliance" fires an invented predicate

A: "Ensure compliance with SOC 2 standards."
  Correct: [] — gap
  Corrupt: ["COMPLIANCE"] ← invented predicate (wrong)
  Teaching: "compliance" alone is gap vocabulary. No COMPLIANCE predicate exists.

---

## C022-002 — Corruption: "PII" fires SECURITY without "secure" verb

A: "Handle PII data in the AdminService."
  Correct: [] — "PII" alone is gap; no SECURITY verb present
  Corrupt: ["SECURITY"] ← wrong (PII alone treated as SECURITY trigger)
  Teaching: "PII" alone does not fire SECURITY. "secure the PII endpoints" fires SECURITY via "secure" verb.
