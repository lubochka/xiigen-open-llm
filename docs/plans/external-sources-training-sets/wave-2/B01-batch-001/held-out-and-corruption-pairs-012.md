# Held-Out and Corruption Pairs (012)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: held_out | corruption
# Focus: Four mixed held-out cases covering advanced B01 patterns

---

## Section 1 — Held-Out Pair HO-012-A: ARCHITECTURE+UI without IMPLEMENT → documentation

```
source_id: xiigen-b01-held-out-012-a
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-012-a"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Design the screen routing architecture and wireframe the user flows for the complete onboarding experience."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "ARCHITECTURE"]
  TransitionType: "documentation"
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "3.1", EvidencePredicates: ["ARCHITECTURE"], KnowledgeEdge: false

Generalization test:
  "screen routing architecture" — "screen" → UI; "routing architecture" → ARCHITECTURE.
  "wireframe" → UI (term 13). "user flows" — "user" is not a UI term; "flows" alone is not a UI term.
  But "wireframe" and "screen" already confirm UI. Two predicates → two routing decisions.
  No IMPLEMENT verb → TransitionType: documentation.

---

## Section 1 — Held-Out Pair HO-012-B: full four-predicate

```
source_id: xiigen-b01-held-out-012-b
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-012-b"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Build the complete onboarding UX: design the screen flow architecture, wireframe all screens, document the requirements, and implement the form components."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "ARCHITECTURE", "DOCUMENT", "IMPLEMENT"]
  TransitionType: "feature_addition"
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  - PredictedSubPhaseId: "3.1", EvidencePredicates: ["ARCHITECTURE"], KnowledgeEdge: false
  - PredictedSubPhaseId: "4.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false

Generalization test:
  "Build" → IMPLEMENT. "screen flow architecture" → ARCHITECTURE + UI ("screen" fires UI).
  "wireframe" → UI. "document" → DOCUMENT. "requirements" → DOCUMENT.
  "implement" → IMPLEMENT (confirmed). "form" → UI. "components" → UI.
  Four predicates → four routing decisions. SubPhaseId in transition: 1.1 (UI primary).

---

## Section 1 — Held-Out Pair HO-012-C: knowledge-edge near-miss

```
source_id: xiigen-b01-held-out-012-c
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-012-c"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "The onboarding journey needs to feel more cohesive — review the flow and suggest improvements."
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
  "onboarding journey" — "onboarding" is context; "journey" is context. Neither fires a predicate.
  "flow" — not in any predicate lexicon alone.
  "feel more cohesive" — UX quality language; no lexicon term.
  "review" — not in any predicate lexicon.
  "suggest improvements" — no predicate term.
  Despite strong UX intent, zero lexicon terms are present → knowledge-edge path.
  The model must NOT infer predicates from contextual UX language.

---

## Section 1 — Held-Out Pair HO-012-D: DOCUMENT alone → no 1.1 routing

```
source_id: xiigen-b01-held-out-012-d
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-012-d"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the functional behavior of the onboarding flow and write the acceptance criteria for the activation step."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  TransitionType: "feature_addition"
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  - PredictedSubPhaseId: "4.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false

Generalization test:
  "Describe" → DOCUMENT. "functional" → DOCUMENT. "behavior" → DOCUMENT. "acceptance criteria" — "criteria" → DOCUMENT.
  "write" → IMPLEMENT (unconditional).
  No UI term present: "onboarding", "flow", "activation step" — context only.
  Result: [DOCUMENT, IMPLEMENT] → 2.1 + 4.1 routing. No 1.1 routing — UI absent.
  SubPhaseId in transition: 2.1 (DOCUMENT is primary when UI absent; IMPLEMENT > DOCUMENT hierarchy
  gives 4.1 as secondary, but SubPhaseId selects the first active predicate per hierarchy:
  UI absent → ARCHITECTURE absent → IMPLEMENT present → SubPhaseId = 4.1).
  Note: SubPhaseId = 4.1 (IMPLEMENT wins over DOCUMENT per hierarchy UI > ARCHITECTURE > IMPLEMENT > DOCUMENT).
