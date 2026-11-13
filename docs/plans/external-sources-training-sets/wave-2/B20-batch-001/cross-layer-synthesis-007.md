# Cross-Layer Synthesis (007)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Topic: TEST+IMPLEMENT for fraud score — feature_addition end-to-end trace
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## S007 — End-to-end trace: "Write unit tests for the FraudScoreCalculator and implement the test doubles for the external API"

### Layer 0-A (Compiler)
Input: "Write unit tests for the FraudScoreCalculator and implement the test doubles for the external risk-scoring API."
  "write" → IMPLEMENT | "unit tests" → TEST | "implement" → IMPLEMENT (deduplicates) | "test doubles" → TEST (deduplicates)
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  Note: "write" and "implement" both fire IMPLEMENT; deduplication keeps one. "unit tests" and "test doubles" both fire TEST; deduplication keeps one.

### Layer 0-B (Router)
  TEST → SubPhase 5.1 | IMPLEMENT → SubPhase 4.1 | TransitionType: feature_addition
  DecisionSet: [UnitTestingDecision(5.1), FeatureImplementationDecision(4.1)]

### Layer 0-C (Knowledge Edge)
  No gap signal — both predicates are canonical; request is fully routable.

### Layer 0-D (Factory)
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [UnitTestingFlow51, BackendServiceFlow41]
  Two flow instances; executed in sequence: BackendServiceFlow41 (builds test doubles) → UnitTestingFlow51 (writes tests using them).

### Layer 1 (Transition Orchestrator)
  TransitionType: feature_addition
  Evidence: "write" + "implement" = construction; "unit tests" + "test doubles" = new test artifacts; net-new.
  PhaseId: 5 | PrimarySubPhaseId: 5.1 | AllSubPhaseIds: [5.1, 4.1]

### Layer 2 (Nine-Node Flow — 5.1)
  Service: FraudScoreCalculatorTests
  Tests produced: 7 (score computation, velocity check, threshold classification)
  Test doubles: FakeRiskScoringApiDouble (from BackendServiceFlow41 companion)
  Result: PASS | Status: green

### Layer 3 (Teaching)
  Lesson: TEST+IMPLEMENT = feature_addition. Two predicates produce two flow instances.
  The IMPLEMENT flow (BackendServiceFlow41) delivers the test doubles; the TEST flow (UnitTestingFlow51) consumes them.
  Sequential execution is the correct order: implement infrastructure first, then write tests.

### Cross-layer invariant
  TEST+IMPLEMENT → feature_addition consistently at every layer.
  "write" fires IMPLEMENT; "unit tests" fires TEST. Both fire independently; neither suppresses the other.
  DeduplicationRule: multiple TEST-firing terms in one request → one TEST predicate. Same for IMPLEMENT.
