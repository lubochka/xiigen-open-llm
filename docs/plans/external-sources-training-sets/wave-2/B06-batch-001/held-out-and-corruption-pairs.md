# Held-Out and Corruption Pairs
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs

---

### Held-Out Pair B06B1-WDA-HO-001 — rephrased requirements request still routes to 2.1

```
source_id: xiigen-b06-held-out-001
pair_kind: held_out
```

A:
  run_id: "B06-ho-001"
  user_request: "We need to capture all the business rules and scenarios for the order placement feature — including the happy path, edge cases, and failure modes."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false

Generalization test:
  "capture" is not in DOCUMENT lexicon but "scenarios" → not directly either.
  "business rules" — "business" is in DOCUMENT lexicon → DOCUMENT fires.
  "feature" — in IMPLEMENT lexicon → IMPLEMENT also fires.
  Corrected expectation: CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Routing: 2.1 (DOCUMENT) + 4.1 (IMPLEMENT)
  The model must recognize "business" as a DOCUMENT term and "feature" as an
  IMPLEMENT term, producing dual-predicate output without suppressing either.

---

### Held-Out Pair B06B1-WDA-HO-002 — boundary: process description without DOCUMENT terms

```
source_id: xiigen-b06-held-out-002
pair_kind: held_out
```

A:
  run_id: "B06-ho-002"
  user_request: "The checkout process should handle different payment methods gracefully."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: []
  Confidence: 0.70
  Warnings: ["no_predicate_matched: knowledge_edge_result"]

Expected routing decisions:
  - PredictedSubPhaseId: null, KnowledgeEdge: true, FailureClassification: "no_predicate_match"

Generalization test:
  "checkout process" — "process" is not in any predicate lexicon.
  "payment methods" — not in any predicate lexicon.
  "gracefully" — not in any predicate lexicon.
  Despite clear business intent, NO lexicon terms are present → knowledge-edge.
  The model must not infer DOCUMENT from "checkout process" context alone.

---

### Held-Out Pair B06B1-WDA-HO-003 — use-case with stakeholder signals routes to 2.1 and 2.4

```
source_id: xiigen-b06-held-out-003
pair_kind: held_out
```

A:
  run_id: "B06-ho-003"
  user_request: "Define the use cases and stakeholder requirements for the returns and refund process, covering merchant, customer, and finance team needs."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  - PredictedSubPhaseId: "2.4", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false

Generalization test:
  "define" → DOCUMENT. "use cases" (use-case) → DOCUMENT. "stakeholder" → DOCUMENT.
  "requirements" → DOCUMENT. All DOCUMENT terms → single predicate compiled.
  However, both 2.1 (use cases) and 2.4 (external stakeholder requirements) are
  valid sub-phase targets for this DOCUMENT signal, producing two routing decisions.
  The model must produce both 2.1 and 2.4 decisions with DOCUMENT as evidence for each.

---

## Section 2 — Corruption Pairs (v6 types)

---

### Corruption Pair B06B1-WDA-COR-001 — Corruption-v6-01: knowledge edge suppression

```
source_id: xiigen-b06-corruption-001
pair_kind: corruption
corruption_type: Corruption-v6-01-knowledge-edge-suppression
```

A:
  DocumentId: "B06-kedge-001-prompt"
  CompiledText: "Our checkout needs to be better — customers are abandoning their carts."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"

B-correct:
  RoutingDecisions:
    - DocumentId: "B06-kedge-001-prompt"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match"

B-corrupt:
  RoutingDecisions:
    - DocumentId: "B06-kedge-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.70
      KnowledgeEdge: false
      FailureClassification: null

Corruption type: Corruption-v6-01 — knowledge edge suppression.
Why harmful: The orchestrator proceeds to execute the requirements authoring flow for a
request with no requirements vocabulary. The output will be a fabricated requirements
document for an unactionable problem statement. The capability gap signal is lost.

---

### Corruption Pair B06B1-WDA-COR-002 — Corruption-v6-02: flow factory fallback suppression

```
source_id: xiigen-b06-corruption-002
pair_kind: corruption
corruption_type: Corruption-v6-02-flow-factory-fallback-suppression
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: "requirements-authoring-flow-2.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 4 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B-correct:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "requirements-authoring-flow-2.1-candidate"
  Warnings: ["probationary_flow_rejected:requirements-authoring-flow-2.1-candidate", "fallback_used:universal-nine-node-development-flow-2.1"]
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

B-corrupt:
  SelectedFlowId: "requirements-authoring-flow-2.1-candidate"
  TrustState: "probationary"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

Corruption type: Corruption-v6-02 — flow factory fallback suppression.
Why harmful: A flow with 4 of 9 nodes covered produces incomplete use case
documents — 5 nodes generate generic or absent output. Warnings suppressed,
so the pipeline never schedules the missing node coverage work.

---

### Corruption Pair B06B1-WDA-COR-003 — Corruption-v6-03: multi-branch result collapse

```
source_id: xiigen-b06-corruption-003
pair_kind: corruption
corruption_type: Corruption-v6-03-multi-branch-result-collapse
```

A:
  DocumentId: "B06-compiler-002-prompt"
  CompiledText: "Write the user stories and acceptance criteria for the payment processing feature."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
    - PredictedSubPhaseId: "4.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false
  DecisionCount: 2

B-corrupt:
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT", "IMPLEMENT"], KnowledgeEdge: false
  DecisionCount: 1

Corruption type: Corruption-v6-03 — multi-branch result collapse.
Why harmful: IMPLEMENT predicate is swallowed into 2.1, which does not own it.
Sub-phase 4.1 (Backend Services and Business Logic) is never selected. The
implementation work that "write" signals is never routed or executed.
