# Layer 3 — Teaching Unit Pairs (015)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B36-L3-015-001 — Release-planning: why "create" does not fire IMPLEMENT

A:
  Question: "Why does 'Create the release plan for Q4' produce knowledge-edge instead
             of routing to IMPLEMENT?"

B:
  Answer: "'create' fires IMPLEMENT only when co-occurring with a code-level entity
    (function, class, module, service, component). 'release plan' is not a code entity —
    it is a project planning artifact. No code-level entity → 'create' does not fire
    IMPLEMENT. Zero predicates → knowledge-edge."

  Rule: "create" + code entity → IMPLEMENT
  Rule: "create" + planning artifact → no predicate (gap)

---

## B36-L3-015-002 — Release-planning: "release" ≠ DOCUMENT

A:
  Question: "Does 'release' fire DOCUMENT? Does 'release plan' fire any predicate?"

B:
  Answer: "'release' is not in the DOCUMENT lexicon. 'release plan' is not in any predicate
    lexicon. The word 'release' is project management vocabulary (gap). Compare: 'release
    notes document' fires DOCUMENT via 'document' — not via 'release'. 'release' alone is
    always gap. The model must not invent a DOCUMENT predicate for 'release plan'."

---

## B36-L3-015-003 — Release-planning gap ID

A:
  Question: "What gap ID covers release planning requests?"

B:
  Answer: "gap-project-planning-release-cycle-001. This ID covers: release plan, release
    planning, release cycle, release schedule. It signals to AutonomousLearningPipeline
    that release planning vocabulary needs a registered sub-phase or predicate."

---

## B36-L3-015-004 — Release-planning + review: QA fires, gap warning emitted

A:
  Question: "A user says: 'Review the release plan and confirm milestone dates.' What fires?"

B:
  Answer: "QA fires on 'review'. DOCUMENT fires on 'stakeholders' if present; in this
    sentence 'stakeholders' is absent. QA fires on 'review' alone. 'release plan' and
    'milestones' are gap vocabulary producing gap warnings (release-cycle-001, milestone-001).
    DomainId: software-development. Routes to Phase 6."
