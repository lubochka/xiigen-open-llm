# Layer 1 — Transition Orchestrator Pairs (018)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-018-001 — accountability coordination: TransitionType unknown

**Input:** "Establish accountability across all workstream leads."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: gap-project-managing-accountability-001, gap-project-managing-work-stream-001, gap-project-managing-manage-delivery-001

---

## B37-L1-018-002 — accountability structure: architecture_design + documentation transitions

**Input:** "Define the accountability structure for project governance."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Transition orchestrator:**
- TransitionType: architecture_design + documentation (two predicates)
- Next states: ArchitectureFlow31 (3.1) + DocumentationFlow (2.1)
- DomainId: "software-development"
- Parallel eligible: Yes
- Gap IDs: gap-project-managing-accountability-001

**Orchestrator note:** "accountability structure" triggers architecture_design (from "structure"). "define" triggers documentation. Both fire. The accountability governance intent is captured in gap warnings only.

---

## B37-L1-018-003 — escalation + accountability: TransitionType unknown

**Input:** "Coordinate accountability and escalation protocols across workstreams."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: coordinate-001, accountability-001, escalation-001, escalation-accountability-001, work-stream-001

---

## B37-L1-018-004 — RACI with IMPLEMENT: TransitionType feature_addition

**Input:** "Build the RACI matrix for the project."

**Compiler output:** ["IMPLEMENT"]

**Transition orchestrator:**
- TransitionType: feature_addition
- Next state: BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-accountability-001

---

## B37-L1-018-005 — v6 ordering violation: SCL emitted when ARCHITECTURE fires

**v6 ordering violation for accountability structure:**

v6 corrupt: "Define the accountability structure" → ARCHITECTURE fires → SCHEDULE_CAPABILITY_LEARNING STILL emitted because "managing intent is present".

**Why wrong:** SCL is emitted ONLY for zero-predicate requests. ARCHITECTURE predicate fired → DomainId "software-development" → SCL NOT emitted. The presence of managing vocabulary (accountability) does not override the SCL emission rule.

Correct: ARCHITECTURE fires → SCL NOT emitted. Gap warning gap-project-managing-accountability-001 is raised instead.
