# Layer 0-D — Flow Factory Pairs (006)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B36-0D-006-001 — QA factory: QAGatingFlow or ReviewFlow when "review" fires in sprint context

**Input predicate list:** `["QA"]`
**Request origin:** "Review the sprint burndown report and approve the release readiness."

**Factory selection process:**

```
predicates contains QA? YES
→ FlowFactory.Select(QA)
→ Returns: QAGatingFlow (or ReviewFlow, depending on QA sub-type)
→ Phase: QA sub-phase
→ TransitionType: no_contribution
```

**Factory output:**
- Flow selected: QAGatingFlow
- Phase: QA sub-phase
- TransitionType: no_contribution
- CapabilityGapWarnings: sprint-001, burndown-001, release-cycle-001

---

## B36-0D-006-002 — QAGatingFlow in sprint context: validation, not planning

QAGatingFlow is a validation flow. When triggered by "review" in a sprint burndown context, it runs its standard QA validation steps. It does not understand burndown charts or sprint velocity as planning concepts. It runs its standard QA check sequence.

**What QAGatingFlow does:**
- Runs validation checks
- Returns QA pass/fail status
- TransitionType: no_contribution

**What QAGatingFlow does NOT do:**
- Does not analyze burndown trends
- Does not assess sprint health
- Does not route to planning sub-systems

---

## B36-0D-006-003 — QA factory result structure

```json
{
  "factoryResult": "QAGatingFlow",
  "selectedFlow": "QAGatingFlow",
  "selectedPhase": "QA",
  "transitionType": "no_contribution",
  "domainId": "software-development",
  "capabilityGapWarnings": [
    "gap-project-planning-sprint-001",
    "gap-project-planning-burndown-001",
    "gap-project-planning-release-cycle-001"
  ],
  "nineNodeFlowEnabled": false
}
```

---

## B36-0D-006-004 — QA factory: "approve" also triggers QAGatingFlow

**Input:** "Approve the sprint completion checklist before the milestone release date."
**Token firing QA:** "approve"
**Factory:** QAGatingFlow
**Phase:** QA sub-phase
**CapabilityGapWarnings:** sprint-001, milestone-001, release-cycle-001

The same factory is selected for "approve" as for "review." Both are QA predicate tokens. Both select QAGatingFlow. The distinction between "review" and "approve" does not affect factory selection; both map to QA.
