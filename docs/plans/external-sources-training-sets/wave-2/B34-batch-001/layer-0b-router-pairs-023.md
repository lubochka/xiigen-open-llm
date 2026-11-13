# Layer 0-B — Router Pairs (023)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-023-001 — Multiple guidance verbs, multiple code verbs

A: "Coach through building the service, walk through integrating it with the message queue, and guide through writing the smoke tests."
Router decision: Predicates = ["IMPLEMENT", "API_CONTRACT", "TEST"] → ImplementationFlow41 → APIContractFlow33 → UnitTestingFlow51
Evidence: Three embedded code actions fire; three guidance frames become gap warnings; GapHandler NOT invoked.

---

## B34-router-023-002 — Multiple guidance verbs, no embedded code actions

A: "Mentor, advise, and coach the team on scaling best practices."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: Three guidance verbs, no embedded code-action verbs. Full GapHandler invocation + SCHEDULE_CAPABILITY_LEARNING.

---

## B34-router-023-003 — Mixed: gap warning + ARCHITECTURE + DOCUMENT

A: "Advise on the approach, design the component architecture, and document the design rationale."
Router decision: Predicates = ["ARCHITECTURE", "DOCUMENT"] → ArchitectureFlow31 + DocumentationFlow
Evidence: "design the component architecture" → ARCHITECTURE; "document the design rationale" → DOCUMENT; "advise" = gap warning.

---

## B34-router-023-004 — Mixed: guidance framing entire request, code verbs embedded

A: "Upskill the team by having them architect the new service, build the API, and write the tests."
Analysis: "upskill the team by having them" = third-party embedding (team's actions); requester's action = upskill = gap.
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "having them architect/build/write" = team's actions, not requester's. Requester's action = upskill = gap.

---

## B34-router-023-005 — Mixed: requester does the code actions, guidance is framing

A: "To onboard the new engineers, implement the sample project and document the step-by-step guide."
Router decision: Predicates = ["IMPLEMENT", "DOCUMENT"] → ImplementationFlow41 + DocumentationFlow
Evidence: "implement the sample project" → IMPLEMENT; "document the step-by-step guide" → DOCUMENT; "to onboard" = purpose clause, not requester's primary action; gap warning for onboarding context.
