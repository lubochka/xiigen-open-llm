# Layer 3 — Teaching Unit Pairs (012)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B36-L3-012-001 — Complete B36 teaching summary

**The 12 essential B36 teaching points:**

1. Pure project planning vocabulary (sprint, roadmap, backlog, velocity, etc.) has no registered predicate → produces KNOWLEDGE_EDGE state

2. "story" fires DOCUMENT — this is a defined false positive. The model must not suppress DOCUMENT even in estimation context.

3. "review" fires QA in any context. "retrospective" does not fire QA. These are distinct tokens with distinct lexicon status.

4. "design"/"architect"/"structure" fire ARCHITECTURE even in sprint/milestone sentences.

5. "capacity" in team planning context is gap (capacity-001). It does NOT fire ARCHITECTURE.

6. DomainId "project-management" only when predicates: []. Any predicate → "software-development".

7. SCHEDULE_CAPABILITY_LEARNING: one per request, only when predicates: []. Consumed by AutonomousLearningPipeline (subproject 16).

8. Predicate dominance: predicates fire alongside gap vocabulary → predicates determine routing; gap vocab → CapabilityGapWarnings only.

9. "update"/"generate"/"capture"/"run"/"plan" do NOT fire any predicate (planning-context versions).

10. v6 corruptions: v6-01 (PROJECT_PLANNING invented), v6-02 (predicate suppressed), v6-03 (DomainId wrong when predicate fired).

11. False positives are correct outputs. The model applies lexicon literally without semantic suppression.

12. CapabilityGapWarnings are always emitted for B36 gap vocabulary, regardless of routing path.

---

## B36-L3-012-002 — Quick reference: B36 predicate decision tree

```
For each token in request:
  Is token in IMPLEMENT lexicon? → IMPLEMENT fires
  Is token in DOCUMENT lexicon? → DOCUMENT fires
  Is token in QA lexicon? → QA fires
  Is token in ARCHITECTURE lexicon? → ARCHITECTURE fires
  Is token in B36 gap vocabulary? → emit CapabilityGapWarning for that token
  Otherwise → neutral, no action

After scanning all tokens:
  predicates == [] AND gap_vocabulary_present → KNOWLEDGE_EDGE, emit SCHEDULE_CAPABILITY_LEARNING
  predicates != [] → normal routing; DomainId: "software-development"
  predicates == [] AND no_gap_vocabulary → KNOWLEDGE_EDGE, DomainId: "project-management" (default)
```

---

## B36-L3-012-003 — B36 evaluation criteria

A model trained on B36 batch-001 should pass these evaluations:

1. Zero-predicate requests with planning vocabulary → KNOWLEDGE_EDGE, project-management, SCHEDULE_CAPABILITY_LEARNING
2. "review" in planning sentence → QA fires, software-development, no KNOWLEDGE_EDGE
3. "story" in estimation sentence → DOCUMENT fires, software-development, story-point-001 warning
4. "capacity" in team context → gap warning, no ARCHITECTURE
5. Any predicate fires → DomainId: software-development (never project-management)
6. Multiple gap terms in one request → one SCHEDULE_CAPABILITY_LEARNING with all IDs bundled
7. No PROJECT_PLANNING predicate ever emitted
8. QA always last when co-occurring with IMPLEMENT and DOCUMENT

---

## B36-L3-012-004 — B36 batch-001 scope summary

This batch teaches the AdaptiveLogicalOrchestrator to:
- Recognize the B36 project planning capability gap
- Correctly classify planning vocabulary as gap (not as new predicates)
- Apply predicate dominance in mixed-signal requests
- Emit precise gap IDs and signals for AutonomousLearningPipeline consumption
- Avoid all three v6 corruption patterns specific to B36

The batch does NOT teach the orchestrator to handle project planning requests. That capability is a future development item, tracked via the SCHEDULE_CAPABILITY_LEARNING signals accumulated from this batch.
