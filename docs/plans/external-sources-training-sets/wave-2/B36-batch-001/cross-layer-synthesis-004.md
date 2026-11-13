# Cross-Layer Synthesis (004)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## B36-CLS-004-001 — Full mixed signal IMPLEMENT+DOCUMENT+QA

**Request:** "Build the sprint tracker service, document the backlog requirements, review the velocity report."

**Layer 0-A — Compiler:**

Token scan:
- build: IMPLEMENT fires
- sprint: GAP → sprint-001
- tracker: no predicate
- service: IMPLEMENT (code-level entity co-occurring with "build")
- document: DOCUMENT fires
- backlog: GAP → backlog-001
- requirements: DOCUMENT fires
- review: QA fires
- velocity: GAP → velocity-001
- report: no predicate

Predicate output: `["IMPLEMENT", "DOCUMENT", "QA"]`
CapabilityGapWarnings: sprint-001, backlog-001, velocity-001

---

**Layer 0-B — Router:**

predicates: ["IMPLEMENT", "DOCUMENT", "QA"]
→ Phase 4.1 (IMPLEMENT, primary)
→ Phase 2.1 (DOCUMENT, parallel with 4.1)
→ QA sub-phase (last, after 4.1 and 2.1)
DomainId: "software-development"
KNOWLEDGE_EDGE: false
TransitionType: feature_addition (IMPLEMENT is primary)

---

**Layer 0-C — Knowledge-Edge:**

Not in KNOWLEDGE_EDGE state (three predicates fired)
Gap warnings forwarded as metadata

---

**Layer 0-D — Factory:**

FlowFactory.Select(IMPLEMENT) → BackendServiceFlow41 (Phase 4.1)
FlowFactory.Select(DOCUMENT) → DocumentationFlow (Phase 2.1)
FlowFactory.Select(QA) → QAGatingFlow (QA sub-phase, executionOrder: last)

Parallel: BackendServiceFlow41 and DocumentationFlow run simultaneously
Sequential constraint: QAGatingFlow runs after both complete

---

**Layer 1 — Transition Orchestrator:**

transitionType: feature_addition
executionModel: parallel_4.1_2.1_then_QA
domainId: "software-development"
finalHumanResult: merged from BackendServiceFlow41 + DocumentationFlow + QAGatingFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
capabilityGapWarnings: sprint-001, backlog-001, velocity-001

---

**Layer 2 — Gap Extended:**

Three predicates fired alongside three gap terms:
- sprint-001: sprint context noted in implementation request (sprint-not-captured)
- backlog-001: backlog context noted in documentation request
- velocity-001: velocity context noted in QA review request

---

**Layer 3 — Teaching:**

Key rules demonstrated:
- Rule 6: "build", "service" fire IMPLEMENT
- Rule 5: "document", "requirements" fire DOCUMENT
- Rule 3: "review" fires QA
- Rule 11: three predicates fire; three gap warnings produced; routing proceeds on all three predicates
- Rule 7: DomainId "software-development" — any predicate present
- QA ordering: QAGatingFlow executes last (structural constraint)
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicates fired)

---

## B36-CLS-004-002 — Trace summary

| Stage | Output |
|-------|--------|
| Compiler | predicates: ["IMPLEMENT", "DOCUMENT", "QA"] |
| Router | Phase 4.1 + 2.1 + QA; DomainId: software-development |
| Factory | BackendServiceFlow41 + DocumentationFlow + QAGatingFlow |
| Orchestrator | feature_addition; 4.1 and 2.1 parallel; QA last; FinalHumanResult: populated |
| Gap extended | sprint-001, backlog-001, velocity-001 (warnings — do not affect routing) |
| Teaching | Rules 3, 5, 6, 7, 11 applied; QA-last ordering demonstrated |

---

## B36-CLS-004-003 — Three-predicate synthesis: the complete B36 mixed-signal case

This is the highest-complexity B36 training case. It demonstrates simultaneously:
1. Three predicates firing from different lexicons (IMPLEMENT, DOCUMENT, QA)
2. Three gap terms producing gap warnings
3. Parallel execution of IMPLEMENT and DOCUMENT
4. QA-last ordering constraint
5. DomainId "software-development" despite heavy planning vocabulary
6. No SCHEDULE_CAPABILITY_LEARNING emitted
7. CapabilityGapWarnings reaching AutonomousLearningPipeline

A model that correctly handles this case has mastered the core B36 mixed-signal behavior.
