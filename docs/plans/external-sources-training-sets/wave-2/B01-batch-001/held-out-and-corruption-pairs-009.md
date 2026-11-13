# Held-Out and Corruption Pairs (009)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: held_out | corruption
# Focus: HO UI→1.1 and DOCUMENT→2.1 both from same request; COR: only 1.1 produced

---

## Section 1 — Held-Out Pair HO-009-A

```
source_id: xiigen-b01-held-out-009-a
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-009-a"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map all the screens in the onboarding flow and write up the requirements for each one — what data is captured, what actions are available, what the success condition is."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  - PredictedSubPhaseId: "4.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false

Generalization test:
  "screens" → UI. "write up" — "write" fires IMPLEMENT (unconditional).
  "requirements" → DOCUMENT. Three predicates → three decisions.
  "what data is captured" — context; no predicate. "success condition" — context.
  TransitionType: feature_addition (IMPLEMENT present).

---

## Section 1 — Held-Out Pair HO-009-B

```
source_id: xiigen-b01-held-out-009-b
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-009-b"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Inventory the onboarding screens and produce a feature brief for each screen covering the user's goal and the interface elements present."
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
  "screens" → UI. "interface" → UI. "feature brief" → DOCUMENT (term 13 in DOCUMENT lexicon).
  No IMPLEMENT verb. Two predicates → two decisions. TransitionType: documentation.

---

## Section 2 — Corruption Pair COR-009

```
source_id: xiigen-b01-corruption-009
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-02-DOCUMENT-suppressed-after-UI-found
```

A:
  DocumentId: "B01-009-both-prompt"
  CompiledText: "Inventory the login and signup screens and document the requirements and acceptance criteria for each screen."
  CompiledPredicateIds_correct: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  TransitionType: "documentation"
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 2

B-corrupt:
  CompiledPredicateIds: ["UI"]
  TransitionType: "feature_addition"
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  DecisionCount: 1
  Note: "DOCUMENT suppressed; only one routing decision produced; TransitionType incorrectly set to feature_addition"

Corruption type: Corruption-v6-02 — DOCUMENT suppressed; only 1.1 routing produced.
  "document" fires DOCUMENT. "requirements" fires DOCUMENT. "acceptance criteria" — "criteria"
  fires DOCUMENT. Three DOCUMENT signals are present and all suppressed.
  The corrupted output also missets TransitionType to feature_addition (no IMPLEMENT present).

Why this is harmful:
  BusinessRequirementsFlow21 (sub-phase 2.1) is never selected. The requirements and
  acceptance criteria documentation is never produced. TransitionType is wrong (feature_addition
  without IMPLEMENT). The correct output produces two routing decisions and sets
  TransitionType to documentation. DOCUMENT must route independently regardless of UI presence.
