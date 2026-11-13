# Cross-Layer Synthesis (006)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Topic: "run unit tests" → no TEST predicate → knowledge_edge — full pipeline trace
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## S006 — End-to-end trace: "Run unit tests for the DiscountEngine after the pricing refactor"

### Layer 0-A (Compiler)
Input: "Run unit tests for the DiscountEngine after the pricing refactor."
  "run" — NOT in TEST predicate lexicon; execution verb; gap
  "unit tests" — object of "run"; does not promote "run" to TEST
  "after the pricing refactor" — temporal qualifier; "refactor" here is a noun, not an IMPLEMENT verb
  CompiledPredicateIds: []

### Layer 0-B (Router)
  RoutingDecisions: [] — no predicates → no routing
  TransitionType: unknown
  SubPhaseId: null

### Layer 0-C (Knowledge Edge)
  CapabilityGapSignal raised:
    gap_id: "gap-run-not-test-synthesis-006"
    gap_vocabulary: ["run unit tests", "after the pricing refactor"]
    SCHEDULE_CAPABILITY_LEARNING: true
  Note: "run unit tests" is the most common source of this gap. Flagged for capability learning.

### Layer 0-D (Factory)
  FlowFactory.CreateMulti([]) → [] — empty predicate list → no flow instances
  Early termination. Factory returns immediately.

### Layer 1 (Transition Orchestrator)
  TransitionType: unknown
  No phase assignment. Request exits the pipeline as knowledge_edge.

### Layer 2 (Nine-Node Flow)
  No flow started. Zero predicates → zero flow instances → no nine-node processing.

### Layer 3 (Teaching)
  Lesson: "run" produces zero predicates at every layer simultaneously. The gap propagates
  consistently from compiler → router → factory → orchestrator → no flow. There is no
  layer at which "run" recovers and becomes TEST. The boundary is absolute.

### Cross-layer invariant
  "run [test noun]" → [] at compiler → [] at router → [] at factory → unknown at orchestrator → no flow at Layer 2.
  Correction required at source: replace "run" with "write", "create", "build", "develop", or "assert/verify/spec".
  Contrast: "Write unit tests for the DiscountEngine" → ["TEST", "IMPLEMENT"] → feature_addition → UnitTestingFlow51 → PASS.
