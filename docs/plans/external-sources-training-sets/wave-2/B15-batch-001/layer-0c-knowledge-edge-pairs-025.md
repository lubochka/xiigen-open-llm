# Layer 0-C — Knowledge-Edge Pairs (025)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-025-001 — "payment" domain noun: no predicate fires

"payment", "billing", "charge", "gateway" — all domain nouns. None fire any predicate.
IMPLEMENT fires from the verb: "build PaymentGatewayService" → "build" is the trigger, not "payment".
v6-01 risk: treating "payment" as an unrecognized predicate and returning [].

---

## B15-kedge-025-002 — "integrate" with external payment provider

"Integrate with the Stripe API" → API_CONTRACT fires.
"Integrate" fires API_CONTRACT regardless of whether the external system is named.
"Integrate with Stripe" and "integrate the components" both fire API_CONTRACT.

---

## B15-kedge-025-003 — "secure the payment endpoints" — verb determines predicate

"secure the payment endpoints" → SECURITY fires on "secure" (verb).
"payment endpoints" is the target — the target does not determine the predicate.
"payment security" (noun phrase only, no verb) → SECURITY does NOT fire.

---

## B15-kedge-025-004 — "domain model" fires ARCHITECTURE in payment context

"payment domain model" → ARCHITECTURE fires (domain noun + "model" phrase).
"architect the domain model" → ARCHITECTURE fires from "architect" verb alone.
Both triggers present → deduplicated to one ARCHITECTURE predicate.

---

## B15-kedge-025-005 — "encrypt the credentials" fires SECURITY

"encrypt the API credentials" → SECURITY fires on "encrypt" verb.
"credentials" is the object of encryption — it does not independently fire SECURITY.
Teaching: SECURITY fires on the verb (secure/harden/protect/encrypt/authenticate), not on the noun.
