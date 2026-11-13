# Layer 2 — Capability Gap Extended Pairs (007)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B36-L2-007-001 — "story points" fires DOCUMENT: full mixed-signal trace with CapabilityGapWarning

**Input:** "Estimate the story points for each backlog item in the sprint."

**Stage 1 — Compiler:**
- estimate: GAP → estimation-001
- story: DOCUMENT lexicon fires DOCUMENT (Rule 2)
- points: no predicate
- backlog: GAP → backlog-001
- item: no predicate
- sprint: GAP → sprint-001

**Predicate output:** ["DOCUMENT"]

**Stage 2 — Router:**
- DOCUMENT → Phase 2.1
- KNOWLEDGE_EDGE: false
- DomainId: "software-development"
- CapabilityGapWarnings: story-point-001, estimation-001, backlog-001, sprint-001

**Stage 3 — Factory:**
- DocumentationFlow selected

**Stage 4 — Orchestrator:**
- TransitionType: documentation
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted (DOCUMENT fired)
- FinalHumanResult: populated by DocumentationFlow
- CapabilityGapWarnings forwarded to AutonomousLearningPipeline

---

## B36-L2-007-002 — what the user wanted vs what routing produced

**User intent:** Estimate (gap) story points (gap) for backlog (gap) sprint items.
**Routing produced:** Documentation phase execution.

**Why this gap matters:**
The user wanted estimation assistance. They got documentation. The CapabilityGapWarnings note this mismatch. The AutonomousLearningPipeline receives signals indicating that users requesting story point estimation are being routed to documentation — an inaccurate match.

This is why the false-positive on "story" fires DOCUMENT even though it serves the user poorly. The compiler is literal. The gap signals accumulate to drive future capability development.

---

## B36-L2-007-003 — story-point-001 gap in mixed-signal context

gap-project-planning-story-point-001 in the CapabilityGapWarnings of a DOCUMENT-routed request means:
- The user wanted story point estimation
- DOCUMENT fired (on "story") and routed to documentation
- The estimation capability gap is noted but not resolved by the documentation routing

The gap is still reported even though a predicate fired. CapabilityGapWarnings do not require KNOWLEDGE_EDGE state — they are emitted whenever gap vocabulary appears, regardless of routing.

---

## B36-L2-007-004 — CapabilityGapWarning structure in mixed-signal case

```json
{
  "capabilityGapWarnings": [
    {
      "gapId": "gap-project-planning-story-point-001",
      "term": "story points",
      "context": "estimation request",
      "predicate_fired": "DOCUMENT",
      "note": "false_positive: story fires DOCUMENT; user intent was estimation"
    },
    {
      "gapId": "gap-project-planning-estimation-001",
      "term": "estimate",
      "context": "effort estimation"
    },
    {
      "gapId": "gap-project-planning-backlog-001",
      "term": "backlog"
    },
    {
      "gapId": "gap-project-planning-sprint-001",
      "term": "sprint"
    }
  ]
}
```
