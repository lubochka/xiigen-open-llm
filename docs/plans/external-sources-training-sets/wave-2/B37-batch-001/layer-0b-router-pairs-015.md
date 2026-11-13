# Layer 0-B — Router Pairs (015)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-015-001 — project-charter with "create": [IMPLEMENT] → 4.1

**Input:** "Create the project charter for the new platform migration programme."

**Compiler output:** ["IMPLEMENT"]

**Router decision:**
- IMPLEMENT predicate (from "create") → BackendServiceFlow41 (sub-phase 4.1)
- DomainId: "software-development"
- Route: 4.1
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001

**Note:** "create" fires IMPLEMENT even when creating a management document. The compiler applies lexical rules regardless of intent.

---

## B37-0B-015-002 — project-charter with "define": [DOCUMENT] → 2.1

**Input:** "Define the scope and objectives in the project charter before kickoff."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate → DocumentationFlow (sub-phase 2.1)
- DomainId: "software-development"
- Route: 2.1

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001

---

## B37-0B-015-003 — project-charter circulation: [] → knowledge_edge

**Input:** "Circulate the project charter and collect sign-off from all workstream leads."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-work-stream-001, gap-project-managing-accountability-001

---

## B37-0B-015-004 — project-charter with "stakeholder": [DOCUMENT] → 2.1

**Input:** "Share the project charter with all stakeholders and confirm the delivery timeline."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate (from "stakeholders") → DocumentationFlow (2.1)
- DomainId: "software-development"
- Route: 2.1

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001
- gap-project-managing-stakeholder-update-001

---

## B37-0B-015-005 — project-charter with "review": [QA] → QA sub-phase

**Input:** "Review the project charter against the original scope assumptions and track any deviations."

**Compiler output:** ["QA"]

**Router decision:**
- QA predicate (from "review") → ReviewFlow (QA sub-phase)
- DomainId: "software-development"
- Route: QA sub-phase

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001
- gap-project-managing-raid-log-001
- gap-project-managing-track-001
