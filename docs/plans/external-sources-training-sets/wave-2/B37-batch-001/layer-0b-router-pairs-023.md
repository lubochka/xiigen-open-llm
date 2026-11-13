# Layer 0-B — Router Pairs (023)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-023-001 — accountability structure: [DOCUMENT, ARCHITECTURE] → 3.1+2.1

**Input:** "Define the accountability structure for project governance and coordinate workstream ownership."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Router decision:**
- DOCUMENT (from "define") → DocumentationFlow (2.1)
- ARCHITECTURE (from "structure") → ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Routes: 3.1 + 2.1 (parallel eligible)

**Gap warnings forwarded:**
- gap-project-managing-accountability-001
- gap-project-managing-coordinate-001
- gap-project-managing-work-stream-001

---

## B37-0B-023-002 — accountability without structure: [] → knowledge_edge

**Input:** "Coordinate workstream ownership and clarify accountability for delivery outcomes."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-work-stream-001, gap-project-managing-accountability-001, gap-project-managing-manage-delivery-001

---

## B37-0B-023-003 — accountability + structure, no define: [ARCHITECTURE] → 3.1

**Input:** "Map the accountability structure and escalation hierarchy for the programme."

**Compiler output:** ["ARCHITECTURE"]

**Router decision:**
- ARCHITECTURE (from "structure") → ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- No DOCUMENT (no "define" or other DOCUMENT trigger)

**Gap warnings forwarded:**
- gap-project-managing-accountability-001
- gap-project-managing-escalation-001

---

## B37-0B-023-004 — three predicates: [DOCUMENT, ARCHITECTURE, IMPLEMENT] → 3.1+2.1+4.1

**Input:** "Define the accountability structure, build the escalation tracking service, and document the process."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]

**Router decision:**
- DOCUMENT → DocumentationFlow (2.1)
- ARCHITECTURE → ArchitectureFlow31 (3.1)
- IMPLEMENT → BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Routes: 3.1 + 2.1 + 4.1

**Gap warnings forwarded:**
- gap-project-managing-accountability-001
- gap-project-managing-escalation-001
- gap-project-managing-track-001

---

## B37-0B-023-005 — "design" as ARCHITECTURE trigger: [ARCHITECTURE] → 3.1

**Input:** "Design the accountability framework and coordinate delivery ownership across teams."

**Compiler output:** ["ARCHITECTURE"]

**Router decision:**
- ARCHITECTURE (from "design") → ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Route: 3.1

**Gap warnings forwarded:**
- gap-project-managing-accountability-001
- gap-project-managing-coordinate-001
- gap-project-managing-manage-delivery-001

**Note:** "design" fires ARCHITECTURE the same as "structure". "Design the accountability framework" routes to 3.1 just like "define the accountability structure" routes to 3.1+2.1.
