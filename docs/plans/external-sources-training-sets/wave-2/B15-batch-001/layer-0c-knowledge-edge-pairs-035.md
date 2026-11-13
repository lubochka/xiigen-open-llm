# Layer 0-C — Knowledge-Edge Pairs (035)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-035-001 — "chargeback" is a domain noun: must not suppress IMPLEMENT

"chargeback" is a financial domain noun — it does NOT suppress IMPLEMENT.
"implement ChargebackService" → "implement" fires IMPLEMENT; "chargeback" adds zero predicates.
v6-01 corruption: treating "chargeback" as unrecognized and returning [].

---

## B15-kedge-035-002 — "authenticate all dispute resolution requests" fires SECURITY

"authenticate all dispute resolution requests" → SECURITY fires on "authenticate".
"dispute resolution requests" is the object of authentication — no additional predicates.

---

## B15-kedge-035-003 — "wire up the payment processor dispute API" fires API_CONTRACT

"wire up the payment processor dispute API" → API_CONTRACT fires on "wire up".
"payment processor" and "dispute API" are the integration targets — "wire up" is the trigger.

---

## B15-kedge-035-004 — "chargeback API contracts" fires API_CONTRACT

"define the chargeback API contracts" → API_CONTRACT fires on "contracts".
"chargeback" is the domain modifier — "API contracts" is the trigger.

---

## B15-kedge-035-005 — "dispute resolution domain model" fires ARCHITECTURE

"architect the dispute resolution domain model" → ARCHITECTURE fires from "architect" verb.
"dispute resolution domain model" → also ARCHITECTURE (domain noun + model phrase).
Two triggers from one clause → deduplicated to one ARCHITECTURE predicate.
