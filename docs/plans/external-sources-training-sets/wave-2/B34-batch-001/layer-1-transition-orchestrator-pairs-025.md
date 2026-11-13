# Layer 1 — Transition Orchestrator Pairs (025)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-025-001 — "show how to" orchestration: GapHandler only

A: "Show the new engineers how to set up their local development environment."
Orchestration: GapHandler invoked → gap-skills-guidance-train-001 + SCHEDULE_CAPABILITY_LEARNING
No flow execution. TransitionType: none. Gap warning: not applicable (full gap, not warning).

---

## TO-025-002 — "demonstrate" orchestration: GapHandler only

A: "Demonstrate the canary deployment process to the ops team."
Orchestration: GapHandler invoked → gap-skills-guidance-train-001 + SCHEDULE_CAPABILITY_LEARNING
No flow execution. TransitionType: none.

---

## TO-025-003 — "demonstrate" + IMPLEMENT orchestration

A: "Demonstrate the service template to the team and implement the notification microservice."
Orchestration: CapabilityGapWarning: gap-skills-guidance-train-001 → ImplementationFlow41
TransitionType: feature_addition. SCHEDULE_CAPABILITY_LEARNING: NOT emitted.
Execution: ImplementationFlow41 executes after gap warning recorded.

---

## TO-025-004 — "introduce [person]" orchestration: GapHandler only

A: "Introduce the new engineers to the engineering excellence programme."
Orchestration: GapHandler invoked → gap-skills-guidance-onboard-001 + SCHEDULE_CAPABILITY_LEARNING
No flow execution. TransitionType: none.

---

## TO-025-005 — "brief" + SECURITY orchestration

A: "Brief the team on CSRF (Cross-Site Request Forgery) attack vectors and implement CSRF protection in the web forms."
Orchestration: CapabilityGapWarning: gap-skills-guidance-mentor-001 → SecurityAuditFlow ‖ ImplementationFlow41
TransitionType: feature_addition. SCHEDULE_CAPABILITY_LEARNING: NOT emitted.
