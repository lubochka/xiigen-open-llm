# Layer 0-A — Compiler Pairs (052)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-052-001 — complete gap ID selection reference

Gap ID selection priority:
1. gap-skills-guidance-onboard-001 — selected when: introduce [person], familiarize with, onboard, ramp-up, new hire context
2. gap-skills-guidance-train-001 — selected when: train, teach, show how to, demonstrate, explain, workshop, present
3. gap-skills-guidance-mentor-001 — selected when: mentor, coach, guide, advise, facilitate, shadow, pair-with, walk-through, review with, brief, run through, discuss, consult, seek advice, get feedback, sanity check, second opinion, rubber duck, sounding board

In ambiguous cases: gap-skills-guidance-mentor-001 is the default (most general mentoring category).

---

## B34-compiler-052-002 — "onboard" = onboard-001; "mentor" = mentor-001

A: "Onboard the new engineers to the platform team and mentor them through their first sprint."
B: Predicates: [] | Gap: gap-skills-guidance-onboard-001
Rule: onboard → onboard-001 (priority); mentor → mentor-001 (secondary). Most specific = onboard-001 selected.

---

## B34-compiler-052-003 — "train" = train-001 priority

A: "Train the developers on Kubernetes (K8s) cluster management."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: train [human target] = train-001 (note: "train [model]" = entirely different, not in B34 scope).

---

## B34-compiler-052-004 — "mentor" + "train" = mentor-001 as most specific for combined mentoring context

A: "Mentor and train the junior team throughout the quarter."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: mentor + train = combined; mentor-001 selected as most specific for ongoing mentoring relationship context.

---

## B34-compiler-052-005 — advisory vocabulary = mentor-001 default

A: "Rubber duck the system design and get a second opinion on the scaling approach."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Rule: rubber duck + second opinion = advisory vocabulary → mentor-001 (advisory = mentoring category).
