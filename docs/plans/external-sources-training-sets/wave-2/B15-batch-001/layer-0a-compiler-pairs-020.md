# Layer 0-A — Compiler Pairs (020)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-020-001 — AuditService: "create" → IMPLEMENT

A: "Create the audit service that logs every write operation on orders, payments, and customer data."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Sub-phase: 4.1
Evidence: create → IMPLEMENT; audit service → backend service noun; logs → domain action, not predicate trigger.

---

## B15-compiler-020-002 — AuditService: "secure" → IMPLEMENT+SECURITY

A: "Build the audit service and secure the audit log against tampering using append-only storage and HMAC (Hash-based Message Authentication Code) signatures."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; secure → SECURITY (HMAC-based tamper protection is a security concern). First use of abbreviation: HMAC (Hash-based Message Authentication Code).

---

## B15-compiler-020-003 — AuditService: "harden" → IMPLEMENT+SECURITY

A: "Implement the audit logging service and harden the audit trail against unauthorized deletion."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; harden → SECURITY (harden fires SECURITY same as secure/protect).

---

## B15-compiler-020-004 — v6-02 corruption: "audit model" → ARCHITECTURE suppressed

v6 corrupt: "Build the audit service and define the domain model for audit events" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["IMPLEMENT", "ARCHITECTURE"]
Error type: v6-02. "domain model" fires ARCHITECTURE. The corrupt model treated "audit model" as domain jargon and suppressed ARCHITECTURE.

---

## B15-compiler-020-005 — AuditService: "write tests" + "secure" → IMPLEMENT+SECURITY+TEST

A: "Implement the audit service, secure the log endpoints, and write unit tests for the tamper-detection logic."
B: Predicates: ["IMPLEMENT", "SECURITY", "TEST"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; secure → SECURITY; write unit tests → TEST. Three independent predicates, no deduplication needed.
