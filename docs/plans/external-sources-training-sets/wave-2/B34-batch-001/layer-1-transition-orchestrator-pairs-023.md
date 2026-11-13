# Layer 1 — Transition Orchestrator Pairs (023)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-023-001 — Multiple guidance verbs + multiple code verbs: full orchestration

Request: "Coach through building the auth service, walk through integrating it with the identity provider, and guide through writing integration tests."
Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"]
Orchestration: APIContractFlow33 → ImplementationFlow41 → UnitTestingFlow51
TransitionType: feature_addition | Gap warnings: coach, walk-through, guide = gap-skills-guidance-mentor-001

---

## TO-023-002 — Multiple guidance verbs, no code actions → GapHandler

Request: "Mentor, advise, and coach the new engineering leads on technical leadership and architectural thinking."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Orchestration: GapHandler. Three guidance verbs; no code-action verbs. SCHEDULE_CAPABILITY_LEARNING emitted once.

---

## TO-023-003 — Guidance framing + ARCHITECTURE + DOCUMENT

Request: "Advise on the approach, design the component architecture, and document the design decisions in the engineering wiki."
Predicates: ["ARCHITECTURE", "DOCUMENT"]
Orchestration: ArchitectureFlow31 → DocumentationFlow (parallel with or following Architecture)
TransitionType: documentation | Gap warning: advise noted

---

## TO-023-004 — Guidance instruction + requester code vs team code (disambiguation)

Request: "Upskill the team — architect the new domain boundaries yourself and have them implement the services."
Analysis: "upskill" = gap; "architect the new domain boundaries yourself" = requester's action → ARCHITECTURE; "have them implement" = third-party directive → no IMPLEMENT from team action.
Predicates: ["ARCHITECTURE"]
Orchestration: ArchitectureFlow31
TransitionType: documentation | Gap warning: upskill = gap-skills-guidance-train-001

---

## TO-023-005 — Guidance purpose clause + code actions

Request: "To onboard the team, implement the starter project template and document the contribution guide."
Predicates: ["IMPLEMENT", "DOCUMENT"]
Orchestration: ImplementationFlow41 ‖ DocumentationFlow
TransitionType: feature_addition | Gap warning: onboard (purpose clause) = gap-skills-guidance-onboard-001
