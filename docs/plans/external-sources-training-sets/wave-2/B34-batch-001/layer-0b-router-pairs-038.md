# Layer 0-B — Router Pairs (038)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-038-001 — "introduce [feature] into" routing = ImplementationFlow41

A: "Introduce pagination into the search results API (Application Programming Interface)."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | TransitionType: feature_addition
Evidence: "introduce [feature] into [system]" = feature addition → IMPLEMENT. No gap.

---

## B34-router-038-002 — "introduce" disambiguation routing

A: "Introduce the team to the new pagination feature and introduce pagination into the search API (Application Programming Interface)."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-onboard-001
Evidence: "introduce team to feature" = gap warning (onboard-001); "introduce pagination into search API" = IMPLEMENT → ImplementationFlow41.

---

## B34-router-038-003 — "brief" routing = GapHandler

A: "Brief the product managers on the technical constraints for the upcoming sprint."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "brief [person] on [topic]" = knowledge-transfer vocabulary → GapHandler. No code flow selected.

---

## B34-router-038-004 — "familiarize with" routing = GapHandler

A: "Familiarize the frontend team with the design token system."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-onboard-001
Evidence: "familiarize [person] with [system]" = onboarding vocabulary → GapHandler. gap-skills-guidance-onboard-001 selected.

---

## B34-router-038-005 — "run through" routing = GapHandler

A: "Run through the pull request review process with the junior developers."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "run through [process] with [person]" = explanation vocabulary → GapHandler invoked.
