# Layer 1 — Transition Orchestrator Pairs (015)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-015-001 — project-charter circulation: TransitionType unknown

**Input:** "Circulate the project charter for sign-off."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-work-stream-001, gap-project-managing-accountability-001

---

## B37-L1-015-002 — project-charter with IMPLEMENT: TransitionType feature_addition

**Input:** "Create the project charter for the programme."

**Compiler output:** ["IMPLEMENT"]

**Transition orchestrator:**
- TransitionType: feature_addition (IMPLEMENT on "create")
- Next state: BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001

---

## B37-L1-015-003 — project-charter with DOCUMENT: TransitionType documentation

**Input:** "Define the scope in the project charter."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001

---

## B37-L1-015-004 — project-charter with stakeholder + DOCUMENT: TransitionType documentation

**Input:** "Share the project charter with stakeholders and confirm the delivery timeline."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-stakeholder-update-001

---

## B37-L1-015-005 — project-charter with QA: TransitionType qa_review

**Input:** "Review the project charter against the original scope."

**Compiler output:** ["QA"]

**Transition orchestrator:**
- TransitionType: qa_review
- Next state: ReviewFlow (QA sub-phase)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-track-001, gap-project-managing-raid-log-001
