# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B36-L1-006-001 — QA fires on "review" in planning context: no_contribution TransitionType; DomainId: software-development

**Input:** `predicates: ["QA"]`
**Request origin:** "Review the team velocity from the last sprint and plan the retrospective agenda."

**Orchestrator state:**

```json
{
  "transitionType": "no_contribution",
  "contextCarryForward": false,
  "phaseId": "QA",
  "flowSelected": "QAGatingFlow",
  "finalHumanResult": "<QA validation result>",
  "domainId": "software-development",
  "knowledgeEdge": false,
  "capabilityGapWarnings": [
    "gap-project-planning-velocity-001",
    "gap-project-planning-sprint-001",
    "gap-project-planning-retrospective-001"
  ],
  "scheduledSignals": []
}
```

---

## B36-L1-006-002 — QA TransitionType no_contribution in planning context

QA orchestration always produces TransitionType "no_contribution" because QA validates but does not create. In a planning context, "review the velocity" is a validation activity. The orchestrator correctly maps this to no_contribution regardless of whether the sentence is about sprint velocity or software performance.

---

## B36-L1-006-003 — QA orchestration: FinalHumanResult is not null

Unlike KNOWLEDGE_EDGE (where FinalHumanResult is null), QA orchestration produces a FinalHumanResult — the QA validation outcome. The result is populated by QAGatingFlow.

**KNOWLEDGE_EDGE vs QA:**

| State | FinalHumanResult | TransitionType |
|-------|-----------------|---------------|
| KNOWLEDGE_EDGE (predicates: []) | null | unknown |
| QA (predicates: ["QA"]) | populated | no_contribution |
| IMPLEMENT (predicates: ["IMPLEMENT"]) | populated | feature_addition |

---

## B36-L1-006-004 — QA orchestration result structure

```json
{
  "orchestrationResult": {
    "transitionType": "no_contribution",
    "contextCarryForward": false,
    "phaseId": "QA",
    "domainId": "software-development",
    "finalHumanResult": "<populated by QAGatingFlow>",
    "knowledgeEdge": false,
    "capabilityGapWarnings": [
      "gap-project-planning-velocity-001",
      "gap-project-planning-sprint-001",
      "gap-project-planning-retrospective-001"
    ],
    "scheduledSignals": []
  }
}
```
