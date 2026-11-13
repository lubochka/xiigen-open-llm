# Layer 0-B — Router Pairs (006)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: router

---

## B34-router-006-001 — Gap route: "inspire" → gap

A: "Inspire the team to embrace continuous learning."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001

---

## B34-router-006-002 — ARCHITECTURE route: "architect" embedded

A: "Teach the team how we architect our systems and architect the new payment service."
Analysis: "teach how we architect" = guidance frame (gap); "architect the new payment service" = direct action (ARCHITECTURE).
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31 (3.1)
Evidence: Direct action verb "architect [artifact]" fires ARCHITECTURE even within a mixed teaching context.

---

## B34-router-006-003 — IMPLEMENT+QA route

A: "Build the developer onboarding portal and QA the user registration flow."
Router decision: Predicates = ["IMPLEMENT", "QA"] → ImplementationFlow41 → QAGatingFlow73
TransitionType: feature_addition. QA always last.

---

## B34-router-006-004 — Gap route: "workshop" → gap

A: "Run a workshop on API testing best practices."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "run a workshop" = training event vocabulary → gap. "API testing" without test-writing verb → gap.

---

## B34-router-006-005 — v6 corruption: multi-gap collapsed to DOCUMENT

v6 corrupt: "Mentor, guide, teach the team" → Predicates: ["DOCUMENT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-03 (multi-predicate collapse or gap→predicate invention). Mentor/guide/teach are all gap → zero predicates.
