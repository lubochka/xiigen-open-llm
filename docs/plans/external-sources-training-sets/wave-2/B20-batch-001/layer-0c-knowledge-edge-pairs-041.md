# Layer 0-C — Knowledge Edge Pairs (041) — DijkstraPath: gap analysis for path-finding domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E041-001 — Path-finding domain verbs that always produce zero predicates

Execution and domain verbs confirmed as zero-predicate for DijkstraPath context:
  find path, locate route, determine shortest path, compute path, evaluate route — all zero predicates
  run, invoke, execute, trigger, start — execution verbs; all zero predicates
  "Dijkstra", "shortest path", "edge weight" as domain noun phrases — zero predicates

---

## E041-002 — "compute shortest path" does NOT fire IMPLEMENT

"Compute the shortest path between node A and node B using Dijkstra's algorithm." → CompiledPredicateIds: []
"compute" is a domain computation verb. It is NOT in the IMPLEMENT predicate lexicon.
Consistent with all computation verbs across B20: calculate, compute, determine, evaluate, process — all zero.

---

## E041-003 — "negative-weight edges" domain term does NOT fire TEST or SECURITY

"Handle negative-weight edges in the DijkstraPath implementation." → CompiledPredicateIds: []
"handle" → domain operation verb; zero predicates.
"negative-weight edges" → domain term; does not fire TEST or SECURITY.
SECURITY fires on security, authentication, JWT (JSON Web Token), token, encryption, authorization, etc.
Domain-specific terms (negative weight, cycle detection, disconnected graph) do not fire SECURITY.

---

## E041-004 — "run path-finding" confirms execution verb gap for algorithm domain

"Run the path-finding algorithm on the network topology graph." → CompiledPredicateIds: []
"run" is an execution verb. "path-finding algorithm" and "network topology" are domain terms.
Zero predicates confirmed. SCHEDULE_CAPABILITY_LEARNING: true.
