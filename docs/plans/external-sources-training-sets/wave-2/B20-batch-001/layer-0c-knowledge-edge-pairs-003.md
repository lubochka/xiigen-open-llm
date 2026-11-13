# Layer 0-C — Knowledge Edge Pairs (003)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E003-001 — "stub" alone is gap

A: "Stub the external payment gateway for local development."
  "stub" — gap (test infrastructure term without test action verb); "local development" — gap
  CompiledPredicateIds: [] → knowledge_edge
  Gap: gap-testing-mock-setup-001

---

## E003-002 — "spy" alone is gap

A: "Create a spy on the NotificationService to track call counts."
  "create" → IMPLEMENT; "spy" — gap; "track" — gap; "call counts" — testing terminology; gap
  CompiledPredicateIds: ["IMPLEMENT"] (via "create" only)
  Teaching: "spy" is gap. IMPLEMENT fires via "create" but there is no TEST predicate.

---

## E003-003 — "TDD" abbreviation is gap; "test" in "TDD" process description does not fire

A: "Follow TDD to develop the CartService — red, green, refactor."
  "TDD" — gap (abbreviation); "develop" → IMPLEMENT; "red, green, refactor" — TDD cycle; gap
  CompiledPredicateIds: ["IMPLEMENT"] (via "develop")
  Teaching: TDD as an abbreviation is gap. IMPLEMENT fires via "develop". TEST does not fire.

---

## E003-004 — "coverage report" is gap

A: "Run the coverage report for the entire test project."
  "run" — gap; "coverage report" — gap; "test project" → TEST fires on "test"
  CompiledPredicateIds: ["TEST"] (weak via "test" in "test project")
  Teaching: "coverage report" is gap. TEST fires weakly via "test project". "run" is gap.

---

## E003-005 — "make it testable" is gap

A: "Refactor the PaymentProcessor to make it testable by extracting dependencies."
  "refactor" → IMPLEMENT (TransitionType: refactoring); "make it testable" — gap;
  "extracting" → IMPLEMENT (extract is a refactoring IMPLEMENT term); "dependencies" — domain
  CompiledPredicateIds: ["IMPLEMENT"] — TEST does NOT fire on "testable" (adjective, not noun/verb).
  TransitionType: refactoring (not feature_addition — "refactor" signals refactoring)
