# Held-Out and Corruption Pairs
# Batch: B01-batch-001
# Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs

Held-out pairs are excluded from training. They test generalization:
the model must produce correct outputs for these inputs without having
seen them in the training set.

---

### Held-Out Pair B01B1-WDA-HO-001 — rephrased screen inventory still routes to 1.1

```
source_id: xiigen-b01-held-out-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We need a full map of every page in our mobile app, grouped by user journey stage, with each page's entry points and next screens documented."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false

Generalization test:
  "page" matches UI lexicon. "screen" is not present but "page" is sufficient.
  "user journey" does not fire any predicate — "journey" is not in the UI lexicon,
  but the presence of "page" ensures UI still fires.
  "entry points", "next screens", "documented" — "screen" fires UI again.
  The model must route this to 1.1 via the UI predicate even though the phrasing
  ("journey stage", "entry points", "next screens") differs from the training examples.

---

### Held-Out Pair B01B1-WDA-HO-002 — boundary: UX improvement without screen terms

```
source_id: xiigen-b01-held-out-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We should improve the onboarding flow to reduce drop-off and make it clearer."
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
  "onboarding flow" — "flow" is not in any predicate lexicon.
  "improve" — not in the IMPLEMENT lexicon (which requires implement, code, build,
  develop, write, create, function, class, logic, handler, backend, feature, refactor).
  "drop-off", "clearer" — no predicate terms.
  Despite the clear UX intent, NO lexicon terms are present → knowledge-edge result.
  The model must NOT infer the UI predicate from context alone. Only explicit lexicon
  matches trigger predicates. This pair teaches the boundary between inference and facts.

---

### Held-Out Pair B01B1-WDA-HO-003 — multi-predicate: screen flows plus spec document

```
source_id: xiigen-b01-held-out-003
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We need to map the admin interface screens and write a functional specification describing each page's behavior and use-case."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false

Generalization test:
  "interface" → UI. "screen" → UI. "page" → UI.
  "functional specification" → DOCUMENT (specification). "behavior" → DOCUMENT (behavior).
  "use-case" → DOCUMENT (use-case).
  Multi-predicate request → two routing decisions. The model must not collapse both
  into a single 1.1 decision — DOCUMENT must independently route to 2.1.

---

## Section 2 — Corruption Pairs (v6 types)

Corruption pairs show a correct A input alongside both the correct B output and
a corrupted B output. Three v6 corruption types are demonstrated.

---

### Corruption Pair B01B1-WDA-COR-001 — Corruption-v6-01: knowledge edge suppression

```
source_id: xiigen-b01-corruption-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-knowledge-edge-suppression
```

A:
  DocumentId: "B01-kedge-001-prompt"
  CompiledText: "We should redesign the onboarding experience to feel more modern and engaging."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"

B-correct:
  RoutingDecisions:
    - DocumentId: "B01-kedge-001-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match"

B-corrupt:
  RoutingDecisions:
    - DocumentId: "B01-kedge-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.70
      KnowledgeEdge: false
      FailureClassification: null

Corruption type: Corruption-v6-01 — knowledge edge suppression.
  The corrupted output invents a sub-phase routing decision (1.1) for an input
  with no matched predicates. It suppresses KnowledgeEdge: true and fabricates
  a ready SourceStatus.

Why this is harmful:
  The orchestrator reads KnowledgeEdge: false and proceeds to execute the
  ScreenInventoryFlow for a request with no actionable UX design signal.
  The result is phantom execution producing an invalid screen map. The
  capability-learning signal is suppressed — the system never learns that
  "modern and engaging" language has no registered routing. The correct
  behavior is KnowledgeEdge: true → synthetic flow → SCHEDULE_CAPABILITY_LEARNING.

---

### Corruption Pair B01B1-WDA-COR-002 — Corruption-v6-02: flow factory fallback suppression

```
source_id: xiigen-b01-corruption-002
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-02-flow-factory-fallback-suppression
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "screen-inventory-flow-1.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 3 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B-correct:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "screen-inventory-flow-1.1-candidate"
  Warnings: ["probationary_flow_rejected:screen-inventory-flow-1.1-candidate", "fallback_used:universal-nine-node-development-flow-1.1"]
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

B-corrupt:
  SelectedFlowId: "screen-inventory-flow-1.1-candidate"
  TrustState: "probationary"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

Corruption type: Corruption-v6-02 — flow factory fallback suppression.
  The corrupted output accepts a probationary flow that has NOT passed Gate B,
  falsely records GateBPassed as true, suppresses rejection warnings, and
  suppresses the fallback flag.

Why this is harmful:
  A flow with only 3 of 9 nodes covered executes for production routing.
  The 6 missing nodes produce generic or incorrect screen inventory outputs.
  Suppressed warnings prevent the learning pipeline from scheduling the missing
  node coverage work. The correct behavior is to reject and use the safe
  deterministic generic flow with explicit rejection warnings.

---

### Corruption Pair B01B1-WDA-COR-003 — Corruption-v6-03: multi-branch result collapse

```
source_id: xiigen-b01-corruption-003
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-03-multi-branch-result-collapse
```

A:
  DocumentId: "B01-compiler-003-prompt"
  CompiledText: "Document the complete user interface flows for the checkout process and produce a page specification with form interaction details at each step."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 2

B-corrupt:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI", "DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 1

Corruption type: Corruption-v6-03 — multi-branch result collapse.
  The corrupted output collapses both predicates into a single routing decision
  for sub-phase 1.1, instead of producing two separate branch decisions.

Why this is harmful:
  The collapsed output assigns DOCUMENT to sub-phase 1.1, which does not own
  that predicate. Sub-phase 2.1 (Business Requirements and Use Cases) is never
  selected; the page specification work is never executed. The page specification
  artifact is missing from the output. The correct behavior is two independent
  RoutingDecision entries, each carrying only its own evidence predicates.
