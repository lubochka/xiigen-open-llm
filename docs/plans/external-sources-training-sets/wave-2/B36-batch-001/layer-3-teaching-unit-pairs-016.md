# Layer 3 — Teaching Unit Pairs (016)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B36-L3-016-001 — Product-roadmap: why "present" does not fire

A:
  Question: "Why does 'Present the product roadmap to the board' produce knowledge-edge?"

B:
  Answer: "'present' is not in any predicate lexicon. 'product roadmap' is gap vocabulary.
    'milestones' are gap vocabulary. Zero predicates → knowledge-edge. DomainId: project-management.
    The model must not invent a DOCUMENT predicate for 'present' or 'roadmap'."

---

## B36-L3-016-002 — Product-roadmap: "design" fires ARCHITECTURE even for roadmap

A:
  Question: "A user says: 'Design the product roadmap structure.' Does ARCHITECTURE fire?"

B:
  Answer: "Yes. 'design' fires ARCHITECTURE unconditionally — even in a roadmap planning
    sentence. ARCHITECTURE routes to Phase 3 (Technical Design), not a project planning
    sub-phase. 'product roadmap' is a gap warning. DomainId: software-development.
    This is the same pattern as 'design the sprint capacity model' in earlier pairs."

---

## B36-L3-016-003 — Product-roadmap five gap terms: knowledge-edge with bundled signal

A:
  Question: "How does SCHEDULE_CAPABILITY_LEARNING work when five roadmap gap terms appear?"

B:
  Answer: "One signal is emitted with all five gap IDs bundled. The signal contains:
    { signal: 'SCHEDULE_CAPABILITY_LEARNING', capabilityGapIds: [roadmap-001, sprint-001,
    velocity-001, epic-001, burndown-001] }. Five gap IDs → one signal (Rule 22).
    The AutonomousLearningPipeline processes the entire set together."

---

## B36-L3-016-004 — DomainId: roadmap alone vs roadmap + review

A:
  Question: "Two sentences: 'Align the product roadmap.' vs 'Review the product roadmap.'
             How does DomainId differ?"

B:
  Answer: "Sentence 1: 'align' is not a predicate. 'roadmap' is gap. Zero predicates →
    DomainId: 'project-management'. knowledge_edge. SCHEDULE_CAPABILITY_LEARNING emitted.

    Sentence 2: 'review' fires QA. One predicate → DomainId: 'software-development'.
    Routes to Phase 6. Gap warning for roadmap-001. SCHEDULE_CAPABILITY_LEARNING NOT emitted.

    One word ('align' → 'review') changes both DomainId and OverallVerdict."
