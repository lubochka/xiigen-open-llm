# Layer 0-D — Flow Factory Pairs (023)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-023-001 — accountability structure: ArchitectureFlow31 + DocumentationFlow

**Input:** "Define the accountability structure for governance and coordinate workstream ownership."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Flow factory decision:**
- DOCUMENT → DocumentationFlow (2.1)
- ARCHITECTURE → ArchitectureFlow31 (3.1)
- Both factories, parallel eligible
- DomainId: "software-development"
- Gap warnings: accountability-001, coordinate-001, work-stream-001

---

## B37-0D-023-002 — accountability without structure: knowledge_edge

**Input:** "Coordinate workstream ownership and clarify accountability for delivery."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: coordinate-001, work-stream-001, accountability-001, manage-delivery-001

---

## B37-0D-023-003 — three predicates: three factories

**Input:** "Define the accountability structure, build the escalation tracking service, and document the process."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]

**Flow factory decision:**
- DOCUMENT → DocumentationFlow (2.1)
- ARCHITECTURE → ArchitectureFlow31 (3.1)
- IMPLEMENT → BackendServiceFlow41 (4.1)
- All three factories selected
- DomainId: "software-development"
- Gap warnings: accountability-001, escalation-001, track-001

---

## B37-0D-023-004 — design (no define): ArchitectureFlow31 only

**Input:** "Design the accountability framework and coordinate delivery ownership."

**Compiler output:** ["ARCHITECTURE"]

**Flow factory decision:**
- ARCHITECTURE → ArchitectureFlow31 (3.1)
- No DocumentationFlow (no "define" or "document" in sentence)
- DomainId: "software-development"
- Gap warnings: accountability-001, coordinate-001, manage-delivery-001

---

## B37-0D-023-005 — Extended factory map: accountability + structure patterns

**Complete factory pattern for accountability + structure vocabulary:**

| Input pattern | Predicates | Factories |
|--------------|-----------|---------|
| Accountability alone | [] | knowledge_edge (no factory) |
| Define + accountability | [DOCUMENT] | DocumentationFlow |
| Structure + accountability | [ARCHITECTURE] | ArchitectureFlow31 |
| Define + accountability structure | [DOCUMENT, ARCHITECTURE] | DocumentationFlow + ArchitectureFlow31 |
| Design + accountability | [ARCHITECTURE] | ArchitectureFlow31 |
| Build + accountability | [IMPLEMENT] | BackendServiceFlow41 |
| Review + accountability | [QA] | ReviewFlow |

Six different factory outputs for the same accountability concept — all determined by the trigger token used alongside "accountability".
