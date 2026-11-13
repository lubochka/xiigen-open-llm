# Held-Out and Corruption Pairs (007)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-007-A — Held-out: "upskill on" → gap

A: "Upskill the frontend team on TypeScript generics."
B: Predicates: [] | GapHandler → gap-skills-guidance-train-001
Reason: "upskill" = training vocabulary → gap.
Phrasing-B: Gap — upskilling is human learning vocabulary, not a code-action verb.

---

## HC-007-B — Held-out: five-verb mixed request

A: "Mentor the team, build the LMS, integrate with HR API, write tests, and document the API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
Reason: mentor→gap; build→IMPLEMENT; integrate→API_CONTRACT; write tests→TEST; document→DOCUMENT.
TransitionType: feature_addition | GapHandler NOT invoked.
Phrasing-B: Four predicates fire — guidance verb "mentor" becomes a gap warning; four code verbs fire their predicates.

---

## HC-007-C — Held-out: "shadow" → gap

A: "Shadow the DevOps team for a week to learn the deployment process."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "shadow" = observation/mentoring vocabulary → gap.

---

## HC-007-D — Corruption v6-01: ONBOARDING predicate invented

v6 corrupt: "Onboard new engineers to the payments team" → Predicates: ["ONBOARDING"] → OnboardingFlow
Correct: Predicates: [] → GapHandler → gap-skills-guidance-onboard-001
Error type: v6-01 — ONBOARDING is not a predicate; OnboardingFlow does not exist.
