# Layer 0-B — Router Pairs (035)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-035-001 — "show how to" + full code chain routing

A: "Show the team how to build the feature, but also implement it for the deadline."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "show how to build" = gap warning (teaching frame); "implement it for the deadline" = requester's direct code action → IMPLEMENT fires. GapHandler suppressed.

---

## B34-router-035-002 — "demonstrate" + "integrate" routing

A: "Demonstrate the integration approach and integrate the reporting service."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "demonstrate the integration approach" = gap warning; "integrate the reporting service" → API_CONTRACT fires.

---

## B34-router-035-003 — "introduce" (new hire) + full stack routing

A: "Introduce the new developer, implement the assigned feature, and write its unit tests."
Router decision: Predicates = ["IMPLEMENT", "TEST"] → ImplementationFlow41 ‖ UnitTestingFlow51 | CapabilityGapWarning: gap-skills-guidance-onboard-001
Evidence: "introduce the new developer" = gap warning (onboard-001); "implement" → IMPLEMENT; "write unit tests" → TEST. TransitionType: feature_addition.

---

## B34-router-035-004 — multiple guidance + API_CONTRACT routing

A: "Show, walk through, and integrate the third-party payment gateway."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: show + walk through = guidance warnings; "integrate the payment gateway" → API_CONTRACT fires (integrate always fires). Single CapabilityGapWarning.

---

## B34-router-035-005 — "present" + "design" + ARCHITECTURE routing

A: "Present the existing system limitations and design the refactored architecture."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "present the existing limitations" = gap warning; "design the refactored architecture" = architecture action → ARCHITECTURE fires. TransitionType: documentation.
