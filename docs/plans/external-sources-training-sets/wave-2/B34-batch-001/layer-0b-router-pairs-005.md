# Layer 0-B — Router Pairs (005)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: router

---

## B34-router-005-001 — Gap route: "help the team"

A: "Help the team understand the event-driven architecture."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "help understand" = instructional/guidance verb → gap.

---

## B34-router-005-002 — DOCUMENT route: "write" + technical guide

A: "Write a technical guide for the new engineering team on our microservices architecture."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow
Evidence: "write a technical guide" → DOCUMENT. "new engineering team" = audience noun, not predicate.

---

## B34-router-005-003 — IMPLEMENT route: "create" + software component

A: "Create the peer review tracking system for the mentorship program."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Evidence: "create the [noun] system" = IMPLEMENT (software system artifact). "mentorship program" = context, not predicate suppressor.

---

## B34-router-005-004 — API_CONTRACT+IMPLEMENT route

A: "Integrate the training platform with Slack and build the notification service."
Router decision: Predicates = ["API_CONTRACT", "IMPLEMENT"] → APIContractFlow33 + ImplementationFlow41
TransitionType: feature_addition

---

## B34-router-005-005 — Gap route: "clarify" → gap

A: "Clarify the deployment process for the junior developers."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "clarify" = instructional/explanatory vocabulary → gap.
