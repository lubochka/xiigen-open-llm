# Layer 1 — Transition Orchestrator Pairs (020)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-020-001 — governance structure: architecture_design + documentation

**Input:** "Define the governance structure for the programme delivery board."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Transition orchestrator:**
- TransitionType: architecture_design + documentation
- Next states: ArchitectureFlow31 (3.1) + DocumentationFlow (2.1)
- DomainId: "software-development"
- Parallel eligible: Yes
- Gap IDs: gap-project-managing-manage-delivery-001

---

## B37-L1-020-002 — org structure: architecture_design only

**Input:** "Map out the org structure for the delivery team."

**Compiler output:** ["ARCHITECTURE"]

**Transition orchestrator:**
- TransitionType: architecture_design
- Next state: ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Gap IDs: manage-delivery-001, work-stream-001, accountability-001

---

## B37-L1-020-003 — no structure (clarify escalation): TransitionType unknown

**Input:** "Clarify escalation paths and accountability levels for delivery workstreams."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: escalation-001, accountability-001, work-stream-001, manage-delivery-001

---

## B37-L1-020-004 — team structure: architecture_design

**Input:** "Establish the team structure for the delivery pod."

**Compiler output:** ["ARCHITECTURE"]

**Transition orchestrator:**
- TransitionType: architecture_design
- Next state: ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Gap IDs: manage-delivery-001

---

## B37-L1-020-005 — v6-03 detection for "structure" sentences

**v6-03 corruption:**
Input: "Map the reporting structure for workstreams."
v6-03 corrupt output: DomainId "project-management" — "reporting structure is an organisational concept, not software architecture."

**Detection method:**
1. "structure" fires ARCHITECTURE (Rule 7 — absolute)
2. Any predicate → DomainId "software-development" (Rule 14)
3. Any output with "structure" present and DomainId "project-management" = v6-03 corruption

**Correct output:** ["ARCHITECTURE"], DomainId "software-development", TransitionType: architecture_design.

DomainId is determined by the predicate scan, NOT by the intent of the sentence.
