# Cross-Layer Synthesis (002)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## Synthesis note

File 002 traces the full layer-by-layer execution for DOCUMENT + IMPLEMENT:
"Write the user stories and acceptance criteria for the checkout feature."
This is the canonical dual-predicate scenario with feature_addition TransitionType.

---

## Synthesis Trace — DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-synthesis-002
pair_kind: cross_layer_synthesis
scenario: "DOCUMENT + IMPLEMENT — dual-predicate with write"
```

### Input
UserRequest: "Write the user stories and acceptance criteria for the checkout feature."
TransitionId: "transition-B06-synthesis-002"

---

### Layer 0-A — Compiler

Layer: 0-A
Unit: PredicateCompilerUnit
Input: "Write the user stories and acceptance criteria for the checkout feature."

Scan results:
  "write" → IMPLEMENT (IMPLEMENT action verb)
  "stories" → DOCUMENT (story in DOCUMENT lexicon)
  "criteria" → DOCUMENT (criteria in DOCUMENT lexicon)
  "feature" → IMPLEMENT (feature in IMPLEMENT lexicon — but not "feature-brief")
  "checkout" → not in any predicate lexicon

DOCUMENT signals: ["stories", "criteria"] → DOCUMENT fires
IMPLEMENT signals: ["write", "feature"] → IMPLEMENT fires

Output:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledTerms_DOCUMENT: ["stories", "criteria"]
  CompiledTerms_IMPLEMENT: ["write", "feature"]
  Confidence: 0.91
  KnowledgeEdge: false

v6-02 risk: model may suppress IMPLEMENT because context is heavy on DOCUMENT nouns. Must not.
v6-01 risk: model may suppress DOCUMENT because "write" is the leading verb. Must not.

Reference: layer-0a-compiler-pairs-003.md

---

### Layer 0-B — Router

Layer: 0-B
Unit: PredicateRouterUnit
Input: CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

Routing logic:
  DOCUMENT → 2.1 (RequirementsAuthoringFlow21)
  IMPLEMENT → 4.1 (ImplementationFlow41)
  DecisionCount: 2

Output:
  RoutingDecisions:
    - DocumentId: "transition-B06-synthesis-002"; PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - DocumentId: "transition-B06-synthesis-002"; PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]

Reference: layer-0b-router-pairs-003.md

---

### Layer 0-D — Flow Factory

Layer: 0-D
Unit: ILogicalFlowFactory
Input: Two routing decisions (2.1 + 4.1)

Output:
  Flow_2_1:
    SelectedFlowId: "requirements-authoring-flow-2.1"
    TrustState: "trusted_learned"
  Flow_4_1:
    SelectedFlowId: "implementation-flow-4.1"
    TrustState: "trusted_learned"

Reference: layer-0d-flow-factory-pairs-006.md

---

### Layer 1 — Transition Orchestrator

Layer: 1
Unit: LogicalTransitionOrchestratorUnit

Output:
  Transition_2_1:
    TransitionId: "transition-B06-synthesis-002-2.1"
    TransitionType: "feature_addition"
    DomainId: "software-development"
    SubPhaseId: "2.1"
    SelectedFlowId: "requirements-authoring-flow-2.1"

  Transition_4_1:
    TransitionId: "transition-B06-synthesis-002-4.1"
    TransitionType: "feature_addition"
    DomainId: "software-development"
    SubPhaseId: "4.1"
    SelectedFlowId: "implementation-flow-4.1"

TransitionType_reason: "IMPLEMENT present → feature_addition for all transitions in this operation"

Reference: layer-1-transition-orchestrator-pairs-003.md

---

### Layer 2 — Nine-Node Flow (2.1 side)

Flow: requirements-authoring-flow-2.1 (runs concurrently with 4.1)

N01: DomainPacket DOCUMENT → "User stories and acceptance criteria for checkout feature"
N02: TechnicalStatement: "8 user stories (US-001 through US-008); 16 acceptance criteria in Given/When/Then format"
N03: WorkBranches: [story-writing, criteria-authoring, story-mapping, traceability]
N09: PASS — 8 stories, 16 criteria, feature_addition

Note: 4.1 flow runs independently; its output is implementation code/service. Not expanded in this synthesis.

Reference: layer-2-nine-node-2.1-pairs-003.md

---

### Synthesis Summary

Predicates: DOCUMENT + IMPLEMENT
Routing decisions: 2 (→ 2.1 + 4.1)
TransitionType: feature_addition (IMPLEMENT present)
DomainId: software-development
Flows: 2 (RequirementsAuthoringFlow21 + ImplementationFlow41), running concurrently
N09 verdicts: PASS (2.1), PASS (4.1)

Key teaching:
  "write" fires IMPLEMENT regardless of noun context
  "stories" + "criteria" both fire DOCUMENT
  "feature" fires IMPLEMENT (not DOCUMENT — that requires "feature-brief")
  Neither predicate suppresses the other
  IMPLEMENT presence → feature_addition for ALL transitions in the same operation
