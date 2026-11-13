# Layer 0-B — Router Pairs (025)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-025-001 — "show how to" routing

A: "Show the team how to configure the distributed cache."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "show how to [infinitive]" = learning frame → gap. Infinitive "to configure" = what others will learn, not requester's action.

---

## B34-router-025-002 — "demonstrate" routing

A: "Demonstrate the CI/CD (Continuous Integration/Continuous Delivery) pipeline to the new developers."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: demonstrate = teaching vocabulary → gap. GapHandler invoked + SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B34-router-025-003 — "introduce" routing: person vs feature

A: "Introduce the new engineers to our deployment workflow."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-onboard-001
Evidence: "introduce [person] to [workflow]" = onboarding context → gap-skills-guidance-onboard-001.

A: "Introduce the feature flag service into the platform."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | TransitionType: feature_addition
Evidence: "introduce [feature] into [system]" = software insertion → IMPLEMENT fires. No gap.

---

## B34-router-025-004 — "brief the team on" routing

A: "Brief the engineering team on the upcoming infrastructure changes."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "brief [team] on [topic]" = information-sharing vocabulary → gap. GapHandler invoked.

---

## B34-router-025-005 — "familiarize with" routing

A: "Familiarize the new backend developers with our API (Application Programming Interface) design standards."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-onboard-001
Evidence: "familiarize with" + new developers context → onboarding vocabulary → gap-skills-guidance-onboard-001.
