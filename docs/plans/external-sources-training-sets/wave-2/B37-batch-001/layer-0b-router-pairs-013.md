# Layer 0-B — Router Pairs (013)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-013-001 — risk-log maintenance: [] → knowledge_edge

**Input:** "Update the risk log with all new risks identified during the sprint retrospective."

**Compiler output:** []

**Router decision:**
- Predicate list is empty → no sub-phase routing possible
- DomainId: "project-management"
- Route: knowledge_edge
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-raid-log-001
- Message: "risk log is B37 gap vocabulary (RAID-R). No predicate matched. Routing to knowledge edge."

---

## B37-0B-013-002 — risk-log with "review": [QA] → QA sub-phase

**Input:** "Review all entries in the risk log and escalate high-severity items."

**Compiler output:** ["QA"]

**Router decision:**
- QA predicate → ReviewFlow (QA sub-phase)
- DomainId: "software-development"
- Route: QA sub-phase
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Gap warnings forwarded:**
- gap-project-managing-raid-log-001
- gap-project-managing-escalation-001

---

## B37-0B-013-003 — risk-log with "document": [DOCUMENT] → 2.1

**Input:** "Document all project risks in the risk log, including probability and impact ratings."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate → DocumentationFlow (sub-phase 2.1)
- DomainId: "software-development"
- Route: 2.1

**Gap warnings forwarded:**
- gap-project-managing-raid-log-001

---

## B37-0B-013-004 — risk-log with track: [] → knowledge_edge

**Input:** "Maintain the risk log by tracking new risks and monitoring existing risk mitigations."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-raid-log-001, gap-project-managing-track-001

---

## B37-0B-013-005 — risk-log with "build": [IMPLEMENT] → 4.1

**Input:** "Build a risk log dashboard to track all open risks and their owners."

**Compiler output:** ["IMPLEMENT"]

**Router decision:**
- IMPLEMENT predicate → BackendServiceFlow41 (sub-phase 4.1)
- DomainId: "software-development"
- Route: 4.1
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Gap warnings forwarded:**
- gap-project-managing-raid-log-001
- gap-project-managing-track-001
- gap-project-managing-accountability-001
