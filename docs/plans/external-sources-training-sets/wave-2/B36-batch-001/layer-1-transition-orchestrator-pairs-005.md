# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B36-L1-005-001 — ARCHITECTURE in sprint sentence: documentation TransitionType; DomainId: software-development

**Input:** `predicates: ["ARCHITECTURE"]`
**Request origin:** "Design the system architecture for the sprint deliverables and plan the milestone dependencies."

**Orchestrator state:**

```json
{
  "transitionType": "documentation",
  "contextCarryForward": false,
  "phaseId": "3.1",
  "flowSelected": "ArchitectureFlow31",
  "finalHumanResult": "<architectural documentation>",
  "domainId": "software-development",
  "knowledgeEdge": false,
  "capabilityGapWarnings": [
    "gap-project-planning-sprint-001",
    "gap-project-planning-milestone-001"
  ],
  "scheduledSignals": []
}
```

---

## B36-L1-005-002 — ARCHITECTURE TransitionType is documentation (not feature_addition)

ARCHITECTURE produces architectural documentation artifacts, not deployed code features. Therefore TransitionType is "documentation" — consistent with DOCUMENT.

**TransitionType mapping:**
- IMPLEMENT → feature_addition (deploys code)
- DOCUMENT → documentation (produces docs)
- ARCHITECTURE → documentation (produces architectural docs)
- QA → no_contribution (validates, does not produce)

---

## B36-L1-005-003 — ARCHITECTURE in planning sentence: no special behavior

When ARCHITECTURE fires in a sprint/milestone sentence, the orchestrator produces an architectural documentation result. The sprint planning context does not cause the orchestrator to produce a sprint plan or roadmap. The output is always architectural documentation.

The planning context (sprint deliverables, milestone dependencies) may appear in the architectural documentation as context, but the orchestration produces documentation — not a sprint plan.

---

## B36-L1-005-004 — ARCHITECTURE orchestration result structure

```json
{
  "orchestrationResult": {
    "transitionType": "documentation",
    "contextCarryForward": false,
    "phaseId": "3.1",
    "domainId": "software-development",
    "finalHumanResult": "<populated by ArchitectureFlow31>",
    "knowledgeEdge": false,
    "capabilityGapWarnings": [
      "gap-project-planning-sprint-001",
      "gap-project-planning-milestone-001"
    ],
    "scheduledSignals": []
  }
}
```
