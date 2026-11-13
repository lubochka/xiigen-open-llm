# Cross-Layer Synthesis (005)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Topic: TEST alone for cart calculation — documentation TransitionType end-to-end trace
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## S005 — End-to-end trace: "Assert that the CartCalculationService returns zero for an empty cart"

### Layer 0-A (Compiler)
Input: "Assert that the CartCalculationService returns zero for an empty cart."
  "assert" → TEST | "CartCalculationService" — domain, no predicate | "returns zero" — domain value, gap
  CompiledPredicateIds: ["TEST"]
  Note: No IMPLEMENT verb present. TEST alone.

### Layer 0-B (Router)
  TEST → SubPhase 5.1 | TransitionType: documentation
  Single routing decision: [UnitTestingDecision(5.1, documentation)]

### Layer 0-C (Knowledge Edge)
  No gap signal raised — "assert" is canonical TEST vocabulary; request is routable.

### Layer 0-D (Factory)
  FlowFactory.CreateSingle("TEST") → [UnitTestingFlow51]
  One flow instance. No IMPLEMENT companion.

### Layer 1 (Transition Orchestrator)
  TransitionType: documentation
  Evidence: "assert" fires TEST only; no construction verb present; specifying expected empty-cart behaviour.
  SubPhaseId: 5.1

### Layer 2 (Nine-Node Flow — 5.1)
  Node 01 (DomainSplitterUnit): splits into "cart_aggregate" domain (CalculateSubtotal with zero items)
  Node 03 (SchemePlannerUnit): test method = "CalculateSubtotal_EmptyCart_ReturnsZero"
  Node 05 (ApplierUnit): 1 test produced
  Node 07 (ArbiterSelectorUnit): PASS
  Node 09 (FormatterUnit): Result: PASS | Tests: 1 | Status: green

### Layer 3 (Teaching)
  Lesson: "assert" alone fires TEST with TransitionType documentation. One predicate → one flow → one test.
  No IMPLEMENT fires from "assert". The test spec is the artefact; no construction occurs.

### Cross-layer invariant
  TEST alone (via assert/verify/validate/spec) → documentation TransitionType throughout all layers.
  No IMPLEMENT fires unless a construction verb (write, create, build, develop) is also present.
  The TransitionType is consistent from compiler output through orchestrator through flow verdict.
