# Layer 0-B — Router Pairs (010)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: router-pairs

---

## B37-0B-010-001 — [ARCHITECTURE, DOCUMENT] → 3.1 + 2.1: project governance structure

**Input:** "Define the project governance structure and document the escalation criteria for critical blockers."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT"]

**Router decision:**
- ARCHITECTURE (structure) → 3.1
- DOCUMENT (define, document, criteria) → 2.1
- DomainId: "software-development"
- Routes: 3.1 + 2.1

**Gap warnings forwarded:**
- gap-project-managing-escalation-001
- gap-project-managing-blockers-001
- gap-project-managing-accountability-001 (governance)

---

## B37-0B-010-002 — [ARCHITECTURE, DOCUMENT] → 3.1 + 2.1: RACI structure with requirements

**Input:** "Document the RACI structure requirements and define the escalation approval criteria."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT"]

**Router decision:**
- ARCHITECTURE (structure) → 3.1
- DOCUMENT (document, requirements, define, criteria) → 2.1
- DomainId: "software-development"
- Routes: 3.1 + 2.1

**Gap warnings forwarded:**
- gap-project-managing-escalation-001
- gap-project-managing-accountability-001

---

## B37-0B-010-003 — [ARCHITECTURE, DOCUMENT, QA] → 3.1 + 2.1 + QA: review governance structure

**Input:** "Review the governance structure and document the accountability criteria."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT", "QA"]

**Router decision:**
- ARCHITECTURE (structure) → 3.1
- DOCUMENT (document, criteria) → 2.1
- QA (review) → QA sub-phase
- DomainId: "software-development"
- Routes: 3.1 + 2.1 + QA sub-phase

**Gap warnings forwarded:**
- gap-project-managing-accountability-001 (governance intent not fully captured)

---

## B37-0B-010-004 — [ARCHITECTURE] → 3.1 only: "structure" without DOCUMENT triggers in governance

**Input:** "Establish the decision-making structure for the programme delivery governance."

**Compiler output:** ["ARCHITECTURE"]

**Router decision:**
- ARCHITECTURE (structure) → 3.1
- No DOCUMENT triggers present
- DomainId: "software-development"
- Route: 3.1

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001 (delivery governance intent)
- gap-project-managing-accountability-001 (decision-making = accountability)

---

## B37-0B-010-005 — [ARCHITECTURE, DOCUMENT, IMPLEMENT] → 3.1 + 2.1 + 4.1: full governance build

**Input:** "Define the governance structure, build the tracking service, and document the accountability criteria."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT"]

**Router decision:**
- ARCHITECTURE (structure) → 3.1
- DOCUMENT (define, document, criteria) → 2.1
- IMPLEMENT (build, service) → 4.1
- DomainId: "software-development"
- Routes: 3.1 + 2.1 + 4.1

**Gap warnings forwarded:**
- gap-project-managing-track-status-001 (tracking intent)
- gap-project-managing-accountability-001 (governance)
