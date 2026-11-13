# Cross-Layer Synthesis (002)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## S002 — End-to-end trace: "Refactor the OrderProcessor and verify the existing tests still pass"

### Layer 0-A (Compiler)
Input: "Refactor the OrderProcessor and verify the existing tests still pass."
  "refactor" → IMPLEMENT | "verify" → TEST | "tests" → TEST (duplicate)
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]

### Layer 0-B (Router)
  IMPLEMENT → SubPhase 4.1 | TransitionType: refactoring
  TEST → SubPhase 5.1
  DecisionSet: [FeatureImplementationDecision(4.1, refactoring), UnitTestingDecision(5.1)]

### Layer 0-D (Factory)
  FlowFactory.CreateMulti(["IMPLEMENT","TEST"]) → [FeatureImplementationFlow41(refactoring), UnitTestingFlow51]
  ExecutionMode: sequential (IMPLEMENT refactors; TEST verifies no regression)

### Layer 1 (Transition Orchestrator)
  TransitionType: refactoring | Primary flow: FeatureImplementationFlow41
  Secondary: UnitTestingFlow51 (regression check)

### Layer 2 (Nine-Node Flow — 5.1 regression check)
  Node 3 plan: Run all existing OrderProcessor tests; no new tests written
  Node 7 gate: All pre-existing tests pass (no regression)
  Result: PASS | Tests run: 8 (existing) | New tests: 0

### Layer 3 (Teaching)
  Lesson: Refactoring context fires IMPLEMENT (TransitionType: refactoring). TEST fires from "verify"+"tests". The refactoring context doesn't suppress TEST.

### Cross-layer invariant
  "refactor" sets TransitionType; never suppresses other predicate flows. IMPLEMENT and TEST flow simultaneously.
