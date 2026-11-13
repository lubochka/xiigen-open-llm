# Held-Out and Corruption Pairs (019)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: held_out_corruption

## HC-019-A — Held-out: NotificationService three predicates

A: "Build the notification service, wire up the email API, and secure the SMS dispatch tokens."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] | feature_addition | DecisionCount: 3

## HC-019-B — Held-out: deduplication with three API triggers

A: "Implement the notification service, integrate with email API, integrate with SMS API, and connect to push gateway."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | DecisionCount: 2 | Three API triggers → one API_CONTRACT after deduplication.

## HC-019-C — Corruption v6-03: deduplication not applied

v6 corrupt: "Implement notification service, integrate with email API, integrate with SMS API, connect to push gateway" → Predicates: ["IMPLEMENT", "API_CONTRACT", "API_CONTRACT", "API_CONTRACT"] or DecisionCount: 4
Correct: Predicates: ["IMPLEMENT", "API_CONTRACT"] | DecisionCount: 2
Error: v6-03 variant — failure to deduplicate same-predicate triggers. Deduplication is per predicate — three API_CONTRACT triggers = one API_CONTRACT predicate.

## HC-019-D — Corruption v6-02: "dispatch" fires API_CONTRACT (inverse error)

v6 corrupt (inverse): "Build the notification service that dispatches email" → Predicates: ["IMPLEMENT", "API_CONTRACT"] ("dispatch" fires API_CONTRACT)
Correct: Predicates: ["IMPLEMENT"] | "dispatch" does not fire API_CONTRACT. Only integrate/wire up/connect fire API_CONTRACT.
Error: v6-02 inverse (spurious API_CONTRACT added for non-trigger verb).

## HC-019-E — Held-out: NotificationService four predicates

A: "Build the notification service, integrate with messaging APIs, secure the dispatch tokens, and document the notification API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "DOCUMENT"] | feature_addition | DecisionCount: 4
