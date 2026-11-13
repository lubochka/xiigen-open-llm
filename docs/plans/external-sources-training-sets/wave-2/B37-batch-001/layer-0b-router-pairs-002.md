# Layer 0-B — Router Pairs (002)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: router-pairs

---

## B37-0B-002-001 — [] → knowledge_edge: RAID log, zero predicates

**Input:** "Set up the RAID log for the project and track all risks, assumptions, issues, and dependencies."

**Compiler output:** []

**Router decision:**
- Predicate list is empty → no sub-phase routing possible
- DomainId: "project-management"
- Route: knowledge_edge
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-raid-log-001, gap-project-managing-track-001, gap-project-managing-dependencies-team-001
- Message: "B37 project managing vocabulary detected. No predicate matched. Routing to knowledge edge for capability gap learning."

---

## B37-0B-002-002 — [] → knowledge_edge: pure tracking request

**Input:** "Track delivery milestones and provide a weekly on-track/off-track status update."

**Compiler output:** []

**Router decision:**
- Predicate list is empty → knowledge_edge
- DomainId: "project-management"
- Route: knowledge_edge
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-track-001, gap-project-managing-track-status-001
- Message: "track and on-track/off-track are B37 gap vocabulary; no predicate fires on these tokens."

---

## B37-0B-002-003 — [] → knowledge_edge: escalation + follow-up vocabulary

**Input:** "Escalate the critical blocker to the steering committee and follow up on all open action items."

**Compiler output:** []

**Router decision:**
- Predicate list is empty → knowledge_edge
- DomainId: "project-management"
- Route: knowledge_edge
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-escalation-001, gap-project-managing-blockers-001, gap-project-managing-follow-up-001, gap-project-managing-action-items-001

---

## B37-0B-002-004 — [] → knowledge_edge: RAG-status request

**Input:** "Produce the RAG-status report for all active workstreams."

**Compiler output:** []

**Router decision:**
- Predicate list is empty → knowledge_edge
- DomainId: "project-management"
- Route: knowledge_edge
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-rag-status-001, gap-project-managing-work-stream-001, gap-project-managing-status-report-001

---

## B37-0B-002-005 — [] → knowledge_edge: accountability + coordinate

**Input:** "Coordinate delivery accountability across all workstream leads."

**Compiler output:** []

**Router decision:**
- Predicate list is empty → knowledge_edge
- DomainId: "project-management"
- Route: knowledge_edge
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-accountability-001, gap-project-managing-work-stream-001
