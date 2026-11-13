# Layer 0-C — Knowledge Edge Pairs (048)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-048-001 — "v6 corruption edge: topic predicate injection"

v6-02 corruption pattern: topic predicate injected when guidance vocabulary present.
- "brief on architecture" → corrupt: ARCHITECTURE; correct: gap
- "discuss security" → corrupt: SECURITY; correct: gap
- "consult on QA" → corrupt: QA; correct: gap
- "present the API design" → corrupt: API_CONTRACT; correct: gap
- "run through the test suite" → corrupt: TEST; correct: gap
All topic predicate injections are v6-02 errors. Guidance vocabulary = gap, not topic predicate.

---

## KE-048-002 — "v6 corruption edge: gap warning omission"

v6-03 corruption pattern: guidance vocabulary present alongside code action, but gap warning omitted.
- "explain design, then implement service" → corrupt: ["IMPLEMENT"] only; correct: ["IMPLEMENT"] + GapWarning
- "brief team, then harden API" → corrupt: ["SECURITY"] only; correct: ["SECURITY"] + GapWarning
- "consult on tests, then write tests" → corrupt: ["TEST"] only; correct: ["TEST"] + GapWarning
- "rubber duck design, then integrate service" → corrupt: ["API_CONTRACT"] only; correct: ["API_CONTRACT"] + GapWarning
All gap warning omissions are v6-03 errors. Guidance vocabulary alongside code action always emits gap warning.

---

## KE-048-003 — "v6 corruption edge: SCHEDULE_CAPABILITY_LEARNING on wrong trigger"

v6-04 corruption pattern: SCHEDULE_CAPABILITY_LEARNING emitted when code predicate present.
Corrupt: "explain design, then implement service" → IMPLEMENT fires AND SCHEDULE_CAPABILITY_LEARNING emitted
Correct: IMPLEMENT fires; CapabilityGapWarning emitted; SCHEDULE_CAPABILITY_LEARNING NOT emitted
Rule: SCHEDULE_CAPABILITY_LEARNING is ONLY emitted when predicate set = [] (full gap). Never when code predicate fires.

---

## KE-048-004 — "v6 corruption edge: multiple SCHEDULE_CAPABILITY_LEARNING emissions"

v6-04 corruption pattern: SCHEDULE_CAPABILITY_LEARNING emitted once per guidance verb.
Corrupt: "mentor, coach, explain to team" → SCHEDULE_CAPABILITY_LEARNING × 3
Correct: → GapHandler × 1 → SCHEDULE_CAPABILITY_LEARNING × 1
Rule: exactly one SCHEDULE_CAPABILITY_LEARNING per full-gap request, regardless of how many guidance verbs appear.

---

## KE-048-005 — "v6 corruption edge: wrong DomainId"

v6-05 corruption pattern: DomainId is education, training, HR, or learning-management instead of software-development.
Corrupt: gap-skills-guidance-mentor-001 with DomainId: education
Correct: gap-skills-guidance-mentor-001 with DomainId: software-development
Corrupt: gap-skills-guidance-train-001 with DomainId: training
Correct: gap-skills-guidance-train-001 with DomainId: software-development
Rule: DomainId is ALWAYS software-development for ALL B34 gap events.
