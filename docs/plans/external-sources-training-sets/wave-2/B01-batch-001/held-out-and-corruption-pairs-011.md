# Held-Out and Corruption Pairs (011)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: held_out | corruption
# Focus: Three v6 corruptions — v6-01, v6-02, v6-03 (one each, canonical forms)

---

## Section 2 — Corruption Pair COR-011-A: v6-01 canonical

```
source_id: xiigen-b01-corruption-011-a
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-UI-predicate-missed-canonical
```

A:
  DocumentId: "B01-011a-ui-miss-prompt"
  CompiledText: "Map the complete screen layout and interaction patterns for the account setup flow."
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
  CorruptEvidenceAnalysis: "'screen', 'layout', 'interaction' all missed by corrupted model"

Corruption v6-01: UI predicate missed.
  Evidence: "screen" (UI term 4), "layout" (UI term 8), "interaction" (UI term 14).
  All three are in the UI lexicon. The corrupted model fails to fire UI for any of them.
  Correct behavior: any single UI term fires UI → routes to 1.1.

---

## Section 2 — Corruption Pair COR-011-B: v6-02 canonical

```
source_id: xiigen-b01-corruption-011-b
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-02-DOCUMENT-suppressed-canonical
```

A:
  DocumentId: "B01-011b-doc-suppress-prompt"
  CompiledText: "Map the onboarding screens and document the functional requirements and use-cases for each screen."
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
  CorruptEvidenceAnalysis: "'document', 'functional', 'use-cases' all present but DOCUMENT not fired"

Corruption v6-02: DOCUMENT suppressed.
  "document" (DOCUMENT term 1), "functional" (DOCUMENT term 9), "use-cases" (DOCUMENT term 11).
  Three DOCUMENT signals present; all suppressed after UI detected.
  Correct behavior: DOCUMENT routes to 2.1 independently; two routing decisions required.

---

## Section 2 — Corruption Pair COR-011-C: v6-03 canonical

```
source_id: xiigen-b01-corruption-011-c
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-03-UI-wrong-sub-phase-canonical
```

A:
  DocumentId: "B01-011c-wrong-phase-prompt"
  CompiledText: "Inventory all screens in the signup and activation onboarding flow."
  CompiledPredicateIds_correct: ["UI"]
  SourceStatus: "ready"

B-correct:
  CompiledPredicateIds: ["UI"]
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  DecisionCount: 1

B-corrupt:
  CompiledPredicateIds: ["UI"]
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  DecisionCount: 1
  CorruptEvidenceAnalysis: "UI detected correctly but routed to 2.1 instead of 1.1"

Corruption v6-03: UI routes to wrong sub-phase.
  UI is detected (via "screens") but mapped to sub-phase 2.1 (DOCUMENT's sub-phase)
  instead of 1.1 (UI's sub-phase).
  Correct behavior: UI always routes to SubPhaseId 1.1, never to 2.1.
  Detection: UI in EvidencePredicates but PredictedSubPhaseId = 2.1.
