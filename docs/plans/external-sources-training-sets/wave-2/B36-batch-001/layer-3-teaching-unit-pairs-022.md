# Layer 3 — Teaching Unit Pairs (022)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B36-L3-022-001 — Roadmap + DOCUMENT: correct documentation routing

A:
  Question: "'Document the roadmap items and specify the requirements.' What fires and why is it correct?"

B:
  Answer: "DOCUMENT fires on 'document', 'specify', 'requirements'. This is correct in
    both lexicon and intent terms — the user explicitly wants documentation output.
    'Roadmap' and 'milestones' are gap warnings (roadmap-001, milestone-001). Routes to
    2.4 (stakeholder requirements context). DomainId: software-development."

---

## B36-L3-022-002 — Backlog + DOCUMENT: "functional specifications"

A:
  Question: "'Define the backlog requirements and document the functional specifications.'
             Which sub-phase is selected and why?"

B:
  Answer: "DOCUMENT fires on 'define', 'requirements', 'document', 'functional', 'specifications'.
    Routes to 2.1 (Business Requirements and Use Cases) because 'functional specifications'
    is a business requirements artifact. 'backlog' and 'epic' are gap warnings. Routes to 2.1
    rather than 2.4 because functional specifications are internal requirements (not external
    stakeholder requirements)."

---

## B36-L3-022-003 — Velocity + DOCUMENT: planning metric documentation

A:
  Question: "'Document the team velocity metrics.' Does this produce knowledge-edge?"

B:
  Answer: "No. 'document' fires DOCUMENT. Even though velocity is gap vocabulary (velocity-001),
    DOCUMENT fires via 'document'. Routes to 2.1. DomainId: software-development. Gap warnings
    for velocity-001, sprint-001, capacity-001. SCHEDULE_CAPABILITY_LEARNING NOT emitted
    because DOCUMENT fired."

---

## B36-L3-022-004 — Milestone + DOCUMENT (use-case): 2.1 selection

A:
  Question: "'Describe deliverables for each milestone and define the use-case.' Why 2.1 not 2.4?"

B:
  Answer: "'use-case' is a primary signal for 2.1 (Business Requirements and Use Cases).
    'describe' and 'define' fire DOCUMENT. 'use-case' reinforces 2.1 over 2.4. 'milestone',
    'roadmap', 'epic' are gap warnings. Routes to 2.1. DomainId: software-development."
