# Cross-Layer Synthesis (008)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## B36-CLS-008-001 — Planning + QA + IMPLEMENT + DOCUMENT: full layer trace

**Request:** "Review the sprint deliverables, build the remaining features, and document the release requirements."

**Layer 0-A — Compiler:**

Token scan:
- review: QA fires
- sprint: GAP → sprint-001
- deliverables: no predicate
- build: IMPLEMENT fires
- remaining: no predicate
- features: no standalone predicate
- feature: no standalone predicate (not a code-level entity alone without "build")
- document: DOCUMENT fires
- release: GAP → release-planning gap (no predicate — "release" without a QA keyword)
- requirements: DOCUMENT fires (second trigger, same predicate)

Predicate output: `["QA", "IMPLEMENT", "DOCUMENT"]`
CapabilityGapWarnings: sprint-001, gap-project-planning-release-cycle-001

Note: "build" + "features" fires IMPLEMENT because "feature" is treated as a code entity
co-occurring with "build" in a delivery context.

---

**Layer 0-B — Router:**

predicates: ["QA", "IMPLEMENT", "DOCUMENT"]
→ Phase 4.1 (IMPLEMENT, primary)
→ Phase 2.1 (DOCUMENT, parallel with 4.1)
→ QA sub-phase (last, after 4.1 and 2.1 complete)
DomainId: "software-development" (predicates fired)
KNOWLEDGE_EDGE: false
TransitionType: feature_addition (IMPLEMENT is primary predicate)

---

**Layer 0-C — Knowledge-Edge:**

Not in KNOWLEDGE_EDGE state (three predicates fired)
Gap warnings forwarded as metadata only
SCHEDULE_CAPABILITY_LEARNING: NOT emitted

---

**Layer 0-D — Factory:**

FlowFactory.Select(IMPLEMENT) → BackendServiceFlow41 (Phase 4.1)
FlowFactory.Select(DOCUMENT) → DocumentationFlow (Phase 2.1)
FlowFactory.Select(QA) → QAGatingFlow (QA sub-phase, executionOrder: last)

Parallel: BackendServiceFlow41 and DocumentationFlow run simultaneously
Sequential constraint: QAGatingFlow runs after both complete (QA-last rule)

---

**Layer 1 — Transition Orchestrator:**

transitionType: feature_addition
executionModel: parallel_4.1_2.1_then_QA
domainId: "software-development"
contextCarryForward: true
finalHumanResult: merged from BackendServiceFlow41 + DocumentationFlow + QAGatingFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicates fired)
capabilityGapWarnings: sprint-001, gap-project-planning-release-cycle-001 (metadata only)

---

**Layer 2 — Gap Extended:**

Three predicates fired alongside two gap terms:
- sprint-001: sprint context noted in review request
- gap-project-planning-release-cycle-001: release context in documentation request

---

**Layer 3 — Teaching:**

Key rules demonstrated:
- Rule 3: "review" fires QA unconditionally in sprint context
- Rule 6: "build" + "features" fires IMPLEMENT
- Rule 5: "document", "requirements" fire DOCUMENT
- Rule 11: three predicates fire; gap warnings do not affect routing
- Rule 7: DomainId "software-development" — any predicate present
- QA-last ordering: QAGatingFlow executes after BackendServiceFlow41 and DocumentationFlow
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicates fired)
- "release" without a QA keyword fires no predicate → gap warning only

---

## B36-CLS-008-002 — Trace summary

| Stage | Output |
|-------|--------|
| Compiler | predicates: ["QA", "IMPLEMENT", "DOCUMENT"] |
| Router | Phase 4.1 + 2.1 + QA-last; DomainId: software-development |
| Factory | BackendServiceFlow41 + DocumentationFlow (parallel) + QAGatingFlow (last) |
| Orchestrator | feature_addition; 4.1 + 2.1 parallel; QA last; FinalHumanResult: populated |
| Gap extended | sprint-001, release-cycle-001 (metadata — routing not affected) |
| Teaching | Rules 3, 5, 6, 7, 11 applied; QA-last ordering demonstrated |

---

## B36-CLS-008-003 — Full B36 complexity: four-signal mixed case

This is the maximum-complexity B36 synthesis case. It demonstrates all core B36 behaviors simultaneously:
1. Three predicates firing from QA, IMPLEMENT, and DOCUMENT lexicons
2. QA-last structural ordering constraint active
3. Two gap terms producing gap warnings without affecting routing
4. Parallel execution of IMPLEMENT (4.1) and DOCUMENT (2.1)
5. QA gating flow executing last after both parallel flows complete
6. DomainId: "software-development" — any predicate fires
7. No SCHEDULE_CAPABILITY_LEARNING emitted
8. CapabilityGapWarnings reach AutonomousLearningPipeline as metadata

Compare with CLS-005 (pure gap: zero predicates, knowledge_edge) and CLS-006 (one predicate via
false positive). This case is the full B36 mixed-signal scenario.

A model that correctly handles this case has mastered:
- Predicate lexicon matching unconditional on planning context
- QA-last ordering rule
- DomainId binary rule (predicate presence → software-development)
- Gap warning propagation without routing impact
