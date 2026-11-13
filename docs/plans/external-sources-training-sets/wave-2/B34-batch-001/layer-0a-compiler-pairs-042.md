# Layer 0-A — Compiler Pairs (042)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-042-001 — guidance + UI (User Interface) topic = gap

A: "Explain the UI (User Interface) component library to the new frontend developers."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: explain = always gap. UI topic does not fire any UI predicate. Topic is learning content only.

---

## B34-compiler-042-002 — guidance + UI + build action

A: "Walk the team through the UI (User Interface) design system and build the new dashboard components."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "walk team through design system" = gap warning; "build the new dashboard components" = requester's IMPLEMENT action → IMPLEMENT fires.

---

## B34-compiler-042-003 — guidance + ARCHITECTURE topic = gap

A: "Show the engineering team how the hexagonal architecture is structured."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: "show how [architecture] is structured" = teaching frame → gap. Architecture topic does not trigger ARCHITECTURE predicate when framed as explanation.

---

## B34-compiler-042-004 — "discuss" + ARCHITECTURE action

A: "Discuss the hexagonal architecture with the team and architect the service boundaries."
B: Predicates: ["ARCHITECTURE"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "discuss with team" = gap warning; "architect the service boundaries" = requester's ARCHITECTURE action → ARCHITECTURE fires.

---

## B34-compiler-042-005 — multiple guidance verbs, no code action = single gap

A: "Introduce, brief, and run through the new CI/CD (Continuous Integration/Continuous Deployment) workflow with the DevOps (Development Operations) team."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: introduce + brief + run through = all guidance vocabulary → gap. Single SCHEDULE_CAPABILITY_LEARNING emitted. No gap emitted multiple times.
