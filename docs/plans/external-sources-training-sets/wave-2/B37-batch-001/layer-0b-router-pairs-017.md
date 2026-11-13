# Layer 0-B — Router Pairs (017)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-017-001 — lessons-learned session: [] → knowledge_edge

**Input:** "Run the lessons-learned session at the end of the project and collect feedback from all workstream leads."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-work-stream-001, gap-project-managing-manage-delivery-001

---

## B37-0B-017-002 — lessons-learned with "document" and "stakeholder": [DOCUMENT] → 2.1

**Input:** "Document the lessons learned from the delivery programme and circulate to all project stakeholders."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate (from "document" + "stakeholders") → DocumentationFlow (2.1)
- DomainId: "software-development"
- Route: 2.1

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001
- gap-project-managing-stakeholder-update-001

---

## B37-0B-017-003 — lessons-learned action items: [] → knowledge_edge

**Input:** "Track the lessons-learned action items and follow up with owners to ensure completion."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-track-001, gap-project-managing-action-items-001, gap-project-managing-follow-up-001, gap-project-managing-accountability-001

---

## B37-0B-017-004 — lessons-learned with "review" and "validate": [QA] → QA sub-phase

**Input:** "Review the lessons learned from the previous release and validate the improvement actions."

**Compiler output:** ["QA"]

**Router decision:**
- QA predicate (from "review" + "validate") → QAGatingFlow / ReviewFlow (QA sub-phase)
- DomainId: "software-development"
- Route: QA sub-phase

**Gap warnings forwarded:**
- gap-project-managing-action-items-001

---

## B37-0B-017-005 — lessons-learned with "define" and "structure": [DOCUMENT, ARCHITECTURE] → 3.1+2.1

**Input:** "Define the lessons-learned process structure for all future project deliveries."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Router decision:**
- DOCUMENT predicate (from "define") → DocumentationFlow (2.1)
- ARCHITECTURE predicate (from "structure") → ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Routes: 3.1 + 2.1 (parallel eligible)

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001
