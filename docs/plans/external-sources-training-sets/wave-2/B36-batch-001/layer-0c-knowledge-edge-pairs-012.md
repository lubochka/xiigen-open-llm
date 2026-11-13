# Layer 0-C — Knowledge-Edge Pairs (012)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B36-0C-012-001 — B36 knowledge-edge summary: all rules in one reference table

**Complete B36 rule reference for knowledge-edge behavior:**

| Rule | Rule number | Statement |
|------|-------------|-----------|
| Gap vocabulary | 1 | sprint, roadmap, milestone, backlog, velocity, standup, retrospective, burndown, capacity (team), iteration, epic, ticket, story-point, estimation have no registered predicate |
| "story" fires DOCUMENT | 2 | "story" in any form fires DOCUMENT — false positive |
| "review" fires QA | 3 | "review" fires QA even in planning context; "retrospective" does NOT |
| ARCHITECTURE triggers | 4 | "design"/"architect"/"structure" fire ARCHITECTURE even in planning sentences |
| DOCUMENT triggers | 5 | "define"/"document"/"specify"/"requirements"/"stakeholder" fire DOCUMENT |
| IMPLEMENT triggers | 6 | "implement"/"build"/"create"/"write"/"feature" fire IMPLEMENT even in planning |
| DomainId rule | 7 | "project-management" only when predicates: []; "software-development" when any predicate fires |
| TransitionType rule | 8 | unknown/PhaseId null when predicates: [] |
| SCHEDULE_CAPABILITY_LEARNING | 9 | Emitted for every zero-predicate gap request |
| AutonomousLearningPipeline | 10 | Subproject 16 consumes SCHEDULE_CAPABILITY_LEARNING |
| Predicate dominance | 11 | When predicates fire alongside gap vocab → CapabilityGapWarnings only; normal routing |
| "update" rule | 12 | "update" in planning context → NOT IMPLEMENT; needs code-level co-occurrence |
| "generate" rule | 13 | "generate" not in any predicate lexicon |
| "capture" rule | 14 | "capture" not in DOCUMENT lexicon |
| "estimate" rule | 15 | "estimate"/"estimation" are gap vocabulary |
| "capacity" team rule | 16 | "capacity" in team planning context → NOT ARCHITECTURE |
| "plan" rule | 17 | "plan" alone not in any predicate lexicon |
| "run" rule | 18 | "run" in ceremony context not in any predicate lexicon |
| Gap ID format | 19 | gap-project-planning-{term(s)}-{sequence} |
| KNOWLEDGE_EDGE state | 20 | No nine-node flow; FinalHumanResult: null; recommended_action: SCHEDULE_CAPABILITY_LEARNING |
| "acceptance criteria" | 21 | "criteria" fires DOCUMENT |
| One signal per request | 22 | One SCHEDULE_CAPABILITY_LEARNING per request (all gap IDs bundled) |
| v6 corruptions | 23 | v6-01: PROJECT_PLANNING invented; v6-02: predicate suppressed; v6-03: DomainId wrong |

---

## B36-0C-012-002 — Knowledge-edge output structure (canonical)

```json
{
  "predicates": [],
  "knowledgeEdge": true,
  "finalHumanResult": null,
  "transitionType": "unknown",
  "phaseId": null,
  "domainId": "project-management",
  "capabilityGapWarnings": [
    { "gapId": "gap-project-planning-sprint-001", "term": "sprint" },
    { "gapId": "gap-project-planning-backlog-001", "term": "backlog" }
  ],
  "scheduledSignals": [
    {
      "signal": "SCHEDULE_CAPABILITY_LEARNING",
      "capabilityGapIds": ["gap-project-planning-sprint-001", "gap-project-planning-backlog-001"]
    }
  ],
  "recommendedAction": "SCHEDULE_CAPABILITY_LEARNING"
}
```

---

## B36-0C-012-003 — Normal routing output structure (canonical, mixed-signal)

```json
{
  "predicates": ["IMPLEMENT"],
  "knowledgeEdge": false,
  "finalHumanResult": "<from Phase 4.1>",
  "transitionType": "feature_addition",
  "phaseId": "4.1",
  "domainId": "software-development",
  "capabilityGapWarnings": [
    { "gapId": "gap-project-planning-sprint-001", "term": "sprint" }
  ],
  "scheduledSignals": [],
  "recommendedAction": "ROUTE_TO_PHASE_4_1"
}
```

**Key differences from knowledge-edge:** predicates non-empty; knowledgeEdge: false; finalHumanResult populated; phaseId set; domainId: "software-development"; scheduledSignals empty (no SCHEDULE_CAPABILITY_LEARNING).
