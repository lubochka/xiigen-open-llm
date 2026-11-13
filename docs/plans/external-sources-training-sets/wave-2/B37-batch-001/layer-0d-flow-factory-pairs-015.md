# Layer 0-D — Flow Factory Pairs (015)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-015-001 — project-charter circulation: knowledge_edge

**Input:** "Circulate the project charter and collect sign-off from all workstream leads."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-work-stream-001, gap-project-managing-accountability-001

---

## B37-0D-015-002 — project-charter with IMPLEMENT (create): BackendServiceFlow41

**Input:** "Create the project charter for the new platform migration programme."

**Compiler output:** ["IMPLEMENT"]

**Flow factory decision:**
- IMPLEMENT predicate (from "create") → BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001

**Factory note:** "Create" fires IMPLEMENT even for document creation. BackendServiceFlow41 is selected; the project charter governance intent is NOT addressed by the factory.

---

## B37-0D-015-003 — project-charter with DOCUMENT: DocumentationFlow

**Input:** "Define the scope and objectives in the project charter."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT predicate → DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001

---

## B37-0D-015-004 — project-charter with QA: ReviewFlow

**Input:** "Review the project charter against the original scope assumptions."

**Compiler output:** ["QA"]

**Flow factory decision:**
- QA predicate → ReviewFlow (QA sub-phase)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001, gap-project-managing-raid-log-001, gap-project-managing-track-001

---

## B37-0D-015-005 — ProjectCharterFlow does NOT exist (v6-01 boundary)

**Confirmed absent from factory registry:**
- ProjectCharterFlow — does not exist
- ProgrammeGovernanceFlow — does not exist
- ProjectInitiationFlow — does not exist

For zero-predicate project charter requests: knowledge_edge (no factory).
For predicate-firing requests: predicate-appropriate factory only.
