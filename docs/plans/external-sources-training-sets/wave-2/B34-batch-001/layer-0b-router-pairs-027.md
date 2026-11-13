# Layer 0-B — Router Pairs (027)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-027-001 — "discuss" routing

A: "Discuss the trade-offs of the proposed caching strategy with the team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "discuss with [team]" = knowledge-sharing dialogue → gap. No code action in request.

---

## B34-router-027-002 — "discuss" + IMPLEMENT routing

A: "Discuss the caching strategy and then implement the Redis (in-memory data store) integration."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "discuss" = gap warning; "implement the Redis integration" = code action → IMPLEMENT fires. GapHandler suppressed.

---

## B34-router-027-003 — guidance + UI embedded routing

A: "Walk the frontend team through the design system and implement the accessible form components."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "walk through" = gap warning; "implement the accessible form components" = code action → IMPLEMENT fires.

---

## B34-router-027-004 — guidance + ARCHITECTURE embedded routing

A: "Present the current state and design the new event-driven architecture."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "present the current state" = gap warning; "design the new event-driven architecture" = architecture action → ARCHITECTURE fires.

---

## B34-router-027-005 — Round 2 routing summary: guidance verb → gap ID assignment

| Guidance verb | Gap ID assigned |
|--------------|----------------|
| show how to | gap-skills-guidance-train-001 |
| demonstrate | gap-skills-guidance-train-001 |
| introduce [person] | gap-skills-guidance-train-001 or onboard-001 |
| brief [team] on | gap-skills-guidance-mentor-001 |
| familiarize with | gap-skills-guidance-onboard-001 |
| run through | gap-skills-guidance-mentor-001 |
| review with | gap-skills-guidance-mentor-001 |
| present to | gap-skills-guidance-train-001 |
| discuss | gap-skills-guidance-mentor-001 |
