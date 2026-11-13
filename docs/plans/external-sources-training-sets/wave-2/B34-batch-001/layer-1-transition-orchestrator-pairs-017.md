# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-017-001 — Gap: "advise on best practices"

Request: "Advise the backend team on best practices for building high-availability systems."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Orchestration: GapHandler. Advisory vocabulary → gap. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-017-002 — ARCHITECTURE: "advise and design"

Request: "Advise on the trade-offs and design the data access layer architecture for the inventory service."
Predicates: ["ARCHITECTURE"]
Orchestration: ArchitectureFlow31
TransitionType: documentation | Gap warning: advise = gap-skills-guidance-mentor-001

---

## TO-017-003 — SECURITY: "advise and harden"

Request: "Advise on the attack surface and harden the authentication endpoints against brute-force attacks."
Predicates: ["SECURITY"]
Orchestration: SecurityAuditFlow
TransitionType: documentation | Gap warning: advise noted

---

## TO-017-004 — IMPLEMENT+DOCUMENT: "advise, implement, document"

Request: "Advise on the approach, implement the caching middleware, and document the caching strategy."
Predicates: ["IMPLEMENT", "DOCUMENT"]
Orchestration: ImplementationFlow41 ‖ DocumentationFlow (parallel after implement)
TransitionType: feature_addition | Gap warning: advise noted

---

## TO-017-005 — Gap: "advise on how to implement" (teach-to pattern)

Request: "Advise the developers on how to implement the repository pattern correctly."
Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Orchestration: GapHandler. "advise on how to implement" = advisory on a pattern → gap. Infinitive learning intent. SCHEDULE_CAPABILITY_LEARNING emitted.
