# Layer 0-D — Flow Factory Pairs (018)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-018-001 — accountability coordination: knowledge_edge

**Input:** "Establish accountability across all workstream leads for delivery milestones."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: gap-project-managing-accountability-001, gap-project-managing-work-stream-001, gap-project-managing-manage-delivery-001

---

## B37-0D-018-002 — accountability structure: ArchitectureFlow31 + DocumentationFlow

**Input:** "Define the accountability structure for project governance."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Flow factory decision:**
- DOCUMENT → DocumentationFlow (2.1)
- ARCHITECTURE → ArchitectureFlow31 (3.1)
- Both factories selected (parallel eligible)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-accountability-001

**Factory note:** ArchitectureFlow31 addresses software architecture design, NOT governance accountability. The accountability intent is captured only in gap warnings.

---

## B37-0D-018-003 — accountability + escalation: knowledge_edge

**Input:** "Coordinate accountability and escalation protocols across programme workstreams."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-accountability-001, gap-project-managing-escalation-001, gap-project-managing-escalation-accountability-001, gap-project-managing-work-stream-001

---

## B37-0D-018-004 — RACI with IMPLEMENT: BackendServiceFlow41

**Input:** "Build the RACI matrix for the project."

**Compiler output:** ["IMPLEMENT"]

**Flow factory decision:**
- IMPLEMENT → BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-accountability-001

---

## B37-0D-018-005 — AccountabilityFlow does NOT exist (v6-01 boundary)

**Confirmed absent from factory registry:**
- AccountabilityFlow — does not exist
- OwnershipFlow — does not exist
- RACIFlow — does not exist
- GovernanceFlow — does not exist

For zero-predicate accountability requests: knowledge_edge (no factory).
Accountability management is a capability gap; no flow exists.
