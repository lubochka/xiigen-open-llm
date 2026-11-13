# Layer 2 — Capability Gap Extended Pairs (006)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B36-L2-006-001 — release cycle planning: knowledge_edge; gap-project-planning-release-cycle-001

**Input:** "Help me plan the release cycle for the next quarter and set the sprint milestones."

**Stage 1 — Compiler:**
- help: no predicate
- plan: no predicate (Rule 17)
- release: no predicate (standalone; not a QA trigger)
- cycle: no predicate
- quarter: no predicate
- set: no predicate
- sprint: GAP → sprint-001
- milestones: GAP → milestone-001

**Predicate output:** []

Wait — should "release cycle" be checked? Rule 1 lists "release cycle (planning)" as gap vocabulary. The compound "release cycle" is gap vocabulary with ID gap-project-planning-release-cycle-001.

**CapabilityGapWarnings:** sprint-001, milestone-001, release-cycle-001

**Stage 2 — Router:** KNOWLEDGE_EDGE
**Stage 4 — Orchestrator:**
- SCHEDULE_CAPABILITY_LEARNING emitted with: sprint-001, milestone-001, release-cycle-001
- DomainId: "project-management"
- FinalHumanResult: null

---

## B36-L2-006-002 — "release" alone vs "release cycle" as gap vocabulary

"Release" alone is not a predicate trigger and is not registered as gap vocabulary on its own. The compound "release cycle" in a planning context is the gap vocabulary unit (release-cycle-001).

However, "release" appearing in context where the user clearly means a deployment event (not a planning cycle) is neutral — it simply has no predicate match.

| Usage | Gap ID | Notes |
|-------|--------|-------|
| "plan the release cycle" | release-cycle-001 | Planning context |
| "approve the release" | QA via "approve" | QA fires, "release" neutral |
| "release the feature" | no gap, no predicate | "release" neutral |

---

## B36-L2-006-003 — release cycle planning gap explanation

gap-project-planning-release-cycle-001 represents the capability to:
- Plan release cycles (quarterly, monthly, etc.)
- Map sprints to release cycles
- Set release milestones and exit criteria
- Track release readiness across multiple sprints

This requires understanding of both project management timelines and software delivery pipelines — a capability not currently registered.

---

## B36-L2-006-004 — release cycle + sprint compound request

**Input:** "Plan the Q3 release cycle with four sprints and quarterly milestones."

**Gap IDs emitted:**
- gap-project-planning-release-cycle-001
- gap-project-planning-sprint-001
- gap-project-planning-milestone-001

**KNOWLEDGE_EDGE:** true
**SCHEDULE_CAPABILITY_LEARNING:** emitted once with all three IDs

This is a common enterprise project planning request. The SCHEDULE_CAPABILITY_LEARNING signal from this class of request helps AutonomousLearningPipeline prioritize release cycle planning capability development.
