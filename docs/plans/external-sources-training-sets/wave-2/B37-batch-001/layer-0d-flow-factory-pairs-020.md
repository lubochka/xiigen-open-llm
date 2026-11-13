# Layer 0-D — Flow Factory Pairs (020)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-020-001 — governance structure: ArchitectureFlow31 + DocumentationFlow

**Input:** "Define the governance structure for the programme delivery board."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Flow factory decision:**
- DOCUMENT → DocumentationFlow (2.1)
- ARCHITECTURE → ArchitectureFlow31 (3.1)
- Both factories selected (parallel eligible)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001

---

## B37-0D-020-002 — org structure: ArchitectureFlow31 only

**Input:** "Map out the org structure for the delivery team."

**Compiler output:** ["ARCHITECTURE"]

**Flow factory decision:**
- ARCHITECTURE → ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001, gap-project-managing-work-stream-001, gap-project-managing-accountability-001

---

## B37-0D-020-003 — no "structure": knowledge_edge

**Input:** "Coordinate escalation paths and clarify accountability for delivery workstreams."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-escalation-001, gap-project-managing-accountability-001, gap-project-managing-work-stream-001, gap-project-managing-manage-delivery-001

---

## B37-0D-020-004 — team structure: ArchitectureFlow31

**Input:** "Establish the team structure for the delivery pod."

**Compiler output:** ["ARCHITECTURE"]

**Flow factory decision:**
- ARCHITECTURE → ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001

---

## B37-0D-020-005 — Extended structure false positive: factory always ArchitectureFlow31

**Complete factory rule for "structure" in B37:**

Regardless of the type of "structure" (governance, org, team, reporting, accountability), the factory selected is always ArchitectureFlow31 (3.1).

ArchitectureFlow31 addresses software architecture design. It does NOT address:
- Governance structure
- Org design
- Team setup
- Accountability structure
- Reporting hierarchy

All of these are capability gaps. The architectural intent is not present; the factory fires on the lexical token "structure".

**SCHEDULE_CAPABILITY_LEARNING:** NOT emitted when ARCHITECTURE fires. The gap warnings capture the managing intent.
