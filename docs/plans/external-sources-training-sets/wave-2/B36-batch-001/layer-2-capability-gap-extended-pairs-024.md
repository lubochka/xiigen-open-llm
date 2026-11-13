# Layer 2 — Capability Gap Extended Pairs (024)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B36-L2-024-001 — Complete B36 gap vocabulary + DomainId rule summary

A: "What is the complete B36 gap vocabulary list and the DomainId rule?"

B:
  GapVocabulary (all produce knowledge_edge when no predicate fires):
    sprint, roadmap, milestone, backlog, velocity, standup, retrospective,
    burndown, capacity (team), iteration, epic, ticket, story-point (compound),
    estimation, release-cycle, planning-poker, effort-estimate, grooming,
    triage, prioritize, refinement, product-roadmap

  DomainIdRule:
    CompiledPredicateIds: [] → DomainId: "project-management"
    Any predicate in CompiledPredicateIds → DomainId: "software-development"

  Hard rule: One predicate token in the sentence forces DomainId: "software-development"
    regardless of how many gap terms are present.

---

## B36-L2-024-002 — Tokens that fire predicates in gap sentences

A: "Which tokens fire predicates even when the sentence context is project planning?"

B:
  AlwaysFires:
    review → QA (even "sprint review", "velocity review", "milestone review")
    approve → QA
    test → TEST (even "test the estimate")
    story → DOCUMENT (even "story points", "user stories")
    stories → DOCUMENT
    document → DOCUMENT
    define → DOCUMENT
    specify → DOCUMENT
    requirements → DOCUMENT
    stakeholder → DOCUMENT
    criteria → DOCUMENT
    use-case → DOCUMENT
    functional → DOCUMENT
    behavior → DOCUMENT
    implement → IMPLEMENT
    feature → IMPLEMENT
    build → IMPLEMENT (with or without planning context)
    create (with code entity) → IMPLEMENT
    write (with code entity) → IMPLEMENT
    design → ARCHITECTURE
    architect → ARCHITECTURE
    structure → ARCHITECTURE
    architecture → ARCHITECTURE

  NeverFires (common planning tokens):
    plan, groom, facilitate, track, run (ceremony), generate, capture,
    estimate, triage, prioritize, refine, align, facilitate, present,
    assign (non-code), update (planning only), capacity (team planning)

---

## B36-L2-024-003 — Gap ID naming convention

A: "State the gap ID naming convention for B36 gap terms."

B:
  Convention: "gap-{domain}-{term(s)}-{sequence}"
  Domain: "project-planning"
  Sequence: starts at 001

  PrimaryIds:
    gap-project-planning-sprint-001
    gap-project-planning-roadmap-001
    gap-project-planning-milestone-001
    gap-project-planning-backlog-001
    gap-project-planning-velocity-001
    gap-project-planning-standup-001
    gap-project-planning-retrospective-001
    gap-project-planning-burndown-001
    gap-project-planning-capacity-001
    gap-project-planning-iteration-001
    gap-project-planning-epic-001
    gap-project-planning-ticket-001
    gap-project-planning-story-point-001
    gap-project-planning-estimation-001
    gap-project-planning-release-cycle-001

  CompoundIds (multi-term co-occurrence):
    gap-project-planning-sprint-roadmap-001
    gap-project-planning-standup-retrospective-001
    gap-project-planning-burndown-milestone-tracker-001
    gap-project-planning-velocity-retrospective-001
    gap-project-planning-backlog-estimation
    gap-project-planning-sprint-not-captured

---

## B36-L2-024-004 — SCHEDULE_CAPABILITY_LEARNING rules summary

A: "When is SCHEDULE_CAPABILITY_LEARNING emitted and what format does it take?"

B:
  EmitWhen: "CompiledPredicateIds: [] AND known gap vocabulary is present"
  DoNotEmitWhen: "Any predicate fires (even DOCUMENT via 'story' false positive)"
  Format:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: [list of all gap IDs detected in the request]
  Rule: One signal per request. Multiple gap IDs → one signal with all IDs bundled.
  Consumer: AutonomousLearningPipeline (subproject 16)

---

## B36-L2-024-005 — B36 complete predicate boundary: the 5 tokens to memorize

A: "What are the five most important boundary tokens in B36?"

B:
  CriticalBoundaryTokens:
    1. "review" → QA (fires in ANY context — planning, retrospective, velocity, sprint)
    2. "story" → DOCUMENT (fires in "story points", "user story", "stories" — false positive in planning)
    3. "retrospective" → gap (NEVER fires QA — distinct from "review")
    4. "capacity" (team) → gap (NEVER fires ARCHITECTURE — distinct from system capacity)
    5. "update" (planning) → gap (fires IMPLEMENT only with code-level entity co-occurrence)

  Mnemonic: "review always fires, story always fires, retrospective never fires QA,
    capacity never fires ARCHITECTURE, update only fires with code entity."
