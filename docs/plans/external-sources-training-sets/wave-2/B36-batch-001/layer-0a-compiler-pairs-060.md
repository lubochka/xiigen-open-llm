# Layer 0-A — Compiler Pairs (060)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-060-001 — Complete Round 3 boundary reference (037–060)

A:
  run_id: "B36-0A-060-001"
  Task: "Complete boundary table for all Round 3 (037–060) gap vocabulary."

B:
  BoundaryTable_Round3_Complete:
    GAP only (zero predicates possible):
      scrum ceremony, daily standup, sprint demo (alone), backlog refinement, product increment,
      MVP definition (alone), launch readiness (alone), go-live checklist (alone),
      feature flag (no code verb), OKRs (Objectives and Key Results), KPIs (Key Performance Indicators),
      north-star metric, impact mapping, t-shirt sizing, RICE scoring (Reach, Impact, Confidence, Effort),
      MoSCoW (Must/Should/Could/Won't), now-next-later, hypothesis, experiment (planning, no "test")

    Fires DOCUMENT:
      - "story" in story splitting / user stories → DOCUMENT (false positive)
      - "criteria" in release criteria / acceptance criteria → DOCUMENT
      - "define", "specify", "document", "requirements", "stakeholders" → DOCUMENT

    Fires QA:
      - "review" (even "sprint review") → QA
      - "approve" → QA

    Fires IMPLEMENT:
      - "build", "implement", "create" + code entity → IMPLEMENT
      - "feature flag" + code verb → IMPLEMENT

    Fires TEST:
      - "test" (literal) → TEST
      - "experiment" alone → gap (never TEST)

    Fires ARCHITECTURE:
      - "design", "architect", "structure" + context → ARCHITECTURE

---

## B36-0A-060-002 — "all six predicates" combo with gap vocabulary

A:
  run_id: "B36-0A-060-002"
  user_request: "Design the architecture, implement the feature flag, test the hypothesis, document OKRs, review the sprint demo, and build the KPI dashboard."
  Task: "Identify all predicates."

B:
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT", "QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-feature-flag-001"
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-sprint-demo-001"
    - "gap-project-planning-kpi-001"
  Teaching: Five predicates fire. All gap vocabulary produces warnings.
    DomainId: "software-development". QA-last applies. SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-060-003 — "RICE + impact mapping + story splitting + IMPLEMENT + QA" full combo

A:
  run_id: "B36-0A-060-003"
  user_request: "Review RICE scoring (Reach, Impact, Confidence, Effort) after story splitting and implement the top impact-mapped features."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-story-splitting-001"
    - "gap-project-planning-impact-mapping-001"
  Teaching: "review" fires QA. "story" fires DOCUMENT. "implement" fires IMPLEMENT.
    QA-last applies. DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-060-004 — Complete DomainId rule summary for all 60 files

A:
  run_id: "B36-0A-060-004"
  Task: "State the single DomainId rule that applies across all 60 layer-0a files."

B:
  DomainIdRule_Canonical:
    CompiledPredicateIds: [] → DomainId: "project-management"
    Any entry in CompiledPredicateIds → DomainId: "software-development"

  Applies to: all B36 gap vocabulary (sprint, backlog, velocity, burndown, OKRs, KPIs,
    north-star metric, RICE, MoSCoW, story splitting, t-shirt sizing, hypothesis, experiment,
    scrum ceremony, daily standup, sprint demo, backlog refinement, product increment,
    MVP, launch readiness, go-live checklist, feature flag, now-next-later, impact mapping)

  Hard rule: "Gap vocabulary quantity is irrelevant. One predicate token → 'software-development'. Always."

---

## B36-0A-060-005 — SCHEDULE_CAPABILITY_LEARNING final canonical rule

A:
  run_id: "B36-0A-060-005"
  Task: "State the canonical SCHEDULE_CAPABILITY_LEARNING rule for all B36 pairs (001–060)."

B:
  CanonicalRule:
    Emit when: "CompiledPredicateIds: [] AND known gap vocabulary detected"
    Do NOT emit when: "Any predicate fires (even false positive DOCUMENT via 'story')"
    Format: "One signal per request. All gap IDs bundled. Never two signals for same request."
    Consumer: "AutonomousLearningPipeline (subproject 16)"

  Edge cases confirmed across all 60 files:
    - "story" fires DOCUMENT → NO signal emitted
    - "review" fires QA in sprint context → NO signal emitted
    - "experiment" without "test" → signal emitted (experiment is gap)
    - "t-shirt sizing" + "review" → QA fires → NO signal emitted
    - "OKRs" + "KPIs" + "north-star" (no predicate) → ONE signal with all three IDs
