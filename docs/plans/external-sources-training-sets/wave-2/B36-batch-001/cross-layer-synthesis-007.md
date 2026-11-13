# Cross-Layer Synthesis (007)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## B36-CLS-007-001 — Planning + IMPLEMENT + DOCUMENT mixed: full layer trace

**Request:** "Document the backlog requirements and build the sprint feature service."

**Layer 0-A — Compiler:**

Token scan:
- document: DOCUMENT fires
- backlog: GAP → backlog-001
- requirements: DOCUMENT fires (second trigger, same predicate — no duplicate in output)
- build: IMPLEMENT fires
- sprint: GAP → sprint-001
- feature: no standalone predicate
- service: IMPLEMENT (code-level entity co-occurring with "build")

Predicate output: `["IMPLEMENT", "DOCUMENT"]`
CapabilityGapWarnings: backlog-001, sprint-001

---

**Layer 0-B — Router:**

predicates: ["IMPLEMENT", "DOCUMENT"]
→ Phase 4.1 (IMPLEMENT, primary)
→ Phase 2.1 (DOCUMENT, parallel with 4.1)
DomainId: "software-development" (predicates fired)
KNOWLEDGE_EDGE: false
TransitionType: feature_addition (IMPLEMENT is primary predicate)

---

**Layer 0-C — Knowledge-Edge:**

Not in KNOWLEDGE_EDGE state (two predicates fired)
Gap warnings forwarded as metadata only
SCHEDULE_CAPABILITY_LEARNING: NOT emitted

---

**Layer 0-D — Factory:**

FlowFactory.Select(IMPLEMENT) → BackendServiceFlow41 (Phase 4.1)
FlowFactory.Select(DOCUMENT) → DocumentationFlow (Phase 2.1)
Parallel execution: BackendServiceFlow41 and DocumentationFlow run simultaneously

---

**Layer 1 — Transition Orchestrator:**

transitionType: feature_addition
executionModel: parallel_4.1_2.1
domainId: "software-development"
contextCarryForward: true
finalHumanResult: merged from BackendServiceFlow41 + DocumentationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
capabilityGapWarnings: backlog-001, sprint-001 (metadata only)

---

**Layer 2 — Gap Extended:**

Two predicates fired alongside two gap terms:
- backlog-001: backlog context in documentation request
- sprint-001: sprint context in implementation request

---

**Layer 3 — Teaching:**

Key rules demonstrated:
- Rule 5: "document", "requirements" fire DOCUMENT
- Rule 6: "build", "service" fire IMPLEMENT
- Rule 11: two predicates fire; gap warnings produced; routing proceeds on both predicates
- Rule 7: DomainId "software-development" — any predicate present
- Parallel execution: IMPLEMENT (4.1) and DOCUMENT (2.1) run simultaneously (no QA, no ordering constraint)
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicates fired)

---

## B36-CLS-007-002 — Trace summary

| Stage | Output |
|-------|--------|
| Compiler | predicates: ["IMPLEMENT", "DOCUMENT"] |
| Router | Phase 4.1 + 2.1; DomainId: software-development |
| Factory | BackendServiceFlow41 + DocumentationFlow (parallel) |
| Orchestrator | feature_addition; parallel 4.1 + 2.1; FinalHumanResult: populated |
| Gap extended | backlog-001, sprint-001 (metadata — routing not affected) |
| Teaching | Rules 5, 6, 7, 11 applied; parallel IMPLEMENT+DOCUMENT demonstrated |

---

## B36-CLS-007-003 — Two-predicate mixed-signal: planning context with code and documentation

This synthesis demonstrates the IMPLEMENT + DOCUMENT two-predicate case in a planning context:
1. "document" + "requirements" fire DOCUMENT; "build" + "service" fire IMPLEMENT
2. Both predicates route in parallel: Phase 4.1 and Phase 2.1 run simultaneously
3. No QA — no ordering constraint applies
4. DomainId: "software-development" despite backlog and sprint gap vocabulary
5. SCHEDULE_CAPABILITY_LEARNING not emitted
6. CapabilityGapWarnings reach the AutonomousLearningPipeline but do not affect routing

Contrast with CLS-005: same sprint/backlog vocabulary, but predicates fire here — gap vocabulary
never suppresses predicates that match the lexicon.
