# Layer 0-B — Router Pairs (015)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-015-001 — Gap route: "walk through" → gap

A: "Walk the new hires through the deployment process step by step."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-onboard-001

---

## B34-router-015-002 — API_CONTRACT route: "walk through integrating"

A: "Walk through integrating the Twilio SMS (Short Message Service) API with our notification service."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33
Evidence: "integrating" fires API_CONTRACT; "walk through" = gap warning.

---

## B34-router-015-003 — Gap route: "walk through the API" (explain intent)

A: "Walk the team through the REST API endpoints we built last sprint."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "walk through [existing artifact]" = explain/present → gap. No integration verb. No DOCUMENT fires from "walk through."

---

## B34-router-015-004 — IMPLEMENT route: "walk through building"

A: "Walk through building the GraphQL (Graph Query Language) schema resolver and implement it."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Evidence: "implement" = direct action → IMPLEMENT; "walk through building" = gap warning alongside IMPLEMENT.

---

## B34-router-015-005 — ARCHITECTURE route: "walk through architecting" → embedded verb fires

A: "Walk through architecting the event-driven system and design the component diagram."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31
Evidence: "architecting" + "design the component diagram" → ARCHITECTURE; "walk through" = gap warning.
