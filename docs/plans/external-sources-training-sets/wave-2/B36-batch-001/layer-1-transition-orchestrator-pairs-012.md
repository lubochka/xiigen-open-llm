# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B36-L1-012-001 — v6 ordering violations for B36

**v6-01 ordering violation:** Orchestrator emits PROJECT_PLANNING flow

Orchestrator receives factory result with invented "ProjectPlanningFlow." Attempts to execute it. No such flow exists in the execution engine. Crash.

**v6-02 ordering violation:** QA was suppressed at compiler; orchestrator receives [] instead of ["QA"]

Orchestrator enters KNOWLEDGE_EDGE for "review the sprint velocity" — wrong. The request should have been routed to QA. The suppression at the compiler stage causes the orchestrator to emit SCHEDULE_CAPABILITY_LEARNING unnecessarily and set DomainId to "project-management" (wrong).

**v6-03 ordering violation:** Orchestrator sets DomainId "project-management" despite IMPLEMENT predicate

Orchestrator receives correct predicates: ["IMPLEMENT"]. But then applies DomainId "project-management" because it detects heavy planning vocabulary. This is the v6-03 corruption — the orchestrator overrides the DomainId rule based on vocabulary proportion.

---

## B36-L1-012-002 — v6 violations are detectable

Each v6 violation produces a specific observable error:

| v6 Code | Observable symptom | Detection method |
|---------|-------------------|-----------------|
| v6-01 | PROJECT_PLANNING in predicate list | Assert predicate not in non-existent list |
| v6-02 | [] for a request containing "review" | Assert ["QA"] for any request with "review" |
| v6-03 | DomainId: "project-management" with non-empty predicates | Assert domainId == "software-development" when predicates != [] |

---

## B36-L1-012-003 — Orchestration anti-patterns for B36

1. Emitting SCHEDULE_CAPABILITY_LEARNING when predicates fired (should only emit for predicates: [])
2. Setting FinalHumanResult: null when predicates fired (null only for KNOWLEDGE_EDGE)
3. Setting TransitionType: unknown when predicates fired (unknown only for predicates: [])
4. Multiple SCHEDULE_CAPABILITY_LEARNING signals for one request (always one per request)
5. Not emitting CapabilityGapWarnings when gap vocabulary present (always emit gap warnings)

---

## B36-L1-012-004 — Orchestration correctness checklist for B36

For any B36 request, the orchestration output is correct if and only if:

1. If predicates: [] → knowledgeEdge: true, transitionType: unknown, finalHumanResult: null, domainId: "project-management", scheduledSignals: ["SCHEDULE_CAPABILITY_LEARNING"]
2. If predicates: [non-empty] → knowledgeEdge: false, transitionType: (per table), finalHumanResult: populated, domainId: "software-development", scheduledSignals: []
3. CapabilityGapWarnings present for every B36 gap term found in the request
4. scheduledSignals array length == 1 for predicates: [], length == 0 for predicates: [non-empty]
5. QA phase is executionOrder: 2 (last) when co-occurring with IMPLEMENT and/or DOCUMENT
