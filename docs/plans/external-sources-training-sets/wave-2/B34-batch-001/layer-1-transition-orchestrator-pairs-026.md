# Layer 1 — Transition Orchestrator Pairs (026)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-026-001 — "familiarize" orchestration: GapHandler only

A: "Familiarize the QA (Quality Assurance) engineers with our test automation framework."
Orchestration: GapHandler invoked → gap-skills-guidance-onboard-001 + SCHEDULE_CAPABILITY_LEARNING
No flow execution. TransitionType: none.

---

## TO-026-002 — "run through" orchestration: GapHandler only

A: "Run through the code review standards with the junior developers."
Orchestration: GapHandler invoked → gap-skills-guidance-mentor-001 + SCHEDULE_CAPABILITY_LEARNING
No flow execution. TransitionType: none.

---

## TO-026-003 — "review with" + TEST orchestration

A: "Review the E2E (End-to-End) test strategy with the QA (Quality Assurance) team and write the browser test suite."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → E2ETestingFlow53
TransitionType: documentation. SCHEDULE_CAPABILITY_LEARNING: NOT emitted.

---

## TO-026-004 — "present" + ARCHITECTURE orchestration

A: "Present the target architecture to leadership and architect the microservices decomposition."
Orchestration: CapabilityGapWarning: gap-skills-guidance-train-001 → ArchitectureFlow31
TransitionType: documentation. SCHEDULE_CAPABILITY_LEARNING: NOT emitted.

---

## TO-026-005 — "discuss" + API_CONTRACT orchestration

A: "Discuss the webhook design with the partner team and integrate the inbound webhook handler."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → APIContractFlow33
TransitionType: documentation. SCHEDULE_CAPABILITY_LEARNING: NOT emitted.
