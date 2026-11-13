# Layer 0-B — Router Pairs (032)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-032-001 — "present" + IMPLEMENT routing

A: "Present the feature proposal and implement the notification service."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "present the feature proposal" = gap warning; "implement the notification service" = code action → IMPLEMENT fires.

---

## B34-router-032-002 — "run through" + IMPLEMENT + TEST routing

A: "Run through the requirements, implement the feature, and write the acceptance tests."
Router decision: Predicates = ["IMPLEMENT", "TEST"] → ImplementationFlow41 ‖ UnitTestingFlow51 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "run through requirements" = gap warning; "implement the feature" → IMPLEMENT; "write the acceptance tests" → TEST. Both fire. TransitionType: feature_addition (IMPLEMENT present).

---

## B34-router-032-003 — pure guidance chain → single gap routing

A: "Show, demonstrate, and walk the new team through our release process."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: show + demonstrate + walk through = all guidance vocabulary. Single GapHandler invocation, single SCHEDULE_CAPABILITY_LEARNING. Primary gap ID = gap-skills-guidance-train-001 (majority teach/demonstrate verbs).

---

## B34-router-032-004 — "familiarize" + ARCHITECTURE routing

A: "Familiarize the new hires with the current architecture and design the migration plan."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31 | CapabilityGapWarning: gap-skills-guidance-onboard-001
Evidence: "familiarize new hires with architecture" = gap warning (onboard-001); "design the migration plan" = architecture action → ARCHITECTURE fires.

---

## B34-router-032-005 — "brief" + TEST routing

A: "Brief the team on test coverage requirements and write the missing unit tests."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "brief on test coverage requirements" = gap warning; "write the missing unit tests" = test-writing action → TEST fires.
