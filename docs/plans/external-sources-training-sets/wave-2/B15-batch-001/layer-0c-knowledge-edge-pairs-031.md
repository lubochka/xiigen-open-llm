# Layer 0-C — Knowledge-Edge Pairs (031)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-031-001 — "wallet" domain noun: no predicate fires

"wallet", "balance", "account balance" are domain nouns — no predicates fire from these nouns.
"build WalletService" → "build" fires IMPLEMENT.

---

## B15-kedge-031-002 — "bounded context" fires ARCHITECTURE

"define the wallet bounded context" → ARCHITECTURE fires on "bounded context".
"bounded context" is a DDD (Domain-Driven Design) term and a confirmed ARCHITECTURE trigger.
The subject "wallet" modifies the context but does not change the predicate.

---

## B15-kedge-031-003 — "protect all balance modification operations" fires SECURITY

"protect all balance modification operations" → SECURITY fires on "protect".
"balance modification operations" is the object of protection — not a separate trigger.

---

## B15-kedge-031-004 — "create integration tests" fires TEST (not IMPLEMENT)

"create integration tests for the deposit flow" → TEST fires.
"create" would normally fire IMPLEMENT — exception: when "create" is immediately followed by a test artifact ("tests", "unit tests", "integration tests") it fires TEST instead.
Teaching: "create tests" = TEST. "create service" = IMPLEMENT.

---

## B15-kedge-031-005 — "harden the withdrawal endpoint" fires SECURITY

"harden the withdrawal endpoint" → SECURITY fires on "harden".
"withdrawal endpoint" is the target, not a trigger.
Complete verb list confirmed: secure / harden / protect / encrypt / authenticate → all fire SECURITY.
