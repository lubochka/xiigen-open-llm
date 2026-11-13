# Held-Out and Corruption Pairs (005)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: held_out | corruption
# Focus: HO "define the screen flow" → UI+DOCUMENT; COR: DOCUMENT suppressed

---

## Section 1 — Held-Out Pair HO-005-A

```
source_id: xiigen-b01-held-out-005-a
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-005-a"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the screen flow for the account setup wizard, including all screens, their order, and the conditions that move the user forward."
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
  "screen" → UI (term 4). "screens" confirms UI.
  "define" → DOCUMENT (DOCUMENT term).
  "their order" — "order" is not a predicate term; the predicate comes from "screen" and "define".
  "conditions" — not a predicate term; does not fire any predicate.
  Result: [UI, DOCUMENT] → two routing decisions (1.1 and 2.1).

---

## Section 1 — Held-Out Pair HO-005-B

```
source_id: xiigen-b01-held-out-005-b
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-005-b"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the user stories for the onboarding screen inventory, including acceptance criteria per screen."
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
  "screen" → UI (term 4). "stories" — matches "story" (DOCUMENT term).
  "criteria" → DOCUMENT (term 6 in DOCUMENT lexicon).
  "acceptance criteria" — "criteria" alone fires DOCUMENT.
  Two predicates → two routing decisions.

---

## Section 2 — Corruption Pair COR-005

```
source_id: xiigen-b01-corruption-005
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-02-DOCUMENT-suppressed-when-UI-present
```

A:
  DocumentId: "B01-005-define-prompt"
  CompiledText: "Define and map the screen flow for the onboarding wizard, documenting the entry and exit conditions for each screen."
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
  CompiledPredicateIds: ["UI"]
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  DecisionCount: 1
  Note: "DOCUMENT suppressed after UI detected first"

Corruption type: Corruption-v6-02 — DOCUMENT suppressed when UI is present.
  "define" and "documenting" both fire DOCUMENT. The corrupted output stops after
  detecting UI via "screen" and suppresses the DOCUMENT predicate.
  This violates the predicate independence rule: each predicate routes independently.

Why this is harmful:
  The documentation work (defining entry/exit conditions, producing a requirements artifact)
  is never executed. BusinessRequirementsFlow21 is never selected. The correct output
  always produces a routing decision per predicate, regardless of which predicates were
  already found. The independence rule: DOCUMENT routes to 2.1 whether or not UI is present.
