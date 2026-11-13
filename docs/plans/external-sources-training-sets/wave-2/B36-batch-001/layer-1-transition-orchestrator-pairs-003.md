# Layer 1 — Transition Orchestrator Pairs (003)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B36-L1-003-001 — DOCUMENT in planning context: documentation TransitionType; DomainId: software-development

**Input:** `predicates: ["DOCUMENT"]`
**Request origin:** "Document the milestone deliverables and define the acceptance criteria for each sprint release."

**Orchestrator state:**

```json
{
  "transitionType": "documentation",
  "contextCarryForward": false,
  "phaseId": "2.1",
  "flowSelected": "DocumentationFlow",
  "finalHumanResult": "<documentation artifact>",
  "domainId": "software-development",
  "knowledgeEdge": false,
  "capabilityGapWarnings": [
    "gap-project-planning-milestone-001",
    "gap-project-planning-sprint-001",
    "gap-project-planning-release-cycle-001"
  ],
  "scheduledSignals": []
}
```

---

## B36-L1-003-002 — TransitionType "documentation" in planning sentences

TransitionType "documentation" means the orchestration produced a documentation artifact. This holds even when the documentation subject is a project planning artifact (milestone, sprint release).

The TransitionType reflects the output type (documentation artifact), not the subject matter. A document about milestones is still a documentation output. The planning context is captured in CapabilityGapWarnings but does not change the TransitionType.

---

## B36-L1-003-003 — DomainId: software-development when DOCUMENT fires in planning sentence

DomainId: "software-development" is set because DOCUMENT fired. The fact that the request is about milestone deliverables (a project planning concept) does not change the DomainId.

This may seem counterintuitive: a request about milestones gets DomainId "software-development." But the DomainId rule is: predicate fired → "software-development." The predicate (DOCUMENT) fired. Therefore DomainId is "software-development." The planning content is noted in gap warnings.

---

## B36-L1-003-004 — DOCUMENT orchestration result structure

```json
{
  "orchestrationResult": {
    "transitionType": "documentation",
    "contextCarryForward": false,
    "phaseId": "2.1",
    "domainId": "software-development",
    "finalHumanResult": "<populated by DocumentationFlow>",
    "knowledgeEdge": false,
    "capabilityGapWarnings": ["gap-project-planning-milestone-001", "gap-project-planning-sprint-001"],
    "scheduledSignals": []
  }
}
```
