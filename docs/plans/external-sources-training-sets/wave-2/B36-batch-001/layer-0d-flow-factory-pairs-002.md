# Layer 0-D — Flow Factory Pairs (002)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B36-0D-002-001 — knowledge_edge: no factory selected (pure gap)

**Input predicate list:** `[]`
**Request origin:** "Groom the product backlog and prioritize the tickets for the next iteration."

**Factory selection process:**

```
predicates == [] ?
  YES →
    KNOWLEDGE_EDGE state
    → no factory query issued
    → FlowFactory.Select() not called
    → Flow: null
    → FinalHumanResult: null
```

**Factory output:**
- Flow selected: NONE
- FactoryResult: null
- Reason: No registered flow exists for zero-predicate, gap-vocabulary-only requests

---

## B36-0D-002-002 — why no factory is queried for KNOWLEDGE_EDGE

When predicates: [], the orchestrator enters KNOWLEDGE_EDGE state before querying the factory. The factory is only queried when at least one predicate fires. This is by design: the factory's job is to select a flow based on a predicate. With no predicate, there is no basis for selection.

**Sequence for KNOWLEDGE_EDGE:**
1. Compiler outputs predicates: []
2. Router routes to KNOWLEDGE_EDGE path
3. Orchestrator notes KNOWLEDGE_EDGE state
4. Orchestrator emits SCHEDULE_CAPABILITY_LEARNING
5. Orchestrator returns FinalHumanResult: null
6. FlowFactory is never called

---

## B36-0D-002-003 — factory is not asked to "handle" project planning

**Anti-pattern:**
Factory receives a custom "project-planning" flow request and tries to return a ProjectPlanningFlow. No such flow exists. This is v6-01 propagating to the factory stage.

**Correct behavior:**
If a v6-01 model sends ["PROJECT_PLANNING"] to the factory, the factory should throw CapabilityGapException or return FlowResult.Null — not invent a ProjectPlanningFlow.

In the correct model, ["PROJECT_PLANNING"] never reaches the factory because the compiler never emits that predicate.

---

## B36-0D-002-004 — KNOWLEDGE_EDGE factory null result structure

```json
{
  "factoryResult": null,
  "selectedFlow": null,
  "selectedPhase": null,
  "reason": "KNOWLEDGE_EDGE — no registered flow for gap vocabulary predicates",
  "capabilityGapWarnings": [
    "gap-project-planning-backlog-001",
    "gap-project-planning-iteration-001",
    "gap-project-planning-ticket-001"
  ]
}
```

This structure propagates to the orchestrator, which uses it to confirm KNOWLEDGE_EDGE state and emit SCHEDULE_CAPABILITY_LEARNING.
