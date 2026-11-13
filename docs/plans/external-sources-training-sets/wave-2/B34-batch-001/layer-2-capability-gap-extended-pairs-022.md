# Layer 2 — Capability Gap Extended Pairs (022)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-022 — Multiple guidance verbs + embedded code verbs

A: "Coach through building the microservice, walk through integrating it with the message broker, and guide through writing contract tests."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: Three guidance frames with three embedded code actions. IMPLEMENT (build), API_CONTRACT (integrate), TEST (write tests). All three guidance verbs become gap warnings. GapHandler suppressed.

A: "Mentor, advise, and coach the senior engineers on technical leadership practices."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: Three guidance verbs, no embedded code actions. Predicate set = []. GapHandler invoked. SCHEDULE_CAPABILITY_LEARNING emitted once.

A: "Advise on trade-offs, architect the data pipeline system, and build the ingestion layer."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
Analysis: "advise" = gap warning; "architect the data pipeline system" → ARCHITECTURE; "build the ingestion layer" → IMPLEMENT. Orchestration: ArchitectureFlow31 → ImplementationFlow41.

A: "Explain the system, shadow the team during deployment, and write the incident runbook."
B: Predicates: ["DOCUMENT"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "explain" = gap warning (train-001); "shadow during deployment" = gap warning; "write the incident runbook" = direct action → DOCUMENT fires (write + documentation artifact).

A: "To onboard new engineers, implement the starter kit and document the contribution workflow."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | GapWarning: gap-skills-guidance-onboard-001 | DomainId: software-development
Analysis: "to onboard" = purpose clause → gap warning (onboard-001); "implement the starter kit" → IMPLEMENT; "document the contribution workflow" → DOCUMENT. Both code predicates fire.
