# Layer 3 — Teaching Unit Pairs
# Batch: B36-batch-001
# Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Contract: TeachingUnit — cross-layer synthesis and rule consolidation
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Method note

Layer 3 teaching pairs synthesize lessons that span multiple layers within
this batch. For capability-gap batches, the teaching unit plays an especially
important role: it must articulate the precise rules that govern when the
orchestrator produces "unknown" vs a real transition type, and when gap
vocabulary changes the DomainId vs when it does not.

---

## Pair B36B1-WDA-teach-001 — the gap-vocabulary rule: what zero predicates means

```
source_id: xiigen-b36-teach-001
pair_kind: teaching_unit
```

A:
  Question: "When a request contains ONLY project planning vocabulary and zero
             development predicates, what exactly does the orchestrator produce,
             and why does it produce a DomainId of 'project-management' instead
             of 'software-development'?"

B:
  Rule: "Zero compiled predicates → TransitionType: unknown, PhaseId: null,
         SubPhaseId: null. The orchestrator cannot select any registered sub-phase
         because all sub-phases require at least one predicate to be identified."

  DomainId logic:
    "The DomainId is NOT determined by the sub-phase (since none is selected).
     Instead, the DomainId is inferred from the gap vocabulary present in the
     request. Project planning vocabulary (sprint, roadmap, milestone) signals
     a domain of 'project-management', not 'software-development'. This is the
     only case where DomainId differs from the default: normally DomainId is
     'software-development' because all registered sub-phases belong to that
     domain. When no sub-phase is selected and gap vocabulary is present, the
     orchestrator infers the domain from the gap vocabulary classification."

  What this means for downstream:
    "ContextCarryForward: false. No nine-node flow executes. The orchestrator
     emits capability_gap_ids and sets recommended_next_action to
     SCHEDULE_CAPABILITY_LEARNING. The result is a structured gap report, not
     a development workflow execution."

  Counter-example: "If the request contained ONE development predicate alongside
     gap vocabulary (e.g., IMPLEMENT fires on 'implement'), the DomainId reverts
     to 'software-development' and the TransitionType reflects the predicate.
     The gap vocabulary produces CapabilityGapWarnings but does not change
     DomainId or TransitionType."

---

## Pair B36B1-WDA-teach-002 — routing priority: predicates dominate gap vocabulary

```
source_id: xiigen-b36-teach-002
pair_kind: teaching_unit
```

A:
  Question: "A user says: 'Plan the sprint to implement the checkout feature.'
             The predicate IMPLEMENT fires on 'implement'. The word 'sprint' is
             a capability gap. Which signal wins? How does the orchestrator decide?"

B:
  Rule: "Predicates dominate. The orchestrator routes based on compiled predicates,
         NOT on the semantic intent of gap vocabulary. 'Sprint' is gap vocabulary —
         it has no registered predicate and cannot alter the routing decision."

  What fires:
    "IMPLEMENT fires on 'implement', 'feature'. CompiledPredicateIds: ['IMPLEMENT']."

  What does NOT fire:
    "'sprint' and 'plan' are gap vocabulary. They do not fire any predicate.
     Their presence produces a CapabilityGapWarning but does not prevent routing."

  Routing result:
    "IMPLEMENT → Phase 4 → SubPhase 4.1 (Backend Services and Business Logic).
     TransitionType: feature_addition. DomainId: software-development.
     ContextCarryForward: true. The nine-node flow EXECUTES for the implementation work.
     The sprint planning aspect is noted as an uncaptured capability gap."

  Teaching point: "The orchestrator is predicate-driven, not intent-driven. A user
     who writes 'sprint to implement X' is expressing project planning intent (the
     sprint framing) around a development action (implement X). The orchestrator
     cannot understand the project planning intent — it routes the development action.
     The sprint gap is emitted as a warning, not as a routing signal."

---

## Pair B36B1-WDA-teach-003 — false-positive predicates: "story" fires DOCUMENT

```
source_id: xiigen-b36-teach-003
pair_kind: teaching_unit
```

A:
  Question: "A developer writes: 'Estimate story points for backlog items.'
             The word 'story' is present. Does DOCUMENT fire? What does the model
             do with this?"

B:
  Rule: "DOCUMENT fires on 'story' because 'story' is in the DOCUMENT lexicon
         (it matches user stories / use-case stories). The compiler is literal —
         it fires on term presence, not on the surrounding intent."

  What fires:
    "'story' → DOCUMENT fires. CompiledPredicateIds: ['DOCUMENT']."

  What does NOT fire:
    "'estimate', 'backlog', 'story points' (as a compound), 'items' — none of
     these are in any predicate lexicon."

  Consequence:
    "DOCUMENT fires → the router routes to a Phase 2 Requirements sub-phase
     (typically 2.1 Business Requirements and Use Cases). The actual developer
     intent — estimating effort in story points — is not routed. It is lost as
     a capability gap. The system will execute a requirements-authoring flow
     (Phase 2.1) when the developer wanted effort estimation."

  Correct response:
    "CompiledPredicateIds: ['DOCUMENT']. SourceStatus: 'ready'.
     CapabilityGapWarnings: ['gap-project-planning-story-point-001: story-point
     estimation intent not captured; DOCUMENT fires on story substring'].
     This is correct. The model must NOT suppress DOCUMENT to avoid the false
     positive — suppressing predicates based on intent is not how the compiler works."

  Teaching point: "False positives in intent terms are correct outputs in lexicon
     terms. The model must learn to be literal at the compilation step and to emit
     accurate gap warnings when the predicate routing diverges from the user's
     semantic intent."

---

## Pair B36B1-WDA-teach-004 — "review" in a project context fires QA

```
source_id: xiigen-b36-teach-004
pair_kind: teaching_unit
```

A:
  Question: "A user writes: 'Review the team velocity and run the retrospective.'
             Does QA fire? Should it? What does the orchestrator do?"

B:
  Rule: "QA fires on 'review' because 'review' is in the QA predicate lexicon.
         The compiler is literal. QA fires regardless of whether 'review' refers
         to a software quality review or a project management ceremony."

  What fires:
    "'review' → QA fires. CompiledPredicateIds: ['QA']."

  Routing consequence:
    "QA → routes to Phase 6 (Review) sub-phases. The orchestrator cannot distinguish
     'review the team velocity' (a Scrum ceremony) from 'review the pull request'
     (a software quality gate). Both produce QA."

  What is lost:
    "'velocity' and 'retrospective' are gap vocabulary. They produce capability gap
     warnings but do not alter the routing."

  Correct response:
    "CompiledPredicateIds: ['QA']. Route to Phase 6. Emit gap warnings for velocity
     and retrospective. This is the correct output — the model must not suppress QA
     because the user meant a project ceremony, not a software review."

  Teaching point: "The lexicon boundary between 'review' (QA) and 'retrospective'
     (gap) is sharp. 'review' fires. 'retrospective' does not. The model must learn
     both sides of this boundary precisely."

---

## Pair B36B1-WDA-teach-005 — SCHEDULE_CAPABILITY_LEARNING: what it triggers

```
source_id: xiigen-b36-teach-005
pair_kind: teaching_unit
```

A:
  Question: "What does SCHEDULE_CAPABILITY_LEARNING mean as a recommended action?
             What system component acts on it? What outcome does it produce?"

B:
  Definition: "SCHEDULE_CAPABILITY_LEARNING is the recommended action emitted by
     the orchestrator when a capability gap is identified. It means that the
     orchestrator has detected vocabulary (or intent) that it cannot route, and
     it is flagging this for the autonomous learning pipeline."

  What triggers it:
    "Any knowledge-edge result where KnownFailureMode is 'no_predicate_match_capability_gap'.
     The gap vocabulary must have been identified as belonging to a known domain
     (project-management, in this batch) that has no registered sub-phase."

  Which system component acts on it:
    "The AutonomousLearningPipeline (implemented in subproject 16) consumes
     SCHEDULE_CAPABILITY_LEARNING signals. It identifies the gap-ID, locates
     evidence pairs, and determines whether enough evidence exists to promote
     a new sub-phase or extend an existing predicate lexicon."

  What outcome it can produce:
    "Three possible outcomes: (1) A new predicate lexicon entry is added to an
     existing predicate (e.g., 'sprint' added to a future PROJECT_PLANNING predicate).
     (2) A new sub-phase is registered in the router (e.g., sub-phase 8.1 for project
     planning ceremonies). (3) Insufficient evidence — the gap is logged but no
     immediate action is taken. The system waits for more evidence pairs."

  What it does NOT mean:
    "SCHEDULE_CAPABILITY_LEARNING does not mean the current request will be retried.
     It does not mean a fallback flow executes. The current request ends with
     OverallVerdict: KNOWLEDGE_EDGE and FinalHumanResult: null. The user receives
     no output for the unsupported vocabulary."
