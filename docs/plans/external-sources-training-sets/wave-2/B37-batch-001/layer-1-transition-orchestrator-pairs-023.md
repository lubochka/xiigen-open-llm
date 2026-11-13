# Layer 1 — Transition Orchestrator Pairs (023)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-023-001 — accountability structure: architecture_design + documentation

**Input:** "Define the accountability structure for governance and coordinate workstream ownership."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Transition orchestrator:**
- TransitionType: architecture_design + documentation
- Next states: ArchitectureFlow31 (3.1) + DocumentationFlow (2.1)
- DomainId: "software-development"
- Parallel eligible: Yes
- Gap IDs: accountability-001, coordinate-001, work-stream-001

---

## B37-L1-023-002 — accountability without structure: TransitionType unknown

**Input:** "Coordinate workstream ownership and clarify accountability."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: coordinate-001, work-stream-001, accountability-001, manage-delivery-001

---

## B37-L1-023-003 — three predicates: three transitions

**Input:** "Define the accountability structure, build the tracking service, and document the process."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]

**Transition orchestrator:**
- TransitionType: documentation + architecture_design + feature_addition (three predicates)
- Next states: DocumentationFlow (2.1) + ArchitectureFlow31 (3.1) + BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Parallel eligible: Yes (all three in parallel)
- Gap IDs: accountability-001, track-001, escalation-001

---

## B37-L1-023-004 — design (ARCHITECTURE only): architecture_design

**Input:** "Design the accountability framework and coordinate delivery ownership."

**Compiler output:** ["ARCHITECTURE"]

**Transition orchestrator:**
- TransitionType: architecture_design
- Next state: ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Gap IDs: accountability-001, coordinate-001, manage-delivery-001

---

## B37-L1-023-005 — Extended v6 corruption summary for 013-024

**Three v6 corruption types applied to extended vocabulary (013-024):**

**v6-01 (invent predicate):**
Example: "Process the change request" → Predicates: ["MANAGEMENT"] (invented)
Correct: Predicates: []

**v6-02 (suppress DOCUMENT):**
Example: "Send update to stakeholders" → Predicates: [] (suppressed)
Correct: Predicates: ["DOCUMENT"]

**v6-03 (wrong DomainId):**
Example: "Define the accountability structure" → DomainId: "project-management" (wrong)
Correct: DomainId: "software-development" (ARCHITECTURE + DOCUMENT fired)

All three corruptions apply to extended vocabulary exactly as to base vocabulary. The rules are unchanged.
