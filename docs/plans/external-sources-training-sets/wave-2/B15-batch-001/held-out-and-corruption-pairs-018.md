# Held-Out and Corruption Pairs (018)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: held_out_corruption

## HC-018-A — Held-out: SearchService refactoring + API_CONTRACT

A: "Refactor the search service and update the Elasticsearch API integration contracts."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: refactoring | DecisionCount: 2

## HC-018-B — Held-out: "extend" fires IMPLEMENT with feature_addition

A: "Extend the search service to support faceted filtering."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | "extend" → IMPLEMENT; feature_addition (not refactoring).

## HC-018-C — Corruption v6-01: TransitionType: refactoring lost

v6 corrupt: "Refactor the search service" → Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Correct: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
Error: v6-01 metadata loss. TransitionType is part of the compiled output — it must be set to refactoring when "refactor" verb is present.

## HC-018-D — Corruption v6-03: refactoring + API_CONTRACT collapsed

v6 corrupt: "Refactor the search service and integrate with Elasticsearch API" → DecisionCount: 1 (only ImplementationFlow41)
Correct: DecisionCount: 2 — ImplementationFlow41 (refactoring) + APIContractFlow33
Error: v6-03 collapse.

## HC-018-E — Held-out: "index" does not add predicates

A: "Build the search service that indexes products and returns ranked keyword results."
B: Predicates: ["IMPLEMENT"] | "indexes" and "ranked results" are domain action verbs within IMPLEMENT scope. No additional predicates fire.
