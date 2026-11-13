# Layer 0-D — Flow Factory Pairs (026) — CompoundInterestAlgorithm: deduplication invariant in factory
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F026-001 — Deduplication: three TEST triggers → one UnitTestingFlow51 instance

Request: "Create unit tests, write test cases, and assert the CompoundInterestAlgorithm handles zero principal."
  CompiledPredicateIds after deduplication: ["TEST", "IMPLEMENT"]
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  Invariant: one FlowInstance per unique predicate — never more, regardless of how many triggers fired.

---

## F026-002 — "spec" alone: factory produces UnitTestingFlow51, TransitionType documentation

Request: "Spec out test cases for CompoundInterestAlgorithm compounding frequencies."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation. One instance.

---

## F026-003 — Factory invariant reinforcement: never collapse two instances into one

Anti-pattern: FlowFactory.CreateMulti(["TEST","IMPLEMENT"]) → [UnitTestingFlow51] only (v6-03 error).
Correct: always produces [BackendServiceFlow41, UnitTestingFlow51] — two separate instances.
Rule: FlowFactory.CreateMulti always returns exactly one FlowInstanceDescriptor per unique predicate.
