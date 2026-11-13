# Held-Out and Corruption Pairs (024)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: held_out_corruption

## HC-024-A — Complete v6 corruption type reference for B15 (013-024 extension)

| Type | Trigger | Example from 013-024 |
|------|---------|----------------------|
| v6-01: suppress | IMPLEMENT suppressed from domain noun | "chargeback" → [] (014); "fulfillment" → [] (013) |
| v6-01: metadata loss | TransitionType lost | "refactor pricing engine" → feature_addition instead of refactoring (022) |
| v6-02: suppress secondary | ARCHITECTURE/API_CONTRACT not caught | "bounded context" → ARCHITECTURE missing (015); "architect" → ARCHITECTURE missing (023) |
| v6-02: inverse | Spurious predicate added | "moderate" → SECURITY added (017); "dispatch" → API_CONTRACT added (019); "tamper" → SECURITY added (020) |
| v6-03: collapse | Multi-predicate collapsed to one | Three decisions → one (021); four decisions → one (016) |
| v6-03: no dedup | Same predicate not deduplicated | Three API triggers → three API_CONTRACT entries (019) |

## HC-024-B — Held-out: full B15 multi-service pipeline

A: "Build the order fulfillment service, integrate with the carrier API, secure the dispatch endpoint, write fulfillment tests, and document the fulfillment workflow."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT"] | feature_addition | DecisionCount: 5

## HC-024-C — Held-out: "develop" fires IMPLEMENT

A: "Develop the tax calculation module."
B: Predicates: ["IMPLEMENT"] | "develop" → IMPLEMENT fires (develop is in IMPLEMENT lexicon same as build/create/implement).

## HC-024-D — Held-out: "add" (code artifact) fires IMPLEMENT

A: "Add a chargeback dispute handler to the refund service."
B: Predicates: ["IMPLEMENT"] | "add" (code artifact) → IMPLEMENT fires. feature_addition TransitionType.

## HC-024-E — Held-out: complete B15 no-gap confirmation

All requests in B15 (001-024) fire at least IMPLEMENT. GapHandler is never triggered. The B15 scenario involves an active development request in all cases — "no capability gap" means every request has a valid predicate resolution.
