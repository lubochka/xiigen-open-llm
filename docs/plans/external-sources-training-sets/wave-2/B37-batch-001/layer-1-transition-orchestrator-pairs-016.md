# Layer 1 — Transition Orchestrator Pairs (016)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-016-001 — decision-log: TransitionType unknown

**Input:** "Update the decision log with all key decisions from the steering committee."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-accountability-001

---

## B37-L1-016-002 — decision-log DOCUMENT + ARCHITECTURE: two TransitionTypes

**Input:** "Document all architectural decisions in the decision log."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Transition orchestrator:**
- TransitionType: documentation + architecture_design (two predicates)
- ContextCarryForward: false
- Next states: DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Parallel eligible: Yes
- Gap IDs: gap-project-managing-manage-delivery-001

---

## B37-L1-016-003 — decision-log with QA: TransitionType qa_review

**Input:** "Review the decision log before the programme board."

**Compiler output:** ["QA"]

**Transition orchestrator:**
- TransitionType: qa_review
- Next state: ReviewFlow
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-action-items-001

---

## B37-L1-016-004 — decision-log with track + follow-up: TransitionType unknown

**Input:** "Track all open decisions and follow up with owners."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: gap-project-managing-track-001, gap-project-managing-follow-up-001, gap-project-managing-accountability-001, gap-project-managing-manage-delivery-001

---

## B37-L1-016-005 — v6 ordering violation: gap IDs assigned before compiler scan

**v6 ordering violation for decision log:**

v6 corrupt: Gap IDs (gap-project-managing-manage-delivery-001) assigned during intent analysis of "Update the decision log" before lexical scan completes.

**Correct order:**
1. Compiler lexical scan runs first
2. If predicate fires → DomainId "software-development" + flow routing
3. Non-predicate tokens → gap IDs assigned
4. Gap IDs forwarded as metadata

Gap ID assignment is POST-scan. Pre-assigning gap IDs based on intent classification is a v6 ordering violation regardless of whether the gap IDs are correct.
