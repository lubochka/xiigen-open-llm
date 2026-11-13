# Layer 3 — Teaching Unit Pairs (018)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: teaching_unit

## TU-018-A — "extend" is IMPLEMENT with feature_addition TransitionType

A: "Extend the search service to support faceted filtering." What predicate and TransitionType?
B: Predicate: IMPLEMENT. TransitionType: feature_addition. "extend" is in the IMPLEMENT lexicon. It does not trigger refactoring (only "refactor" triggers refactoring). "Extend" = adding new capability to existing code → feature_addition.

## TU-018-B — "refactor" overrides feature_addition even with "replace"

A: "Refactor the search service to replace the in-memory index with Elasticsearch." What TransitionType?
B: TransitionType: refactoring. "refactor" verb is present → overrides feature_addition. "replace" is an additional context word describing the nature of the refactor but is not itself a TransitionType trigger.

## TU-018-C — "index" verb does not fire additional predicates

A: "Build the search service that indexes products." How many predicates?
B: One: ["IMPLEMENT"]. "indexes" — technical action verb within IMPLEMENT scope. Not in any other predicate lexicon. Single IMPLEMENT fires.

## TU-018-D — "integrate with Elasticsearch" fires API_CONTRACT

A: "Implement the product search service and integrate with the Elasticsearch index API." How many predicates?
B: Two: ["IMPLEMENT", "API_CONTRACT"]. "integrate" → API_CONTRACT fires. Elasticsearch is an external named system — confirms integration context.

## TU-018-E — TF-IDF does not fire any predicate

A: "Build the search service using TF-IDF (Term Frequency-Inverse Document Frequency) ranking." How many predicates?
B: One: ["IMPLEMENT"]. TF-IDF (Term Frequency-Inverse Document Frequency) is a technical algorithm acronym — not a predicate trigger. Only "build" fires IMPLEMENT.
