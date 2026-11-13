# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-011-001 — v6 corruption: QA invented for "quality of training"

v6 corrupt: "Ensure the quality of our onboarding program" → ["QA"] → QAGatingFlow73
Correct: [] → GapHandler → gap-skills-guidance-onboard-001
Error type: v6-02. "ensure quality" ≠ QA abbreviation. "quality assurance" spelled out → gap. QA fires ONLY on "QA" abbreviation.

---

## TO-011-002 — v6 corruption: "teach to write tests" fires TEST

v6 corrupt: "Teach the juniors to write unit tests" → ["TEST"] → UnitTestingFlow51
Correct: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. Requester action = "teach" = gap. The infinitive "to write tests" is what others will learn; not a direct requester action.

---

## TO-011-003 — v6 corruption: DOCUMENT invented for "explain"

v6 corrupt: "Explain the CI/CD pipeline to the team" → ["DOCUMENT"] → DocumentationFlow
Correct: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "explain" is always gap. Explanation ≠ documentation action.

---

## TO-011-004 — Correct: "write a guide on" fires DOCUMENT

"Write a guide on the CI/CD pipeline for new developers" → ["DOCUMENT"] → DocumentationFlow ✓
"Explain the CI/CD pipeline to the team" → [] → GapHandler ✓
Distinction: write a guide = requester creates documentation artifact (DOCUMENT); explain = requester teaches (gap).

---

## TO-011-005 — Gap: three-gap-category request

Request: "Train the team, mentor the leads, and onboard new hires."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001 (mentor-001 priority)
Three gap categories present; mentor category wins priority; single SCHEDULE_CAPABILITY_LEARNING.
