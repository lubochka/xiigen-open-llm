# Layer 0-B — Router Pairs (003)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: router

---

## B36-0B-003-001 — [QA] → QA routing (velocity review — "review" fires QA)

**Input predicate list:** `["QA"]`
**Request origin:** "Review the team velocity from the last sprint and plan the retrospective agenda."

**Router decision tree:**

```
predicates == ["QA"] ?
  YES →
    HAS_QA: true
    → route to QA sub-phase
    → TransitionType: no_contribution
    → DomainId: "software-development"
    → CapabilityGapWarnings forwarded (velocity, sprint, retrospective)
    → Nine-node flow does NOT execute for gap vocabulary
    → QA sub-phase executes for "review" token
```

**Router output:**
- Path selected: QA routing
- Sub-phase selected: QA gate
- ContextCarryForward: false (standalone QA request)
- DomainId: "software-development" (QA fired)
- CapabilityGapWarnings: velocity-001, sprint-001, retrospective-001

---

## B36-0B-003-002 — [QA] routing: planning context does not suppress QA

**Input predicate list:** `["QA"]`
**Request origin:** "Review the sprint burndown report."

**Router decision:**
- "review" fires QA → route to QA
- The fact that the sentence is about sprint burndown (gap vocabulary) does NOT redirect to KNOWLEDGE_EDGE
- Gap vocabulary never overrides a fired predicate
- DomainId: "software-development"

**Anti-pattern (v6-02):**
Router wrongly checks "is this a planning sentence?" and redirects to KNOWLEDGE_EDGE when predicates fired. This is v6-02 corruption. Predicates dominate. Always.

---

## B36-0B-003-003 — [QA] → QA: approve in release planning

**Input predicate list:** `["QA"]`
**Request origin:** "Approve the sprint completion checklist before the milestone release date."

**Router decision:**
- "approve" fires QA → QA sub-phase
- CapabilityGapWarnings: sprint-001, milestone-001, release-cycle-001
- DomainId: "software-development"
- TransitionType: no_contribution

---

## B36-0B-003-004 — contrast: "retrospective" alone → knowledge_edge vs "review" → QA

| Request | Predicate | Router path |
|---------|-----------|-------------|
| "Plan the retrospective agenda" | [] | KNOWLEDGE_EDGE |
| "Review the retrospective outcomes" | [QA] | QA sub-phase |
| "Facilitate the retrospective" | [] | KNOWLEDGE_EDGE |
| "Review the sprint results" | [QA] | QA sub-phase |

**Key teaching:** The token "retrospective" alone never fires a predicate. The token "review" always fires QA. The combination of both in one sentence ("review the retrospective") fires QA — because "review" fires regardless of what follows it.
