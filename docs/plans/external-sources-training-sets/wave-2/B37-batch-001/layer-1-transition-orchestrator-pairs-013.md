# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-013-001 — risk-log: TransitionType unknown (zero predicates)

**Input:** "Maintain the risk log by tracking new risks."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown (zero predicates — no sub-phase to transition to)
- ContextCarryForward: false
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs forwarded: gap-project-managing-raid-log-001, gap-project-managing-track-001

---

## B37-L1-013-002 — risk-log with QA: TransitionType qa_review

**Input:** "Review all entries in the risk log."

**Compiler output:** ["QA"]

**Transition orchestrator:**
- TransitionType: qa_review (QA predicate)
- ContextCarryForward: false
- Next state: ReviewFlow (QA sub-phase)
- DomainId: "software-development"
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted
- Gap IDs forwarded: gap-project-managing-raid-log-001, gap-project-managing-escalation-001

---

## B37-L1-013-003 — risk-log with DOCUMENT: TransitionType documentation

**Input:** "Document all project risks in the risk log."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation (DOCUMENT predicate)
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs forwarded: gap-project-managing-raid-log-001

---

## B37-L1-013-004 — risk-log with IMPLEMENT: TransitionType feature_addition

**Input:** "Build a risk log dashboard."

**Compiler output:** ["IMPLEMENT"]

**Transition orchestrator:**
- TransitionType: feature_addition (IMPLEMENT predicate)
- Next state: BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Gap IDs forwarded: gap-project-managing-raid-log-001, gap-project-managing-track-001

---

## B37-L1-013-005 — risk-log gap_interruption scenario

**Scenario:** User in active BackendServiceFlow41 context, issues risk log request.

**Prior context:** BackendServiceFlow41 active
**New input:** "Track the risk log for the current service build."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: gap_interruption (gap request during active flow)
- ContextCarryForward: true (BackendServiceFlow41 context preserved)
- Next state: knowledge_edge (for gap handling)
- DomainId: "project-management" (for gap request)
- SCHEDULE_CAPABILITY_LEARNING: emitted
- After gap handling: BackendServiceFlow41 context resumable
- Gap IDs: gap-project-managing-raid-log-001, gap-project-managing-track-001
