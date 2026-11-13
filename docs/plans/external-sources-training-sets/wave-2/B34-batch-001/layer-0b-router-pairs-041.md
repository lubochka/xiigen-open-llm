# Layer 0-B — Router Pairs (041)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-041-001 — "rubber duck" routing = GapHandler

A: "Rubber duck the thread-safety design with a senior engineer."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "rubber duck [problem] with [person]" = advisory vocabulary → GapHandler invoked.

---

## B34-router-041-002 — "sounding board" routing = GapHandler

A: "Use the architecture review board as a sounding board for the microservices boundaries."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "sounding board for [design]" = advisory vocabulary → GapHandler. Architecture topic in advisory frame does not route to ArchitectureFlow31.

---

## B34-router-041-003 — "rubber duck" + IMPLEMENT routing

A: "Rubber duck the concurrency approach and implement the thread-safe queue."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "rubber duck approach" = gap warning; "implement thread-safe queue" = requester's IMPLEMENT action → IMPLEMENT routes to ImplementationFlow41.

---

## B34-router-041-004 — "sounding board" + ARCHITECTURE routing

A: "Use the team as a sounding board and then architect the event-driven topology."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "sounding board" = gap warning; "architect the event-driven topology" = requester's ARCHITECTURE action → ARCHITECTURE routes to ArchitectureFlow31.

---

## B34-router-041-005 — "consult on" + SECURITY routing

A: "Consult on the threat landscape and then harden the network edge layer."
Router decision: Predicates = ["SECURITY"] → SecurityAuditFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "consult on threat landscape" = gap warning; "harden network edge layer" = requester's SECURITY action → SECURITY routes to SecurityAuditFlow.
