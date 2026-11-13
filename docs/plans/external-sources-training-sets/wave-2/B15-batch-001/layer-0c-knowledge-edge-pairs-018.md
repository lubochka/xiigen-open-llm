# Layer 0-C — Knowledge-Edge Pairs (018)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-018-001 — "index" does not fire additional predicates

A: "Build the search service that indexes products and returns ranked results."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "indexes" — technical verb within IMPLEMENT scope; not a predicate trigger. "returns" — domain action verb; not a predicate trigger. Single IMPLEMENT predicate fires.

---

## B15-kedge-018-002 — "integrate with Elasticsearch" fires API_CONTRACT

A: "Implement the product search service and integrate with the Elasticsearch index API."
B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Edge analysis: "integrate" → API_CONTRACT fires. "Elasticsearch" is a named external system confirming API integration context. API_CONTRACT fires on the verb "integrate" alone.

---

## B15-kedge-018-003 — "extend" fires IMPLEMENT (not a new predicate)

A: "Extend the search service to support faceted filtering by category, price range, and rating."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "extend" → IMPLEMENT fires. TransitionType: feature_addition (extend is a feature-addition verb, not refactoring). "faceted filtering" — domain feature noun; not a predicate trigger.

---

## B15-kedge-018-004 — "refactor" + "replace" fires IMPLEMENT with TransitionType: refactoring

A: "Refactor the search service to replace the in-memory index with a distributed Elasticsearch-backed index."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  TransitionType: refactoring
  Edge analysis: "refactor" → IMPLEMENT fires with TransitionType: refactoring. "replace" — additional context for refactoring intent; does not fire additional predicates.

---

## B15-kedge-018-005 — "ranked results algorithm" does not fire ARCHITECTURE

A: "Build the search service with a ranked results algorithm."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "algorithm" — technical noun within IMPLEMENT scope; does not fire ARCHITECTURE. ARCHITECTURE requires: architecture, architect, domain model, bounded context. "algorithm" is a code construct, not an architectural concept.
