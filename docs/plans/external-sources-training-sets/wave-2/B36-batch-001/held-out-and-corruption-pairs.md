# Held-Out and Corruption Pairs
# Batch: B36-batch-001
# Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## Held-Out Pairs

Held-out pairs test generalization. They use vocabulary or phrasing not seen
in the training pairs but governed by the same rules.

---

### Held-Out B36B1-HO-001 — "iteration" and "epic": pure gap, no predicate

```
source_id: xiigen-b36-ho-001
held_out_reason: "Tests generalization to iteration/epic vocabulary not explicitly covered in training pairs"
expected_outcome: knowledge_edge
```

A:
  user_request: "Break the epic into iterations and assign each team member their
                 tasks for the upcoming release cycle."
  Task: "Compile predicates and determine routing."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"
  OverallVerdict: "KNOWLEDGE_EDGE"
  capability_gap_ids:
    - "gap-project-planning-epic-001"
    - "gap-project-planning-iteration-001"
    - "gap-project-planning-release-cycle-001"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"

Predicate analysis:
  "break" — not in any predicate lexicon (break ≠ refactor; REFACTOR lexicon
    requires: refactor, restructure, extract, rename, migrate, consolidate)
  "epic" — not in any predicate lexicon
  "iterations" — not in any predicate lexicon
  "assign" — not in any predicate lexicon
  "tasks" — not in any predicate lexicon (task ≠ test)
  "release cycle" — "release" is not in any predicate lexicon in this context
  Result: zero predicates → KNOWLEDGE_EDGE

---

### Held-Out B36B1-HO-002 — capacity planning with ARCHITECTURE boundary

```
source_id: xiigen-b36-ho-002
held_out_reason: "Tests boundary: 'capacity' in team planning context vs system capacity"
expected_outcome: knowledge_edge (ARCHITECTURE does NOT fire on team capacity)
```

A:
  user_request: "Plan team capacity for the next three sprints and allocate resources
                 to the highest-priority backlog items."
  Task: "Compile predicates and determine routing."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"
  OverallVerdict: "KNOWLEDGE_EDGE"
  capability_gap_ids:
    - "gap-project-planning-capacity-001"
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-backlog-001"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"

Predicate analysis:
  "plan" — not in any predicate lexicon
  "team capacity" — 'capacity' is NOT in ARCHITECTURE lexicon; ARCHITECTURE
    fires on system-level capacity terms, not human-resource allocation
  "sprints" — not in any predicate lexicon
  "allocate" — not in any predicate lexicon
  "resources" — not in any predicate lexicon in project planning context
  "backlog" — not in any predicate lexicon
  Result: zero predicates → KNOWLEDGE_EDGE

Generalization test: The model must not fire ARCHITECTURE on "capacity" when
the surrounding context is team planning. ARCHITECTURE fires on system design
terms (architect, design, structure, component, module, layer, service).

---

### Held-Out B36B1-HO-003 — ticket grooming with TEST false-positive boundary

```
source_id: xiigen-b36-ho-003
held_out_reason: "Tests whether 'test' in 'test the estimate' fires TEST predicate"
expected_outcome: TEST fires (literal match), gap warnings emitted
```

A:
  user_request: "Groom the ticket backlog, test the story point estimates with
                 the team, and finalize the sprint scope."
  Task: "Compile predicates and determine routing."

B:
  CompiledPredicateIds: ["TEST", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  CapabilityGapWarnings:
    - "gap-project-planning-ticket-001: 'ticket' not captured"
    - "gap-project-planning-backlog-001: 'backlog' not captured"
    - "gap-project-planning-sprint-001: 'sprint' and 'scope' not captured"
    - "gap-project-planning-story-point-001: 'story point estimates' partially captured via DOCUMENT('story') and TEST('test'); estimation intent not routed"

Predicate analysis:
  "groom" — not in any predicate lexicon
  "ticket" — not in any predicate lexicon
  "backlog" — not in any predicate lexicon
  "test" → TEST fires on "test" (literal match; "test the estimates" fires TEST
    even though the intent is collaborative verification, not software testing)
  "story" → DOCUMENT fires on "story" (literal match; see B36 boundary teaching)
  "point estimates" — not in any predicate lexicon beyond "story" already captured
  "sprint" — not in any predicate lexicon
  "scope" — not in any predicate lexicon
  Result: CompiledPredicateIds: ["TEST", "DOCUMENT"]

Routing consequence: TEST → Phase 5 (Testing); DOCUMENT → Phase 2. Two routing
decisions are produced. The actual user intent (backlog grooming) is not routed
to any registered sub-phase. Both routings are false positives in intent terms
but correct in lexicon terms.

---

## Corruption Pairs

Corruption pairs show incorrect model outputs that a well-trained model must
reject. Three corruption types from the v6 taxonomy apply here.

---

### Corruption B36B1-COR-001 — v6-01: knowledge-edge suppression for gap vocabulary

```
source_id: xiigen-b36-cor-001
corruption_type: "Corruption-v6-01: knowledge edge suppression"
```

A:
  CompiledText: "We need to plan the next sprint and update the roadmap with the new milestones for Q3."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"

CORRUPT B (do not reproduce):
  RoutingDecisions:
    - DocumentId: "B36-compiler-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.71
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

Why this is corrupt:
  "The corrupt output suppresses the knowledge-edge result and invents a DOCUMENT
   predicate that does not exist in the CompiledPredicateIds. 'roadmap' is in the
   input but 'roadmap' does NOT fire DOCUMENT — DOCUMENT fires on document, spec,
   define, describe, requirement, story, criteria, use-case. 'roadmap' is gap
   vocabulary. The corrupt model hallucinated a predicate match to avoid emitting
   KnowledgeEdge: true. This is the canonical v6-01 corruption: suppressing the
   knowledge-edge result by inventing a predicate."

Correct B:
  RoutingDecisions:
    - DocumentId: "B36-compiler-001-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"
  DecisionCount: 1

---

### Corruption B36B1-COR-002 — v6-01: knowledge-edge suppression with false QA routing

```
source_id: xiigen-b36-cor-002
corruption_type: "Corruption-v6-01: knowledge edge suppression (variant — false QA routing)"
```

A:
  CompiledText: "Run our daily standup and capture the action items from yesterday's retrospective."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"

CORRUPT B (do not reproduce):
  RoutingDecisions:
    - DocumentId: "B36-gap-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.68
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: null

Why this is corrupt:
  "The corrupt output suppresses the knowledge-edge result and invents QA on
   'retrospective' — but 'retrospective' is NOT in the QA predicate lexicon.
   Only 'review' fires QA. 'retrospective' is gap vocabulary. The corrupt model
   confused the teaching from B36 kedge-003 (where 'review' fires QA in a
   retrospective context) — the lesson is that 'review' fires, NOT that
   'retrospective' fires. The corrupt model over-generalized and incorrectly
   applied QA to 'retrospective' itself."

Correct B:
  RoutingDecisions:
    - DocumentId: "B36-gap-003-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"
  DecisionCount: 1

---

### Corruption B36B1-COR-003 — v6-03: gap-vocabulary domain bleeding into DomainId

```
source_id: xiigen-b36-cor-003
corruption_type: "Corruption-v6-03: domain signal collapse (gap domain bleeding)"
```

A:
  TransitionId: "transition-B36-t1-002-4.1-feature_addition"
  TransitionType: "feature_addition"
  CompiledText: "Plan the sprint to implement the checkout feature and build the payment integration module."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"

CORRUPT B (do not reproduce):
  TransitionId: "transition-B36-t1-002-corrupt"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "project-management"
  ContextCarryForward: true

Why this is corrupt:
  "The corrupt output sets DomainId to 'project-management' because the request
   contains sprint planning vocabulary. This is wrong. DomainId is determined by
   the PREDICATE that fired (IMPLEMENT → software-development), not by the gap
   vocabulary present in the request. DomainId: 'project-management' is only
   correct when zero predicates fire and the orchestrator infers domain from gap
   vocabulary alone. When IMPLEMENT fires, DomainId must be 'software-development'."

Correct B:
  TransitionId: "transition-B36-t1-002-4.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  UserRequest: "Plan the sprint to implement the checkout feature and build the payment integration module."
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B36-compiler-003-prompt"]
  Metadata:
    transition_classification_basis: "IMPLEMENT predicate (implement, build, feature) classifies as feature_addition; 'sprint' and 'plan' are capability gap vocabulary and do not alter the classification"
    primary_intent: "create"
    domain_signal: "backend_implementation"
    capability_gap_warnings: ["gap-project-planning-sprint-not-captured"]
