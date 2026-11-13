# Layer 0-C — Knowledge-Edge Pairs (040)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-040-001 — ComplianceService: ARCHITECTURE triggers

Confirmed ARCHITECTURE triggers:
- "compliance domain model" → ARCHITECTURE
- "regulatory model" → ARCHITECTURE
- "bounded context" (compliance bounded context) → ARCHITECTURE
- "architect the compliance rule engine" → ARCHITECTURE

---

## B15-kedge-040-002 — ComplianceService: API_CONTRACT triggers

Confirmed API_CONTRACT triggers:
- "compliance API contracts" → API_CONTRACT
- "integrate with the regulatory reporting API" → API_CONTRACT
- "wire up the external compliance check service" → API_CONTRACT

---

## B15-kedge-040-003 — ComplianceService: SECURITY triggers

Confirmed SECURITY triggers:
- "secure all audit log endpoints" → SECURITY
- "encrypt the compliance record store" → SECURITY
- "protect the rule configuration endpoint" → SECURITY

---

## B15-kedge-040-004 — "enforce" does NOT fire SECURITY (confirmed)

"enforce compliance rules" → IMPLEMENT only.
SECURITY fires from: secure, harden, protect, encrypt, authenticate.
v6-02 risk: "enforce" triggering SECURITY is an inverse corruption — spurious predicate added incorrectly.

---

## B15-kedge-040-005 — ComplianceService: DOCUMENT trigger

"document the regulatory rule catalog" → DOCUMENT.
"spec out the compliance rule schema" → DOCUMENT.
"write compliance runbook" → DOCUMENT.
DOCUMENT fires from: document, spec out, write docs, write documentation, runbook.
