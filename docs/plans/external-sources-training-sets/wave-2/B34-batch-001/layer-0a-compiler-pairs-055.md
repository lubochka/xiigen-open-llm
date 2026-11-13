# Layer 0-A — Compiler Pairs (055)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-055-001 — "show how to" + "demonstrate how to" stacked = single gap

A: "Show and demonstrate how to write idiomatic Go (Go programming language) services to the backend team."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: show how to + demonstrate how to = both training vocabulary → gap-skills-guidance-train-001. Single gap emitted.

---

## B34-compiler-055-002 — "introduce" (person) + "demonstrate" = single gap

A: "Introduce the QA (Quality Assurance) team to our testing framework and demonstrate the test runner workflow."
B: Predicates: [] | Gap: gap-skills-guidance-onboard-001
Rule: introduce [person] + demonstrate = both guidance vocabulary → gap-skills-guidance-onboard-001 (introduce implies onboarding).

---

## B34-compiler-055-003 — "brief" + "run through" + "present" = single gap

A: "Brief the stakeholders, run through the technical approach, and present the delivery timeline."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: brief + run through + present = all guidance vocabulary → gap-skills-guidance-mentor-001. Single SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B34-compiler-055-004 — all advisory vocabulary stacked = single gap

A: "Consult with the team, get a second opinion from the architect, and use the senior engineers as a sounding board."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: consult + second opinion + sounding board = all advisory vocabulary → gap-skills-guidance-mentor-001. Single gap. No multiplied SCHEDULE_CAPABILITY_LEARNING.

---

## B34-compiler-055-005 — Round 3 complete corruption reference

v6-01 (invented predicates): show how to → TUTORIAL; demonstrate → DEMO; consult → CONSULT; brief → BRIEFING
v6-02 (gap suppression): consult on architecture → ARCHITECTURE; get feedback on security → SECURITY; sounding board for design → ARCHITECTURE
v6-03 (predicate loss): "show how to, then build" → IMPLEMENT fires but gap warning lost; "get feedback on docs, then write docs" → DOCUMENT fires but gap warning lost
