# Layer 3 — Teaching Unit Pairs (020)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: teaching_unit

## TU-020-A — "append-only" alone does not fire SECURITY; "secure" does

A: "Build the audit service with append-only storage." Does SECURITY fire?
B: No. "append-only" is a storage pattern — not a SECURITY trigger. SECURITY fires only on verbs: secure, harden, protect, encrypt, authenticate. Only ["IMPLEMENT"] fires.

## TU-020-B — "harden" fires SECURITY

A: "Build the audit service and harden the audit trail against unauthorized deletion." How many predicates?
B: Two: ["IMPLEMENT", "SECURITY"]. "harden" → SECURITY fires. Equivalent to "secure" in predicate firing strength.

## TU-020-C — HMAC (Hash-based Message Authentication Code) context with "secure"

A: "Build the audit service and secure the audit log with HMAC signatures." How many predicates?
B: Two: ["IMPLEMENT", "SECURITY"]. "secure" → SECURITY fires. HMAC (Hash-based Message Authentication Code) is a technical noun describing the security mechanism. The trigger is "secure" — HMAC reinforces context but is not itself a trigger.

## TU-020-D — "write tamper-detection tests" fires TEST not SECURITY

A: "Implement the audit service and write unit tests for the tamper-detection logic." How many predicates?
B: Two: ["IMPLEMENT", "TEST"]. "write unit tests" → TEST fires. "tamper-detection logic" is the test subject — not a SECURITY trigger because no security verb (secure/harden/protect) is present in this request.

## TU-020-E — Audit service: IAuditRepository append-only

A: The audit repository must be append-only. How is this enforced?
B: IAuditRepository exposes only WriteEntry(AuditEntry) → OperationResult<AuditEntryId>. No Update or Delete methods are defined on the interface. The fabric-first pattern ensures no concrete implementation can expose unauthorized mutations — callers depend only on IAuditRepository which has no mutating methods beyond append.
