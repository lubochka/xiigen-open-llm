# Layer 0-C — Knowledge Edge Pairs (030) — IRRCalculator: "iterate", "converge" algorithm verb gaps
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E030-001 — Gap: algorithm iteration verbs → knowledge_edge

A: "Iterate the IRR calculation until it converges to within 0.001% tolerance."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["iterate", "converges", "0.001% tolerance"]
  Note: "iterate" and "converge" are numerical algorithm verbs; no predicate mapping.
  Algorithm verb gap class: iterate, converge, initialise, seed, approximate, estimate, refine.

---

## E030-002 — Gap class: Newton-Raphson iteration vocabulary — all zero-predicate

  "Newton-Raphson", "bisection method", "secant method", "convergence threshold",
  "initial guess", "iteration count", "tolerance", "numerical stability",
  "root-finding algorithm", "derivative approximation"
  All are algorithm domain terms. None fire any predicate.

---

## E030-003 — Contrast: algorithm context verbs vs TEST/IMPLEMENT verbs

| Request | CompiledPredicateIds | Source |
|---------|---------------------|--------|
| "Iterate the IRR until convergence" | [] | knowledge_edge |
| "Implement the Newton-Raphson iteration" | ["IMPLEMENT"] | routable → 4.1 |
| "Test the IRR convergence at 0.001%" | ["TEST"] | routable → 5.1 |
| "Build and spec the IRR convergence logic" | ["TEST","IMPLEMENT"] | routable → 4.1 + 5.1 |

The algorithm domain provides no predicate signals. All predicates come from action verbs.
