# Layer 0-A — Compiler Pairs (005)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## B34-compiler-005-001 — "code review" mentoring → gap

A: "Help the team improve their code review skills."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "help improve" = coaching vocabulary; "code review skills" = skill-building vocabulary → gap.

---

## B34-compiler-005-002 — "write" + knowledge article → DOCUMENT

A: "Write a knowledge base article on our API design standards."
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Evidence: "write" + "knowledge base article" → DOCUMENT (write + documentation artifact).

---

## B34-compiler-005-003 — "implement" + skill tracking → IMPLEMENT

A: "Implement the skill assessment tracker for the engineering team."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "implement" = code verb; "skill assessment tracker" = software component. Code verb overrides guidance object.

---

## B34-compiler-005-004 — "QA" + review process → QA

A: "QA the developer onboarding review process."
B: Predicates: ["QA"] | QAGatingFlow73 | TransitionType: no contribution
Evidence: "QA" abbreviation fires QA; "onboarding review process" = object (feature acceptance gate for process).

---

## B34-compiler-005-005 — "shadow" mentoring → gap

A: "Have junior developers shadow senior engineers during sprint planning."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "shadow" = mentoring/observation vocabulary; "sprint planning" = PM vocabulary (not QA trigger) → gap.
