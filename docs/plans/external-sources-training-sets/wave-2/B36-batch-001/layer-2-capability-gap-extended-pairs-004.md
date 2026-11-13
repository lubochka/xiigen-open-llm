# Layer 2 — Capability Gap Extended Pairs (004)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B36-L2-004-001 — burndown chart: full knowledge_edge trace with gap-ID emission

**Input:** "Show me the burndown chart for the current sprint and explain the velocity trend."

**Stage 1 — Compiler:**
- show: no predicate
- burndown: GAP → burndown-001
- chart: no predicate
- current: no predicate
- sprint: GAP → sprint-001
- explain: no predicate
- velocity: GAP → velocity-001
- trend: no predicate

**Predicate output:** []

**Stage 2 — Router:**
- predicates: [] → KNOWLEDGE_EDGE path
- CapabilityGapWarnings queued: burndown-001, sprint-001, velocity-001

**Stage 3 — Factory:**
- Not called (KNOWLEDGE_EDGE path)

**Stage 4 — Orchestrator:**
- TransitionType: unknown
- PhaseId: null
- DomainId: "project-management"
- FinalHumanResult: null
- SCHEDULE_CAPABILITY_LEARNING emitted:
  ```json
  {
    "signal": "SCHEDULE_CAPABILITY_LEARNING",
    "capabilityGapIds": ["gap-project-planning-burndown-001", "gap-project-planning-sprint-001", "gap-project-planning-velocity-001"]
  }
  ```

---

## B36-L2-004-002 — burndown gap: what the gap represents

gap-project-planning-burndown-001 represents the orchestrator's inability to:
- Generate a burndown chart
- Analyze burndown data
- Predict sprint completion based on burndown trends
- Interpret burndown anomalies (scope creep, mid-sprint additions, etc.)

These are project planning capabilities that require integration with project tracking systems (Jira, Azure DevOps, etc.) — not currently in scope for the orchestrator.

---

## B36-L2-004-003 — "review the burndown" fires QA (contrast with pure burndown gap)

**Input A (pure gap):** "Check the burndown for the sprint."
- Predicate: [] — "check" is not in any lexicon
- KNOWLEDGE_EDGE: true

**Input B (QA fires):** "Review the burndown for the sprint."
- Predicate: ["QA"] — "review" fires QA
- KNOWLEDGE_EDGE: false

The difference is entirely in the verb: "check" vs "review." Same planning context. Same burndown subject. Different predicate outcome.

---

## B36-L2-004-004 — compound gap: burndown + milestone tracker

When burndown and milestone tracking co-occur in a zero-predicate request, the compound gap ID gap-project-planning-burndown-milestone-tracker-001 is emitted in addition to the individual gap IDs.

**Example:** "Track the sprint burndown and milestone completion status."
**Gap IDs emitted:**
- gap-project-planning-burndown-001 (individual)
- gap-project-planning-sprint-001 (individual)
- gap-project-planning-milestone-001 (individual)
- gap-project-planning-burndown-milestone-tracker-001 (compound)

The compound ID signals that the user wants an integrated burndown + milestone tracking capability — a more specific gap than either alone.
