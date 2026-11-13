# Held-Out and Corruption Pairs (003)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: held_out | corruption
# Focus: HO "interactive wireframe" → UI; COR: wireframe→DOCUMENT

---

## Section 1 — Held-Out Pair HO-003-A

```
source_id: xiigen-b01-held-out-003-a
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-003-a"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Produce interactive wireframes for the entire onboarding flow, showing all screen states and transitions."
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
  "wireframes" — plural form of "wireframe" (UI term 13). Fires UI.
  "interactive" — not a standalone UI term; "interaction" is the UI term (14); "interactive" is an adjective form.
  "screen states" — "screen" fires UI (term 4).
  "transitions" — not in any predicate lexicon alone; context word.
  Two UI terms confirm routing to 1.1 via ScreenInventoryFlow11.

---

## Section 1 — Held-Out Pair HO-003-B

```
source_id: xiigen-b01-held-out-003-b
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-003-b"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create low-fidelity wireframes for the signup and activation screens, and document the interaction patterns on each screen."
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
  "wireframes" → UI. "screens" → UI. "interaction" → UI.
  "document" → DOCUMENT. Two predicates → two routing decisions.
  "low-fidelity" is an adjective for wireframe quality; does not change predicate membership.
  "Create" does not fire IMPLEMENT in this context (the verb table notes "create" is context-dependent).

---

## Section 2 — Corruption Pair COR-003

```
source_id: xiigen-b01-corruption-003
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-wireframe-misrouted-to-DOCUMENT
```

A:
  DocumentId: "B01-003-wf-prompt"
  CompiledText: "Wireframe all the onboarding screens and document the interaction patterns."
  CompiledPredicateIds_correct: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 2

B-corrupt:
  CompiledPredicateIds: ["DOCUMENT"]
  Confidence: 0.80
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 1
  Note: "wireframe misidentified as DOCUMENT context word"

Corruption type: Corruption-v6-01 — UI predicate missed; "wireframe" classified as DOCUMENT.
  The corrupted output mistakes "wireframe" for a documentation activity and fires DOCUMENT
  but not UI. "Wireframe" is term 13 in the UI lexicon — it unconditionally fires UI.

Why this is harmful:
  Sub-phase 1.1 and ScreenInventoryFlow11 are never selected. The screen inventory
  for the onboarding wireframes is missing from the output. Only the interaction
  documentation (2.1) executes. The correct behavior pairs both 1.1 and 2.1 routing
  decisions for a request containing both "wireframe" (UI) and "document" (DOCUMENT).
