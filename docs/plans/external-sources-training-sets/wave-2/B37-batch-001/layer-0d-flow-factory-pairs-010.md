# Layer 0-D — Flow Factory Pairs (010)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: flow-factory-pairs

---

## B37-0D-010-001 — TEST+DOCUMENT+QA factory: three flows for estimation sentence

**Input:** "Test the delivery estimates with the stakeholders and validate the timeline assumptions."

**Compiler output:** ["TEST", "DOCUMENT", "QA"]

**Flow factory decision:**
- TEST (test) → TestFlow5x
- DOCUMENT (stakeholders) → DocumentationFlow
- QA (validate) → QAGatingFlow
- Three factories: TestFlow5x + DocumentationFlow + QAGatingFlow
- Ordering: TestFlow5x may precede QAGatingFlow (test before gating)
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-manage-delivery-001 (delivery estimates not captured)
- gap-project-managing-track-status-001 (timeline assumptions not captured)
- gap-project-managing-stakeholder-update-001 (stakeholder notification intent)
- gap-project-managing-raid-log-001 (assumptions = RAID-A)

**Factory note:** Three factories firing from a pure project managing request. The request intent (testing estimates with stakeholders) is entirely managing vocabulary in spirit, but three tokens accidentally fire predicates.

---

## B37-0D-010-002 — TEST+DOCUMENT factory: test + stakeholder

**Input:** "Test the assumptions with stakeholders and document the outcomes."

**Compiler output:** ["TEST", "DOCUMENT"]

**Flow factory decision:**
- TEST (test) → TestFlow5x
- DOCUMENT (stakeholders, document) → DocumentationFlow
- Parallel: TestFlow5x + DocumentationFlow
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-raid-log-001 (assumptions = RAID-A)
- gap-project-managing-stakeholder-update-001

---

## B37-0D-010-003 — TEST factory only: "test" without other predicates

**Input:** "Test whether the escalation process is meeting the team's expectations."

**Compiler output:** ["TEST"]

**Flow factory decision:**
- TEST (test) → TestFlow5x
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-escalation-001

---

## B37-0D-010-004 — QA factory: validate in delivery context

**Input:** "Validate the delivery timeline and confirm the on-track status."

**Compiler output:** ["QA"]

**Flow factory decision:**
- QA (validate) → QAGatingFlow
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-manage-delivery-001
- gap-project-managing-track-status-001

---

## B37-0D-010-005 — Complete B37 flow factory map: canonical reference

**B37 flow factory complete reference:**

| Predicate(s) | Factory/Factories | Parallel? |
|-------------|-----------------|---------|
| [] | None (knowledge_edge) | N/A |
| [DOCUMENT] | DocumentationFlow | N/A |
| [ARCHITECTURE] | ArchitectureFlow31 | N/A |
| [ARCHITECTURE, DOCUMENT] | ArchitectureFlow31 + DocumentationFlow | Yes |
| [IMPLEMENT] | BackendServiceFlow41 | N/A |
| [IMPLEMENT, DOCUMENT] | BackendServiceFlow41 + DocumentationFlow | Yes |
| [API_CONTRACT] | APIContractFlow33 | N/A |
| [API_CONTRACT, DOCUMENT] | APIContractFlow33 + DocumentationFlow | Yes |
| [QA] (review) | ReviewFlow | N/A |
| [QA] (validate) | QAGatingFlow | N/A |
| [QA, DOCUMENT] | QAGatingFlow + DocumentationFlow | Yes |
| [TEST] | TestFlow5x | N/A |
| [TEST, DOCUMENT] | TestFlow5x + DocumentationFlow | Yes |
| [TEST, DOCUMENT, QA] | TestFlow5x + DocumentationFlow + QAGatingFlow | Ordered (T before QA) |
| [TEST, IMPLEMENT, DOCUMENT] | TestFlow5x + BackendServiceFlow41 + DocumentationFlow | Ordered (I before T) |
