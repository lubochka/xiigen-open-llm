# Layer 2 — Nine-Node 3.3 Pairs (023)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: nine_node

---

## B12-L2-023 — ProbationaryRejected: API_CONTRACT+TEST contract validation flow REJECTED

```
source_id: xiigen-b12-l2-023
flow_name: ProbationaryRejected
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: nine_node
gate_b: FAIL
```

### Input (N01)

N01 classification:
  user_request: "Specify the API contract for the product review service endpoints."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  DomainId: "software-development"
  FlowId: "ProbationaryRejected"
  SubPhase: "3.3"
  flow_registry_state:
    registered_flow_id: "product-review-api-contract-flow-3.3-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 4 of 9 nodes covered"
  FallbackUsed: true (→ universal-nine-node-development-flow-3.3 used instead)

### Processing note

This nine-node record documents a REJECTED probationary flow scenario.
The candidate flow "product-review-api-contract-flow-3.3-candidate" failed Gate B because
only 4 of 9 nodes had sufficient evidence pairs. The system fell back to
universal-nine-node-development-flow-3.3 (deterministic_generic).

Rejection details:
  Nodes with insufficient coverage:
    N03 (SchemePlannerUnit): 1 pair (minimum 3 required)
    N06 (ResultTranslatorUnit): 2 pairs (minimum 3 required)
    N07 (ArbiterSelectorUnit): 1 pair (minimum 3 required)
    N08 (ArbiterUnit): 0 pairs (minimum 3 required)
    N09 (FormatterUnit): 1 pair (minimum 3 required)
  Nodes with sufficient coverage:
    N01 (DomainSplitterUnit): 4 pairs — PASS
    N02 (DomainTranslatorUnit): 5 pairs — PASS
    N04 (DomainProcessorUnit): 3 pairs — PASS
    N05 (ApplierUnit): 3 pairs — PASS

### Fallback execution (universal-nine-node-development-flow-3.3)

N02 — Domain: product reviews (create, read, moderate)
N03 — Branches: POST /reviews, GET /reviews/{productId}, DELETE /reviews/{id}
N04 — Schemas: ReviewRequest, ReviewResponse, ModerationStatus
N05 — Apply: product-review-openapi.yaml
N06 — Summary: 3 endpoints, rating 1-5 constraint, moderation flag
N08 — Verdict: PASS (fallback flow completes successfully)

### Output (N09)

N09 — FormatterUnit:
  FlowStatus: "PASS" (via fallback)
  FallbackUsed: true
  RejectedFlow: "product-review-api-contract-flow-3.3-candidate"
  Artifacts:
    1. product-review-openapi.yaml
    2. product-review-api-guide.md
  GateB: FAIL for candidate flow; PASS for fallback execution
  Teaching: Probationary rejection does not block execution. The fallback (deterministic_generic)
  always provides a reliable baseline result. Gate B enforcement prevents under-tested learned
  flows from replacing the reliable deterministic baseline.
