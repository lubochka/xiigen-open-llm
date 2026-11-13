# Held-Out and Corruption Pairs (020)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: held_out_corruption

## HC-020-A — Held-out: AuditService five predicates

A: "Design the audit domain model, build the audit service, secure the log, write tamper-detection tests, and document the audit API."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"] | feature_addition | DecisionCount: 5

## HC-020-B — Held-out: "append-only" does not fire SECURITY

A: "Build the audit service with append-only storage."
B: Predicates: ["IMPLEMENT"] | "append-only" is a storage pattern noun — not a SECURITY verb trigger.

## HC-020-C — Held-out: "secure" fires SECURITY for audit log

A: "Build the audit service and secure the audit log against tampering."
B: Predicates: ["IMPLEMENT", "SECURITY"] | "secure" → SECURITY fires.

## HC-020-D — Corruption v6-02: "audit model" not caught

v6 corrupt: "Build the audit service and define the domain model for audit events" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | "domain model" fires ARCHITECTURE.
Error: v6-02.

## HC-020-E — Corruption v6-01: "tamper" suppresses SECURITY

v6 corrupt (inverse): "Implement the audit service and write tamper-detection tests" → Predicates: ["IMPLEMENT", "SECURITY"] ("tamper" fires SECURITY)
Correct: Predicates: ["IMPLEMENT", "TEST"] | "tamper-detection tests" → TEST fires on "write" + "tests"; "tamper" alone does not fire SECURITY (only secure/harden/protect/encrypt/authenticate fire SECURITY).
Error: v6-02 inverse (spurious SECURITY from non-trigger noun).
