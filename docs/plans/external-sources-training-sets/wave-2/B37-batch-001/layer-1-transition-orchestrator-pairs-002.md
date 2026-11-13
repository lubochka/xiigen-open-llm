# Layer 1 — Transition Orchestrator Pairs (002)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-002-001 — knowledge_edge: TransitionType unknown, ContextCarryForward false

**Input:** "Set up the RAID log and track all risks, assumptions, issues, and dependencies."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown (no predicate to determine transition)
- ContextCarryForward: false (no prior flow context to carry)
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted

**Orchestrator note:** When zero predicates fire, the transition orchestrator has no sub-phase to transition to. TransitionType remains unknown because no flow has been established. The orchestrator routes to knowledge_edge and records the gap IDs for the learning pipeline.

---

## B37-L1-002-002 — knowledge_edge: pure coordinate/track request

**Input:** "Coordinate all delivery workstreams and track the weekly status."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- ContextCarryForward: false
- Next state: knowledge_edge
- DomainId: "project-management"
- Gap IDs recorded: gap-project-managing-coordinate-001, gap-project-managing-work-stream-001, gap-project-managing-track-status-001, gap-project-managing-manage-delivery-001

---

## B37-L1-002-003 — knowledge_edge: escalation vocabulary

**Input:** "Escalate critical risks and follow up on all open RAID log items."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- ContextCarryForward: false
- Next state: knowledge_edge
- DomainId: "project-management"
- Gap IDs: gap-project-managing-escalation-001, gap-project-managing-follow-up-001, gap-project-managing-raid-log-001

---

## B37-L1-002-004 — knowledge_edge from previous flow: ContextCarryForward possible

**Scenario:** User previously in DocumentationFlow (2.1), then issues managing request.

**Prior context:** DocumentationFlow active
**New input:** "Track the progress on the documentation workstream."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: gap_interruption (gap request arrived during active flow)
- ContextCarryForward: true (documentation flow context preserved)
- Next state: knowledge_edge (for gap handling)
- After gap handling: resume DocumentationFlow context
- DomainId: "project-management" (for gap request)
- Gap IDs: gap-project-managing-track-progress-001, gap-project-managing-work-stream-001

---

## B37-L1-002-005 — Orchestrator behavior: knowledge_edge does not consume flow context

**Rule:** Routing to knowledge_edge for a zero-predicate request does NOT destroy any active flow context.

If the user is mid-flow (e.g., in BackendServiceFlow41) and issues a zero-predicate managing request:
1. Orchestrator notes: gap_interruption
2. knowledge_edge handles the gap request
3. SCHEDULE_CAPABILITY_LEARNING emitted
4. BackendServiceFlow41 context preserved
5. Next request (if returning to software work) can resume the prior flow

**TransitionType for gap_interruption:** gap_interruption
**ContextCarryForward:** true (prior flow context preserved)
