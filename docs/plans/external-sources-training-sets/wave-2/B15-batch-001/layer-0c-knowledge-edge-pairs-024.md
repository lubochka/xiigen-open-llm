# Layer 0-C — Knowledge-Edge Pairs (024)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-024-001 — Complete ARCHITECTURE trigger reference

ARCHITECTURE fires on:
- "architecture" — noun or adjective
- "architect" — verb ("architect the service") or noun ("the software architect")
- "domain model" — noun phrase (any "[X] model" where X is a domain concept)
- "bounded context" — DDD (Domain-Driven Design) term
- "bounded domain" — variant DDD term
Teaching point: "model" must be paired with a domain noun to fire ARCHITECTURE. "data model" fires ARCHITECTURE. "object model" fires ARCHITECTURE. "mental model" does NOT fire ARCHITECTURE (non-domain usage).

---

## B15-kedge-024-002 — Complete API_CONTRACT trigger reference

API_CONTRACT fires on:
- "contract" — with any modifier (data contracts, API contracts, service contracts, message contracts)
- "integrate" — verb
- "wire up" — phrasal verb
- "connect" — verb (when connecting to an external system)
Teaching point: "integrate" without an external system noun is sufficient. "integrate the components internally" still fires API_CONTRACT.

---

## B15-kedge-024-003 — Complete SECURITY trigger reference

SECURITY fires on:
- "secure" — verb
- "harden" — verb
- "protect" — verb
- "encrypt" — verb
- "authenticate" — verb
Teaching point: SECURITY fires on the verb, not the target. "secure the data" fires SECURITY. "data security" as a noun phrase does NOT fire SECURITY (no verb trigger).

---

## B15-kedge-024-004 — TEST routing: 5.1 vs 5.3

TEST at sub-phase 5.1: unit tests, integration tests, regression tests.
TEST at sub-phase 5.3: E2E (End-to-End) tests, end-to-end tests, acceptance tests, flow tests.
Keyword "E2E" or "end-to-end" → routes to 5.3. All other test types → routes to 5.1.

---

## B15-kedge-024-005 — QA routing: 7.1 vs 7.3

QA at sub-phase 7.1: general QA gate, "QA before release", "QA before staging".
QA at sub-phase 7.3: release certification, production deployment gate.
"release to staging" vocabulary → 7.1. "release to production" / "production release" vocabulary → 7.3.
"QA" abbreviation alone with no release context → defaults to 7.1.
