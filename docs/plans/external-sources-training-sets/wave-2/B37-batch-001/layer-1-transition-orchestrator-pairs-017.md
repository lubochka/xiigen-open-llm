# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-017-001 — lessons-learned: TransitionType unknown

**Input:** "Run the lessons-learned session at project end."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: gap-project-managing-work-stream-001, gap-project-managing-manage-delivery-001

---

## B37-L1-017-002 — lessons-learned action items: TransitionType unknown

**Input:** "Track lessons-learned action items and follow up with owners."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: gap-project-managing-track-001, gap-project-managing-action-items-001, gap-project-managing-follow-up-001, gap-project-managing-accountability-001

---

## B37-L1-017-003 — lessons-learned with QA: TransitionType qa_review

**Input:** "Review lessons learned and validate improvement actions."

**Compiler output:** ["QA"]

**Transition orchestrator:**
- TransitionType: qa_review
- Next state: ReviewFlow / QAGatingFlow (QA sub-phase)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-action-items-001

---

## B37-L1-017-004 — lessons-learned with DOCUMENT: TransitionType documentation

**Input:** "Document the lessons learned and circulate to stakeholders."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-stakeholder-update-001

---

## B37-L1-017-005 — lessons-learned with DOCUMENT + ARCHITECTURE: two transitions

**Input:** "Define the lessons-learned process structure for future projects."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Transition orchestrator:**
- TransitionType: documentation + architecture_design
- Next states: DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Parallel eligible: Yes
- Gap IDs: gap-project-managing-manage-delivery-001

**Orchestrator note:** "process structure" triggers both DOCUMENT (from "define") and ARCHITECTURE (from "structure"). Two transitions fire in parallel. Lessons-learned intent is not addressed by either flow.
