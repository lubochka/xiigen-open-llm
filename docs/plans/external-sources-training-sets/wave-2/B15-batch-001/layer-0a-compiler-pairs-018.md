# Layer 0-A — Compiler Pairs (018)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-018-001 — SearchService: "build" IMPLEMENT alone

A: "Build the search service that indexes products and returns ranked results for keyword queries."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Sub-phase: 4.1
Evidence: build → IMPLEMENT; search service → backend service noun; indexes/returns → domain action verbs, not predicate triggers.

---

## B15-compiler-018-002 — SearchService: "implement" + "integrate" → IMPLEMENT+API_CONTRACT

A: "Implement the product search service and integrate with the Elasticsearch index API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; integrate → API_CONTRACT (integrate/wire up/connect fires API_CONTRACT).

---

## B15-compiler-018-003 — SearchService: "extend" → IMPLEMENT (TransitionType: feature_addition)

A: "Extend the search service to support faceted filtering by category, price range, and rating."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: extend → IMPLEMENT (extend is in IMPLEMENT lexicon); search service → backend service. TransitionType remains feature_addition because "extend" without "refactor" does not trigger refactoring.

---

## B15-compiler-018-004 — v6-03 corruption: IMPLEMENT+API_CONTRACT collapsed to IMPLEMENT

v6 corrupt: "Implement the search service and integrate with Elasticsearch" → Predicates: ["IMPLEMENT"] (API_CONTRACT lost)
Correct: Predicates: ["IMPLEMENT", "API_CONTRACT"]
Error type: v6-03. "integrate" fires API_CONTRACT independently. The corrupt model collapsed the two predicates to the "dominant" IMPLEMENT. Both must fire.

---

## B15-compiler-018-005 — SearchService: "refactor" → IMPLEMENT with TransitionType: refactoring

A: "Refactor the search service to replace the in-memory index with a distributed Elasticsearch-backed index."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
Evidence: refactor → IMPLEMENT (refactor is in IMPLEMENT lexicon); TransitionType: refactoring overrides feature_addition when "refactor" verb present.
