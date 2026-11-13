# Layer 0-C — Knowledge-Edge Pairs (041)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-041-001 — ConsentService: ARCHITECTURE triggers

Confirmed ARCHITECTURE triggers:
- "consent domain model" → ARCHITECTURE
- "bounded context" (consent bounded context) → ARCHITECTURE
- "architect the consent state machine" → ARCHITECTURE

---

## B15-kedge-041-002 — ConsentService: API_CONTRACT triggers

Confirmed API_CONTRACT triggers:
- "consent service contracts" → API_CONTRACT
- "integrate with the preference management API" → API_CONTRACT
- "wire up the GDPR consent API" → API_CONTRACT

---

## B15-kedge-041-003 — ConsentService: SECURITY triggers

Confirmed SECURITY triggers:
- "protect the consent record modification endpoint" → SECURITY
- "secure the consent withdrawal endpoint" → SECURITY
- "encrypt the consent audit log" → SECURITY

---

## B15-kedge-041-004 — "GDPR" and "opt-in" do NOT fire SECURITY

"GDPR", "opt-in", "opt-out", "consent record" are domain nouns.
None fire SECURITY. SECURITY fires only from action verbs.
v6-02 risk: "GDPR" triggering SECURITY is an inverse corruption.

---

## B15-kedge-041-005 — ConsentService: DOCUMENT trigger

"spec out the consent change event schema" → DOCUMENT.
"document the consent withdrawal API" → DOCUMENT.
"write consent management runbook" → DOCUMENT.
