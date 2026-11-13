# Cross-Layer Synthesis (001)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## S001 — End-to-end trace: "Write unit tests for the PaymentService and validate the output schema"

### Layer 0-A (Compiler)
Input: "Write unit tests for the PaymentService and validate the output schema."
  "write" → IMPLEMENT | "unit tests" → TEST | "validate" → TEST (duplicate) | "schema" alone → gap
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]

### Layer 0-B (Router)
  TEST → SubPhase 5.1 | IMPLEMENT → SubPhase 4.1 | TransitionType: feature_addition
  DecisionSet: [UnitTestingDecision(5.1), FeatureImplementationDecision(4.1)]

### Layer 0-D (Factory)
  FlowFactory.CreateMulti(["IMPLEMENT","TEST"]) → [FeatureImplementationFlow41, UnitTestingFlow51]
  ExecutionMode: sequential (IMPLEMENT provides code; TEST validates it)

### Layer 1 (Transition Orchestrator)
  TransitionType: feature_addition | Phase transition: → [4.1, 5.1]

### Layer 2 (Nine-Node Flow — 5.1)
  Service: PaymentService | Tests: PaymentValidationTests.cs
  Tests produced: 10 | Result: PASS

### Layer 3 (Teaching)
  Lesson: "validate" fires TEST in output-schema context — same as in data validation. Universal rule confirmed.

### Cross-layer invariant
  "schema" alone is gap; "validate" always fires TEST. Predicate comes from verb, not noun.
