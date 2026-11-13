# Layer 0-B — Router Pairs (012)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: router

---

## B34-router-012-001 — Gap route: "celebrate" → gap

A: "Celebrate the team's learning milestones and encourage continued growth."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001

---

## B34-router-012-002 — DOCUMENT route: "spec out"

A: "Spec out the developer onboarding process with step-by-step instructions."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow
Evidence: "spec out" = DOCUMENT trigger (specification action); "step-by-step instructions" = documentation artifact.

---

## B34-router-012-003 — IMPLEMENT+TEST+DOCUMENT three-flow

A: "Build the skill gap tracker, write tests for the assessment logic, and document the scoring algorithm."
Router decision: Predicates = ["IMPLEMENT", "TEST", "DOCUMENT"] → ImplementationFlow41 → UnitTestingFlow51 ‖ DocumentationFlow
TransitionType: feature_addition

---

## B34-router-012-004 — Gap route: "shadow" → gap

A: "Shadow the senior SRE (Site Reliability Engineer) during the on-call rotation."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001

---

## B34-router-012-005 — v6 corruption: QA invented for "quality of training"

v6 corrupt: "Ensure the quality of the training program" → Predicates: ["QA"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-02. "ensure quality" in training context ≠ QA predicate. QA fires ONLY on "QA" abbreviation. "quality" spelled out → gap.
