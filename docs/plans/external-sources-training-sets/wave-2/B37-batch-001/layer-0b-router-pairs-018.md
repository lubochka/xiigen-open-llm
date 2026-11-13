# Layer 0-B — Router Pairs (018)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-018-001 — accountability with escalation: [] → knowledge_edge

**Input:** "Establish clear accountability across all workstream leads for delivery milestones."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-accountability-001, gap-project-managing-work-stream-001, gap-project-managing-manage-delivery-001

---

## B37-0B-018-002 — accountability structure: [DOCUMENT, ARCHITECTURE] → 3.1+2.1 (key boundary)

**Input:** "Define the accountability structure for the project governance model."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Router decision:**
- DOCUMENT predicate (from "define") → DocumentationFlow (2.1)
- ARCHITECTURE predicate (from "structure") → ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Routes: 3.1 + 2.1

**Gap warnings forwarded:**
- gap-project-managing-accountability-001

**Routing note:** "accountability structure" is a governance concept but "structure" fires ARCHITECTURE. "define" fires DOCUMENT. Both predicates route. SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B37-0B-018-003 — accountability + escalation: [] → knowledge_edge

**Input:** "Coordinate accountability and escalation protocols across all programme workstreams."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-accountability-001, gap-project-managing-escalation-001, gap-project-managing-escalation-accountability-001, gap-project-managing-work-stream-001

---

## B37-0B-018-004 — accountability with "document": [DOCUMENT] → 2.1

**Input:** "Document the accountability framework and assign ownership to each delivery outcome."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate → DocumentationFlow (2.1)
- DomainId: "software-development"

**Gap warnings forwarded:**
- gap-project-managing-accountability-001

---

## B37-0B-018-005 — RACI with "build": [IMPLEMENT] → 4.1

**Input:** "Build the RACI matrix to clarify accountability for all project activities."

**Compiler output:** ["IMPLEMENT"]

**Router decision:**
- IMPLEMENT predicate (from "build") → BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Route: 4.1

**Gap warnings forwarded:**
- gap-project-managing-accountability-001

**Routing note:** "Build the RACI matrix" fires IMPLEMENT even though intent is creating a project artefact. DomainId is "software-development" because IMPLEMENT fired.
