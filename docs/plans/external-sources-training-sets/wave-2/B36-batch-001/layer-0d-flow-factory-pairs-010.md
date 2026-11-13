# Layer 0-D — Flow Factory Pairs (010)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B36-0D-010-001 — probationary_rejected: planning-specific flow candidate fails Gate B

**Scenario:** A proposed "SprintPlanningFlow" was evaluated as a candidate for handling B36 gap vocabulary requests.

**Proposed flow:** SprintPlanningFlow
**Gate B condition:** Must handle sprint/backlog/velocity/retrospective/burndown vocabulary with meaningful FinalHumanResult in at least 80% of test cases.
**Gate B result:** FAILED — the flow produced vacuous responses for all test cases (essentially restating the gap without adding planning intelligence).

**Disposition:** probationary_rejected — the candidate flow was rejected and is NOT registered.

---

## B36-0D-010-002 — why no planning flow is registered

The AdaptiveLogicalOrchestrator (ALO) operates on nine registered predicates. Project planning vocabulary has no predicate. A planning flow would require:
1. A registered predicate (e.g., "PLAN") to trigger it
2. A lexicon entry for planning tokens
3. Gate B validation showing the flow adds value

None of these exist. SprintPlanningFlow was rejected because it failed Gate B. SCHEDULE_CAPABILITY_LEARNING signals the AutonomousLearningPipeline to potentially develop this capability in a future iteration.

---

## B36-0D-010-003 — rejected flow does not appear in factory mapping

```
FlowFactory mapping table (current state):
  IMPLEMENT → BackendServiceFlow41
  DOCUMENT  → DocumentationFlow
  ARCHITECTURE → ArchitectureFlow31
  QA → QAGatingFlow
  (SprintPlanningFlow: REJECTED — not in table)
```

A rejected flow candidate does not appear in the factory mapping table. It does not exist as a named flow that could be accidentally selected. The rejection is permanent until a passing Gate B evaluation triggers re-registration.

---

## B36-0D-010-004 — what probationary_rejected means for CapabilityGapWarnings

Gap IDs (sprint, backlog, etc.) are still emitted as CapabilityGapWarnings. The rejection of SprintPlanningFlow does not suppress gap warning emission. The AutonomousLearningPipeline still receives SCHEDULE_CAPABILITY_LEARNING and CapabilityGapWarnings. The gap signals continue accumulating even though no flow exists to handle them yet.

**This is intentional:** gap signals accumulate to build the case for future capability development. The more frequently gap-project-planning-sprint-001 appears in signals, the stronger the case for developing a planning capability.
