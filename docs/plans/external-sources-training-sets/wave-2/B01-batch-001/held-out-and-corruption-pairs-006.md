# Held-Out and Corruption Pairs (006)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: held_out | corruption
# Focus: HO "write the UX spec" → IMPLEMENT+UI+DOCUMENT; COR: IMPLEMENT missing

---

## Section 1 — Held-Out Pair HO-006-A

```
source_id: xiigen-b01-held-out-006-a
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-006-a"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write a complete UX specification for the onboarding screens, covering all interaction patterns and component behaviors."
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
  "Write" → IMPLEMENT (unconditional).
  "UX" → UI (term 3). "screens" → UI (term 4). "interaction" → UI (term 14). "component" → UI (term 9).
  "specification" → DOCUMENT (term 3). "behaviors" → adjacent to "behavior" (DOCUMENT term).
  Three predicates → three routing decisions. TransitionType: feature_addition (IMPLEMENT present).

---

## Section 1 — Held-Out Pair HO-006-B

```
source_id: xiigen-b01-held-out-006-b
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-006-b"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write the screen inventory and functional requirements document for the onboarding flow."
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
  "Write" → IMPLEMENT. "screen" → UI. "functional" → DOCUMENT. "requirements" → DOCUMENT.
  "document" → DOCUMENT. Three predicates confirmed.

---

## Section 2 — Corruption Pair COR-006

```
source_id: xiigen-b01-corruption-006
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-IMPLEMENT-missing-from-write-request
```

A:
  DocumentId: "B01-006-write-spec-prompt"
  CompiledText: "Write the UX specification for the signup screen, describing each form component and its validation behavior."
  CompiledPredicateIds_correct: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B-correct:
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  TransitionType: "feature_addition"
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
    - PredictedSubPhaseId: "4.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false
  DecisionCount: 3

B-corrupt:
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  TransitionType: "documentation"
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 2
  Note: "IMPLEMENT predicate missed; 'write' verb not recognized as IMPLEMENT signal"

Corruption type: Corruption-v6-01 — IMPLEMENT predicate missing from a "write" request.
  "Write" unconditionally fires IMPLEMENT. The corrupted output ignores the "write" verb
  and produces [UI, DOCUMENT] with TransitionType: documentation.

Why this is harmful:
  ImplementationFlow41 (sub-phase 4.1) is never selected. TransitionType is misset
  to "documentation" when it should be "feature_addition". Any downstream pipeline
  step that checks TransitionType to decide whether to invoke implementation agents
  will skip that step. The "write" → IMPLEMENT rule has no exceptions in B01;
  any request with "write" as the primary verb must include IMPLEMENT in PredicateIds.
