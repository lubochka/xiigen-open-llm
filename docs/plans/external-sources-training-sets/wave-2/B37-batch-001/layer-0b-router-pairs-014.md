# Layer 0-B — Router Pairs (014)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-014-001 — change-request coordination: [] → knowledge_edge

**Input:** "Process the change request and coordinate approval from the change advisory board."

**Compiler output:** []

**Router decision:**
- Predicate list is empty → knowledge_edge
- DomainId: "project-management"
- Route: knowledge_edge
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-manage-delivery-001
- Message: "change-request coordination is B37 gap vocabulary. No predicate matched."

---

## B37-0B-014-002 — change-request with "document": [DOCUMENT] → 2.1

**Input:** "Document the change request with full impact analysis and stakeholder sign-off requirements."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate (from "document", "stakeholder", "requirements") → DocumentationFlow (2.1)
- DomainId: "software-development"
- Route: 2.1
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001
- gap-project-managing-stakeholder-update-001

---

## B37-0B-014-003 — change-request with "review": [QA] → QA sub-phase

**Input:** "Review the pending change requests in the change log before the CAB meeting."

**Compiler output:** ["QA"]

**Router decision:**
- QA predicate → ReviewFlow (QA sub-phase)
- DomainId: "software-development"
- Route: QA sub-phase

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001

---

## B37-0B-014-004 — change-request with track and escalate: [] → knowledge_edge

**Input:** "Track all open change requests and escalate any that are overdue to the project manager."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-track-001, gap-project-managing-escalation-001, gap-project-managing-manage-delivery-001

---

## B37-0B-014-005 — change-request with "implement": [IMPLEMENT] → 4.1

**Input:** "Implement the approved change request to add authentication to the user service."

**Compiler output:** ["IMPLEMENT"]

**Router decision:**
- IMPLEMENT predicate → BackendServiceFlow41 (sub-phase 4.1)
- DomainId: "software-development"
- Route: 4.1
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001
