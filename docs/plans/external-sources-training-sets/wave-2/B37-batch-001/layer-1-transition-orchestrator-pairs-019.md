# Layer 1 — Transition Orchestrator Pairs (019)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-019-001 — stakeholder update: TransitionType documentation (false positive)

**Input:** "Send a status update to all project stakeholders."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation (DOCUMENT predicate on "stakeholders")
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-status-report-001, gap-project-managing-stakeholder-update-001, gap-project-managing-manage-delivery-001

**Orchestrator note:** TransitionType "documentation" does not match the intent of sending a status update. The intent mismatch is recorded in gap warnings.

---

## B37-L1-019-002 — escalate to stakeholders: TransitionType documentation

**Input:** "Escalate the delivery blocker to senior stakeholders."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: escalation-001, blockers-001, stakeholder-update-001

---

## B37-L1-019-003 — programme board (no stakeholder): TransitionType unknown

**Input:** "Send a status update to the programme board."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: gap-project-managing-status-report-001, gap-project-managing-manage-delivery-001

---

## B37-L1-019-004 — stakeholder requirements: TransitionType documentation

**Input:** "Gather requirements from stakeholders and track progress."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: stakeholder-update-001, track-progress-001, manage-delivery-001

---

## B37-L1-019-005 — v6-02 detection for stakeholder sentence

**v6-02 corruption:**
Input: "Send a weekly status update to stakeholders."
v6-02 corrupt output: Predicates: [] — "this is communication, not documentation; DOCUMENT suppressed."

**Detection method:**
1. Sentence contains "stakeholders"
2. Correct output must include ["DOCUMENT"] (Rule 6 is absolute)
3. Any output with "stakeholders" present and DOCUMENT absent = v6-02 corruption

**Correct output:** ["DOCUMENT"], DomainId "software-development", TransitionType: documentation.

**v6-02 detection is certain** when both conditions are met:
- Token "stakeholder" or "stakeholders" present in input
- "DOCUMENT" absent from predicate list in output
