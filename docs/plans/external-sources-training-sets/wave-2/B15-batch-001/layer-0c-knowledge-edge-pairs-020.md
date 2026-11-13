# Layer 0-C — Knowledge-Edge Pairs (020)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-020-001 — "audit log" is a domain noun, not a predicate trigger

A: "Create the audit service that logs every write operation on orders, payments, and customer data."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "audit log" — domain noun; "logs" — action verb in IMPLEMENT scope; "write operation" — technical noun; none trigger additional predicates. "create" fires IMPLEMENT.

---

## B15-kedge-020-002 — "append-only storage" does not fire SECURITY alone

A: "Build the audit service with append-only storage."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "append-only storage" — technical pattern noun; does not fire SECURITY. SECURITY fires only when a security verb (secure, harden, protect, encrypt, authenticate) is present.

---

## B15-kedge-020-003 — "secure" + "append-only" fires SECURITY

A: "Build the audit service and secure the audit log against tampering using append-only storage."
B:
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  Edge analysis: "secure" → SECURITY fires. "tamper" context reinforces security intent but is not itself the trigger. The verb "secure" alone fires SECURITY.

---

## B15-kedge-020-004 — "HMAC signatures" with "secure" fires SECURITY

A: "Build the audit service and secure the audit log with HMAC (Hash-based Message Authentication Code) signatures."
B:
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  Edge analysis: "secure" → SECURITY fires. "HMAC" (Hash-based Message Authentication Code) — abbreviation expanded on first use per Rule 9. Technical security mechanism noun; the verb "secure" is the trigger.

---

## B15-kedge-020-005 — "tamper-detection" with "write tests" fires TEST

A: "Implement the audit service and write unit tests for the tamper-detection logic."
B:
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Edge analysis: "write unit tests" → TEST fires. "tamper-detection logic" is the test subject; it does not fire SECURITY because no security verb (secure/harden/protect) is present in this request — only "write tests" for it.
