# Held-Out and Corruption Pairs
# Batch: B17-batch-001
# Scenario: B17 — Development — integration
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## Held-Out B17B1-HO-001 — "connector" + "build" → 4.3

A: "Build a connector to the Salesforce CRM API for syncing customer records."
Expected: IMPLEMENT (build) + API_CONTRACT (connector, api) → ["IMPLEMENT", "API_CONTRACT"].
Routing: 4.3 + 3.3.

---

## Held-Out B17B1-HO-002 — "integrate" only → 3.3, not 4.3

A: "Integrate the Twilio SMS API for sending notification alerts."
Expected: API_CONTRACT fires on "integrate" and "api"; IMPLEMENT does NOT fire (no build/write/implement/create/code/function/class).
CompiledPredicateIds: ["API_CONTRACT"]. Routing: 3.3 only (not 4.3).

---

## Held-Out B17B1-HO-003 — webhook + implement → 4.3

A: "Implement the webhook listener for GitHub events and handle push and pull request payloads."
Expected: IMPLEMENT (implement, handler), API_CONTRACT (webhook). CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"].
Routing: 4.3 + 3.3.

---

## Corruption B17B1-COR-001 — v6-01: "integrate" misclassified as IMPLEMENT

CORRUPT: "Integrate the SendGrid email API." → CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"].
Why corrupt: "integrate" fires API_CONTRACT, NOT IMPLEMENT. No IMPLEMENT term present.
Correct: CompiledPredicateIds: ["API_CONTRACT"]. Routing: 3.3 only.

---

## Corruption B17B1-COR-002 — v6-01: SECURITY suppressed for "token" in OAuth request

CORRUPT: "Build the OAuth connector and implement the token exchange." → CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"].
Why corrupt: "token" fires SECURITY and "oauth" fires AUTHORIZATION. Both are suppressed.
Correct: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"].

---

## Corruption B17B1-COR-003 — v6-03: IMPLEMENT routes to 4.1 instead of 4.3

CORRUPT: "Build the Stripe payment integration connector." → IMPLEMENT → 4.1 (Backend Services).
Why corrupt: Integration-context vocabulary (connector, integration, webhook) signals 4.3.
The router must recognize that IMPLEMENT in an integration context routes to 4.3, not 4.1.
Correct: IMPLEMENT → 4.3.
