# Layer 0-B — Router Pairs (031)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-031-001 — "review with" vs "code review" disambiguation

A: "Conduct a code review with the team to discuss best practices."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "code review with team to discuss" = guided discussion → gap. Learning/teaching intent ("discuss best practices") confirms gap.

---

## B34-router-031-002 — "review and fix" routing

A: "Code review the authentication service and fix the identified security issues."
Router decision: Predicates = ["SECURITY"] → SecurityAuditFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "code review" = gap warning (review-with sense); "fix the identified security issues" = security remediation → SECURITY fires.

---

## B34-router-031-003 — "demonstrate" + both UI and test routing

A: "Demonstrate the testing patterns and write the snapshot tests for all UI (User Interface) components."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "demonstrate the testing patterns" = gap warning; "write the snapshot tests" = test-writing action → TEST fires.

---

## B34-router-031-004 — "brief" + "implement" routing

A: "Brief the squad on the new feature requirements and implement the booking service."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "brief the squad on" = gap warning; "implement the booking service" = code action → IMPLEMENT fires.

---

## B34-router-031-005 — "introduce" (new hire) + document routing

A: "Introduce the new engineers and document the onboarding setup process."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-onboard-001
Evidence: "introduce the new engineers" = gap warning (onboard-001); "document the onboarding setup process" = documentation action → DOCUMENT fires.
