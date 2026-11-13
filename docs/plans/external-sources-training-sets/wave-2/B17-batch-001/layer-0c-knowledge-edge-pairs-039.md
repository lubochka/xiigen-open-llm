# Layer 0-C — Knowledge-Edge Pairs (039)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: PagerDuty connector

---

## B17-0C-039-001 — PagerDuty: non-trigger terms

**Proper nouns — no predicate:** PagerDuty
**Domain nouns — no predicate:** incident, alert, escalation, on-call, schedule, service, policy, team, acknowledge, resolve, trigger (when used as noun/verb for incident creation)
**Acronyms — no predicate:** SRE (Site Reliability Engineering), PD (abbreviation for PagerDuty)

---

## B17-0C-039-002 — PagerDuty: "trigger" as incident action is not IMPLEMENT

"Trigger an alert in PagerDuty" uses "trigger" as a domain verb (create/send incident). This is NOT in the IMPLEMENT trigger list. Only "trigger" as a compound with explicit build vocabulary matters if IMPLEMENT is to fire.

---

## B17-0C-039-003 — PagerDuty: REST API key does not fire SECURITY

"PagerDuty REST API key" alone → no SECURITY.
"PagerDuty API token" → SECURITY fires.
"PagerDuty user token" → SECURITY fires.
Same rule as all other connectors: the word "token" must be present.

---

## B17-0C-039-004 — PagerDuty: webhook payload HMAC fires SECURITY

"Validate the PagerDuty webhook event payload using HMAC-SHA256" → SECURITY fires (Rule 8: HMAC validation phrase).
"Verify webhook authenticity" alone → SECURITY does NOT fire (no token, no HMAC-SHA256 phrase, no "authenticate").
