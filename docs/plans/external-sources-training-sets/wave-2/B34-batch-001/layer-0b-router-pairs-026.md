# Layer 0-B — Router Pairs (026)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-026-001 — "run through" routing

A: "Run through the branching strategy with the junior developers."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "run through with [team]" = walkthrough/explanation → gap. No code action by requester.

---

## B34-router-026-002 — "run through" + code action routing

A: "Run through the deployment steps and deploy the hotfix to production."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "run through" = gap warning; "deploy to production" = requester's action → IMPLEMENT fires. GapHandler suppressed.

---

## B34-router-026-003 — "review with" routing

A: "Review the new API contract with the backend team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "review with [people]" = collaborative guidance → gap. No fix/update action by requester.

---

## B34-router-026-004 — "review with" + code action routing

A: "Review the API contract with the team and update the deprecated endpoints."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "review with" = gap warning; "update the deprecated endpoints" = DOCUMENT action → DOCUMENT fires.

---

## B34-router-026-005 — "present" routing

A: "Present the new microservices design to the architecture review board."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "present to [audience]" = communication/teaching vocabulary → gap. GapHandler invoked + SCHEDULE_CAPABILITY_LEARNING emitted.
