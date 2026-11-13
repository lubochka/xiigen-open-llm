# Layer 2 — Capability Gap Extended Pairs (003)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B36-L2-003-001 — team capacity planning: pure gap

**Input:** "Assess team capacity for the sprint and allocate resources to backlog items."

**Full knowledge_edge trace:**

**Stage 1 — Compiler:**
- assess: no predicate
- team: no predicate
- capacity: team context → GAP (capacity-001; NOT ARCHITECTURE per Rule 16)
- sprint: GAP → sprint-001
- allocate: no predicate
- resources: no predicate
- backlog: GAP → backlog-001
- items: no predicate

**Predicate output:** []
**CapabilityGapWarnings:** capacity-001, sprint-001, backlog-001

**Stage 2 — Router:** KNOWLEDGE_EDGE
**Stage 3 — Factory:** not called
**Stage 4 — Orchestrator:** SCHEDULE_CAPABILITY_LEARNING(capacity-001, sprint-001, backlog-001)

---

## B36-L2-003-002 — "design the capacity plan" fires ARCHITECTURE: mixed-signal trace

**Input:** "Design the capacity plan for the sprint team and plan the upcoming iteration."

**Stage 1 — Compiler:**
- design: ARCHITECTURE fires (Rule 4)
- capacity: team context → would be GAP, but ARCHITECTURE already fired via "design"
- plan: no predicate
- sprint: GAP → sprint-001
- team: no predicate
- iteration: GAP → iteration-001

**Predicate output:** ["ARCHITECTURE"]
**CapabilityGapWarnings:** capacity-001, sprint-001, iteration-001

**Stage 2 — Router:** Phase 3.1 (ARCHITECTURE)
**KNOWLEDGE_EDGE:** false
**DomainId:** "software-development"
**Stage 4 — Orchestrator:** SCHEDULE_CAPABILITY_LEARNING NOT emitted (predicate fired)

---

## B36-L2-003-003 — capacity boundary cases

| Request | Predicates | Notes |
|---------|-----------|-------|
| "Plan team capacity for sprint" | [] | Pure gap: capacity-001, sprint-001 |
| "Design capacity model" | [ARCHITECTURE] | "design" fires ARCHITECTURE |
| "Estimate capacity allocation" | [] | estimate is gap; capacity is gap |
| "Document the capacity requirements" | [DOCUMENT] | document + requirements fire DOCUMENT |

---

## B36-L2-003-004 — "capacity planning" as compound gap

When both "capacity" and "planning" appear together in team context, the compound gap ID gap-project-planning-capacity-001 covers the team capacity planning concept. The individual gap ID is used (capacity-001) since "planning" is not itself gap vocabulary but rather a neutral qualifier.

The capacity gap represents: the orchestrator cannot help the user plan team capacity, allocate resources across a sprint, or calculate how many story points a team can handle in an iteration. These are all capability gaps covered by capacity-001.
