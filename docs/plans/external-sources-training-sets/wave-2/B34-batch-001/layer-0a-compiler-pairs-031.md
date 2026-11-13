# Layer 0-A — Compiler Pairs (031)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-031-001 — "show how to" + UI context

A: "Show the design team how to use our React (UI library) component library."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "show how to use" = teaching/demonstration vocabulary → gap. Using existing components = knowledge transfer, not code creation.

---

## B34-compiler-031-002 — "introduce" + team + onboarding context → onboard gap

A: "Introduce the new hire to the team's coding standards and review processes."
B: Predicates: [] | Gap: gap-skills-guidance-onboard-001
Evidence: "introduce the new hire to" = onboarding vocabulary → gap-skills-guidance-onboard-001. New hire + standards/processes = orientation context confirms onboard gap ID.

---

## B34-compiler-031-003 — "familiarize" + code embedded

A: "Familiarize the team with the new logging framework and update the existing services to use it."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "familiarize with" = gap warning (gap-skills-guidance-onboard-001); "update the existing services to use it" = requester's direct modification action → IMPLEMENT fires.

---

## B34-compiler-031-004 — "run through" + API topic → gap

A: "Run through the API (Application Programming Interface) versioning strategy with the backend team."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "run through with [team]" = explanation/guidance → gap. API versioning = knowledge topic, not a direct API action by requester.

---

## B34-compiler-031-005 — "brief" + DOCUMENT topic → gap (not DOCUMENT)

A: "Brief stakeholders on the technical documentation standards we use."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "brief on" = teaching vocabulary → gap. Documentation standards = knowledge topic; "brief" frames it as communication/teaching → gap. Even if the topic involves DOCUMENT nouns, "brief on" keeps it gap.
