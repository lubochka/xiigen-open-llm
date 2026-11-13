# Held-Out and Corruption Pairs (004)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: held_out | corruption
# Focus: HO "button and input states" → UI; COR: UI not recognized

---

## Section 1 — Held-Out Pair HO-004-A

```
source_id: xiigen-b01-held-out-004-a
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-004-a"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define all button and input field states across the onboarding flow: default, hover, focus, disabled, error, and loading."
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
  "button" → UI (term 16). "input" → UI (term 17). Two UI terms present.
  "define" → DOCUMENT (fires DOCUMENT predicate).
  "states" — not a standalone predicate term; compound "button states" anchors to "button" (UI).
  "hover, focus, disabled, error, loading" — state names; not predicate terms.
  Result: [UI, DOCUMENT] → two routing decisions.

---

## Section 1 — Held-Out Pair HO-004-B

```
source_id: xiigen-b01-held-out-004-b
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-004-b"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Specify the visual states for all form inputs used in the signup flow."
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
  "visual" → UI (term 12). "form" → UI (term 7). "inputs" → UI (term 17).
  "Specify" — not in DOCUMENT lexicon (DOCUMENT terms: document, spec, specification, requirement,
  story, criteria, describe, define, functional, behavior, use-case, narrative, feature-brief,
  capability, epics). "Specify" is NOT "specification" or "spec" — does not fire DOCUMENT.
  Result: UI alone → 1.1.

---

## Section 2 — Corruption Pair COR-004

```
source_id: xiigen-b01-corruption-004
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-UI-not-recognized-button-input
```

A:
  DocumentId: "B01-004-btn-prompt"
  CompiledText: "List all button and input states for the onboarding form components."
  CompiledPredicateIds_correct: ["UI"]
  SourceStatus: "ready"

B-correct:
  CompiledPredicateIds: ["UI"]
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  DecisionCount: 1

B-corrupt:
  CompiledPredicateIds: []
  Confidence: 0.70
  RoutingDecisions:
    - PredictedSubPhaseId: null
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match"
  DecisionCount: 0
  Note: "'button', 'input', 'form', 'component' all missed"

Corruption type: Corruption-v6-01 — UI not recognized; four UI terms all suppressed.
  "button" (term 16), "input" (term 17), "form" (term 7), and "component" (term 9) are
  all in the UI lexicon. The corrupted output misses all four and routes to knowledge-edge.

Why this is harmful:
  All four terms are tier-1 UI lexicon members — "button" and "input" are atomic UI primitives,
  "form" is a primary B01 screen type, and "component" is a core UI lexicon term.
  Missing all four sends a valid screen state inventory request to the knowledge-edge
  dead end. The correct output fires UI via any one of the four terms, routes to 1.1,
  and executes ScreenInventoryFlow11.
