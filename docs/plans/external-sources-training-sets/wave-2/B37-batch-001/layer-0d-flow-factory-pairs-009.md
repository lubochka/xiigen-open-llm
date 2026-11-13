# Layer 0-D — Flow Factory Pairs (009)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: flow-factory-pairs

---

## B37-0D-009-001 — ARCHITECTURE+DOCUMENT factory for governance sentences

**Input:** "Define the project governance structure and document the escalation criteria for critical blockers."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT"]

**Flow factory decision:**
- ARCHITECTURE (structure) → ArchitectureFlow31
- DOCUMENT (define, document, criteria) → DocumentationFlow
- Parallel factories: ArchitectureFlow31 + DocumentationFlow
- DomainId: "software-development"

**Factory advisory:** Both flows are invoked for organizational governance vocabulary. Neither flow handles escalation or blocker management. Gap warnings record the unaddressed managing intent.

**Gap warnings:**
- gap-project-managing-escalation-001
- gap-project-managing-blockers-001
- gap-project-managing-accountability-001 (governance)

---

## B37-0D-009-002 — ARCHITECTURE factory for RACI structure: governance governance context

**Input:** "Design the RACI structure and specify roles for each programme deliverable."

**Compiler output:** ["ARCHITECTURE"]

**Flow factory decision:**
- ARCHITECTURE (design, structure) → ArchitectureFlow31
- No DOCUMENT triggers ("specify" not in DOCUMENT list)
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-accountability-001 (RACI = accountability framework)

---

## B37-0D-009-003 — APIContractFlow33 + DocumentationFlow for API dep sentence

**Input:** "Document the service API dependencies and coordinate the integration release."

**Compiler output:** ["API_CONTRACT", "DOCUMENT"]

**Flow factory decision:**
- API_CONTRACT (dependencies + api + integration co-occurrence) → APIContractFlow33
- DOCUMENT (document) → DocumentationFlow
- Parallel: APIContractFlow33 + DocumentationFlow
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-coordinate-001
- gap-project-managing-manage-delivery-001

---

## B37-0D-009-004 — TestFlow5x + DocumentationFlow + BackendServiceFlow41 (three factories)

**Input:** "Test the delivery service module and document the acceptance criteria for stakeholders."

**Compiler output:** ["TEST", "IMPLEMENT", "DOCUMENT"]

**Flow factory decision:**
- TEST (test, acceptance) → TestFlow5x
- IMPLEMENT (service, module) → BackendServiceFlow41
- DOCUMENT (document, criteria, acceptance→TEST already, stakeholders) → DocumentationFlow
- Three factories: TestFlow5x + BackendServiceFlow41 + DocumentationFlow
- Ordering: BackendServiceFlow41 and DocumentationFlow parallel; TestFlow5x after BackendServiceFlow41 is complete (test the service)
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-manage-delivery-001
- gap-project-managing-stakeholder-update-001 (intent was stakeholder communication, not documentation)

---

## B37-0D-009-005 — Factory selection stability under managing framing

**Stability test:** Does changing the managing framing affect factory selection?

Request A: "Build the reporting service." → IMPLEMENT → BackendServiceFlow41
Request B: "Manage delivery — build the reporting service." → IMPLEMENT → BackendServiceFlow41
Request C: "Coordinate with the team to build the reporting service." → IMPLEMENT → BackendServiceFlow41
Request D: "Track progress while building the reporting service." → IMPLEMENT → BackendServiceFlow41

**Answer:** No. Factory selection depends on predicate, not framing. Adding "manage", "coordinate", "track" before the IMPLEMENT-triggering tokens does not change factory selection. Gap warnings are added, but the factory is unchanged.
