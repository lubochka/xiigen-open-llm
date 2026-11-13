# Layer 0-B — Router Pairs (016)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-016-001 — decision-log update: [] → knowledge_edge

**Input:** "Update the decision log with all key decisions made during the steering committee meeting."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-accountability-001
- Message: "decision log is B37 gap vocabulary (project governance). No predicate matched."

---

## B37-0B-016-002 — decision-log with "document" and "architectural": [DOCUMENT, ARCHITECTURE] → 3.1+2.1

**Input:** "Document all architectural decisions in the decision log with rationale and alternatives considered."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Router decision:**
- DOCUMENT predicate → DocumentationFlow (2.1)
- ARCHITECTURE predicate → ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Routes: 3.1 + 2.1 (parallel eligible)
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001

---

## B37-0B-016-003 — decision-log with track and follow-up: [] → knowledge_edge

**Input:** "Track all open decisions in the decision log and follow up with owners on outstanding items."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-track-001, gap-project-managing-follow-up-001, gap-project-managing-accountability-001, gap-project-managing-manage-delivery-001

---

## B37-0B-016-004 — decision-log with "review": [QA] → QA sub-phase

**Input:** "Review the decision log before the programme board and confirm all decisions have been actioned."

**Compiler output:** ["QA"]

**Router decision:**
- QA predicate → ReviewFlow (QA sub-phase)
- DomainId: "software-development"

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001
- gap-project-managing-action-items-001

---

## B37-0B-016-005 — decision-log with "define": [DOCUMENT] → 2.1

**Input:** "Define the decision-making process and log all decisions in the decision log."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate (from "define") → DocumentationFlow (2.1)
- DomainId: "software-development"

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001
- gap-project-managing-accountability-001
