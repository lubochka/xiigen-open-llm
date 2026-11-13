# Layer 0-C — Knowledge-Edge Pairs (022)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-022-001 — "refactor" fires IMPLEMENT with TransitionType: refactoring

A: "Refactor the pricing engine to separate discount calculation from margin calculation."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  TransitionType: refactoring
  Edge analysis: "refactor" → IMPLEMENT fires with TransitionType: refactoring. "separate" — action describing refactoring intent; not a separate predicate trigger. Single IMPLEMENT fires.

---

## B15-kedge-022-002 — "strategy classes" does not fire ARCHITECTURE

A: "Refactor the pricing engine to use distinct strategy classes."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  TransitionType: refactoring
  Edge analysis: "strategy classes" — design pattern noun; does not fire ARCHITECTURE. ARCHITECTURE fires only on: architecture, architect, domain model, bounded context. "strategy pattern" / "strategy classes" are implementation design patterns within IMPLEMENT scope.

---

## B15-kedge-022-003 — "regression tests" fires TEST

A: "Refactor the pricing engine and write regression tests to verify pricing parity."
B:
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  TransitionType: refactoring
  Edge analysis: "write regression tests" → TEST fires. "regression tests" maps to TestingFlow51 (sub-phase 5.1 — unit/regression tests). TransitionType: refactoring propagates alongside TEST.

---

## B15-kedge-022-004 — "pricing parity" does not fire any additional predicate

A: "Verify pricing parity before and after refactor."
B:
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] (assuming "write tests" or "verify" in context)
  Edge analysis: "verify" can fire TEST if in the context of "verify [by testing]". "pricing parity" — domain noun; not a predicate trigger. "verify" alone without "test" context → marginal; "write tests to verify" → TEST fires clearly on "write tests".

---

## B15-kedge-022-005 — "API contracts" with refactoring fires API_CONTRACT

A: "Refactor the pricing engine and update the pricing API contracts to reflect the new strategy interface."
B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  TransitionType: refactoring
  Edge analysis: "refactor" → IMPLEMENT (TransitionType: refactoring); "API contracts" → API_CONTRACT fires. Two predicates. "update the contracts" — update does not suppress API_CONTRACT; "contract" keyword fires regardless of the verb modifier.
