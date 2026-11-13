# Layer 0-B — Router Pairs (004)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: router-pairs

---

## B37-0B-004-001 — [ARCHITECTURE, DOCUMENT] → 3.1 + 2.1: accountability structure

**Input:** "Define the accountability structure for the project delivery team."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT"]

**Router decision:**
- ARCHITECTURE present → route to 3.1 (ArchitectureFlow)
- DOCUMENT present → route to 2.1 (DocumentationFlow)
- DomainId: "software-development"
- Routes: 3.1 + 2.1 (parallel eligible)

**Gap warnings forwarded:**
- gap-project-managing-accountability-001
- gap-project-managing-manage-delivery-001

**Router note:** The router routes both predicates. "accountability structure" → ARCHITECTURE (structure token) + DOCUMENT (define token). The managing vocabulary is gap.

---

## B37-0B-004-002 — [ARCHITECTURE, DOCUMENT] → 3.1 + 2.1: governance structure

**Input:** "Document the governance structure and accountability framework for the programme."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT"]

**Router decision:**
- ARCHITECTURE present → 3.1
- DOCUMENT present → 2.1
- DomainId: "software-development"
- Routes: 3.1 + 2.1

**Gap warnings forwarded:**
- gap-project-managing-accountability-001

---

## B37-0B-004-003 — [ARCHITECTURE] → 3.1 only: "structure" without DOCUMENT triggers

**Input:** "Outline the escalation structure and communication hierarchy."

**Compiler output:** ["ARCHITECTURE"]

**Router decision:**
- ARCHITECTURE present → 3.1
- No DOCUMENT trigger → 2.1 not activated
- DomainId: "software-development"
- Route: 3.1

**Gap warnings forwarded:**
- gap-project-managing-escalation-001

---

## B37-0B-004-004 — [ARCHITECTURE, DOCUMENT] → 3.1 + 2.1: RACI structure

**Input:** "Define the RACI structure and document the delivery accountability matrix."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT"]

**Router decision:**
- ARCHITECTURE (structure) + DOCUMENT (define, document) → 3.1 + 2.1
- DomainId: "software-development"
- Routes: 3.1 + 2.1

**Gap warnings forwarded:**
- gap-project-managing-accountability-001
- gap-project-managing-manage-delivery-001

---

## B37-0B-004-005 — [ARCHITECTURE, DOCUMENT, QA] → 3.1 + 2.1 + QA sub-phase: three routes

**Input:** "Review the governance structure and define the escalation criteria for the delivery team."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT", "QA"]

**Router decision:**
- ARCHITECTURE (structure) → 3.1
- DOCUMENT (define, criteria) → 2.1
- QA (review) → QA sub-phase
- DomainId: "software-development"
- Routes: 3.1 + 2.1 + QA sub-phase

**Gap warnings forwarded:**
- gap-project-managing-escalation-001
- gap-project-managing-manage-delivery-001
