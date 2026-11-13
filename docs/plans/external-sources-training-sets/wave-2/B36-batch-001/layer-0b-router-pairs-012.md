# Layer 0-B — Router Pairs (012)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: router

---

## B36-0B-012-001 — Complete B36 routing decision table

**Gap vocabulary does NOT change routing. This table is exhaustive.**

| Predicate list | Route | Phase(s) | TransitionType | DomainId | SCHEDULE_CAPABILITY_LEARNING |
|----------------|-------|----------|---------------|----------|------------------------------|
| [] | KNOWLEDGE_EDGE | null | unknown | project-management | YES |
| ["QA"] | QA sub-phase | QA | no_contribution | software-development | NO |
| ["IMPLEMENT"] | Phase 4.1 | 4.1 | feature_addition | software-development | NO |
| ["DOCUMENT"] | Phase 2.1 | 2.1 | documentation | software-development | NO |
| ["ARCHITECTURE"] | Phase 3.1 | 3.1 | documentation | software-development | NO |
| ["IMPLEMENT", "DOCUMENT"] | Phase 4.1 + 2.1 | 4.1, 2.1 | feature_addition | software-development | NO |
| ["IMPLEMENT", "QA"] | Phase 4.1 → QA | 4.1, QA | feature_addition | software-development | NO |
| ["DOCUMENT", "QA"] | Phase 2.1 → QA | 2.1, QA | documentation | software-development | NO |
| ["ARCHITECTURE", "DOCUMENT"] | Phase 3.1 + 2.1 | 3.1, 2.1 | documentation | software-development | NO |
| ["IMPLEMENT", "DOCUMENT", "QA"] | 4.1 + 2.1 → QA | 4.1, 2.1, QA | feature_addition | software-development | NO |

---

## B36-0B-012-002 — Key routing invariants for B36

1. `predicates == []` → always KNOWLEDGE_EDGE; always DomainId "project-management"
2. Any predicate → always DomainId "software-development"; never KNOWLEDGE_EDGE
3. QA always executes last when combined with IMPLEMENT and/or DOCUMENT
4. SCHEDULE_CAPABILITY_LEARNING only when `predicates == []`
5. CapabilityGapWarnings present for any gap vocabulary, in any predicate combination
6. Gap vocabulary never appears as a routing selector

---

## B36-0B-012-003 — B36 gap vocabulary effect on RouterResult

**CapabilityGapWarnings are attached to RouterResult whenever gap vocabulary appears:**

```json
{
  "capabilityGapWarnings": [
    { "gapId": "gap-project-planning-sprint-001", "term": "sprint", "status": "KNOWLEDGE_EDGE" },
    { "gapId": "gap-project-planning-backlog-001", "term": "backlog", "status": "KNOWLEDGE_EDGE" }
  ]
}
```

This field exists on RouterResult regardless of which route was taken. For KNOWLEDGE_EDGE, it is the only meaningful output. For predicate-fired routes, it is supplementary information for the AutonomousLearningPipeline (subproject 16).

---

## B36-0B-012-004 — Complete B36 router boundary: what triggers each path

| Token present | Path triggered | Notes |
|--------------|----------------|-------|
| review / approve / test (test) | QA sub-phase | Even in planning sentences |
| implement / build / create / write / feature | Phase 4.1 | Even in sprint/epic sentences |
| document / define / specify / requirements / stakeholder / criteria | Phase 2.1 | Even in planning sentences |
| design / architect / structure / architecture | Phase 3.1 | Even in sprint/milestone sentences |
| sprint / roadmap / backlog / velocity / standup / retrospective / burndown / capacity (team) / iteration / epic / ticket / story-point / estimation / release-cycle | CapabilityGapWarning only | Never trigger a route |
