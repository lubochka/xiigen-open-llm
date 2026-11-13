# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B36-T1-024-001 — Complete transition type table for B36

A:
  Task: "State the TransitionType for each predicate combination in B36."

B:
  TransitionTypeTable:
    predicates: [] → TransitionType: "unknown", DomainId: "project-management"
    predicates: ["DOCUMENT"] → TransitionType: "documentation", DomainId: "software-development"
    predicates: ["IMPLEMENT"] → TransitionType: "feature_addition", DomainId: "software-development"
    predicates: ["QA"] → TransitionType: "review", DomainId: "software-development"
    predicates: ["ARCHITECTURE"] → TransitionType: "architecture", DomainId: "software-development"
    predicates: ["TEST"] → TransitionType: "testing", DomainId: "software-development"
    predicates: ["IMPLEMENT", "DOCUMENT"] → TransitionType: "feature_addition" (IMPLEMENT is primary), DomainId: "software-development"
    predicates: ["QA", "DOCUMENT"] → TransitionType: "review" (QA is primary), DomainId: "software-development"
    predicates: ["QA", "IMPLEMENT"] → TransitionType: "review" (QA last rule applies), DomainId: "software-development"
    predicates: ["IMPLEMENT", "DOCUMENT", "QA"] → TransitionType: "feature_addition" (IMPLEMENT primary, QA last), DomainId: "software-development"

---

## B36-T1-024-002 — ContextCarryForward rule for B36

A:
  Task: "State when ContextCarryForward is true vs false in B36 orchestrator output."

B:
  ContextCarryForwardRule:
    true: "When CompiledPredicateIds is not empty (any predicate fired). A nine-node flow will execute."
    false: "When CompiledPredicateIds is [] (knowledge-edge path). No flow executes. FinalHumanResult: null."
  Note: Gap vocabulary proportion does not affect ContextCarryForward. One predicate → true.

---

## B36-T1-024-003 — Three-predicate transition: IMPLEMENT+DOCUMENT+QA

A:
  run_id: "B36-T1-024-003"
  user_request: "In this sprint: build the feature, document the requirements, and review the output."
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "QA"]
  PredictedSubPhaseId: "4.1"

B:
  TransitionId: "transition-B36-T1-024-003-4.1-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT", "DOCUMENT", "QA"]
  ContextCarryForward: true
  Metadata:
    secondary_sub_phases: ["2.4", "6.1"]
    execution_model: "parallel_4.1_2.4_then_6.1 (QA-last)"
    capability_gap_warnings: ["gap-project-planning-sprint-001"]

---

## B36-T1-024-004 — DomainId boundary: one planning word + one predicate word

A:
  Scenario: "Two versions of the same request — one word changes DomainId and TransitionType."

B:
  Version1:
    request: "Plan the sprint and groom the backlog."
    predicates: []
    TransitionType: "unknown"
    DomainId: "project-management"
    ContextCarryForward: false

  Version2:
    request: "Plan the sprint and build the backlog tracker."
    predicates: ["IMPLEMENT"]
    TransitionType: "feature_addition"
    DomainId: "software-development"
    ContextCarryForward: true

  Teaching: "groom" is gap → unknown + project-management.
    "build" fires IMPLEMENT → feature_addition + software-development.
    The gap vocabulary (sprint, backlog) is present in both versions. The single
    change of "groom" to "build" completely changes the orchestrator output.
